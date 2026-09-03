using UnityEngine;

namespace BoxerP0
{
    public sealed class OpponentBoxer : MonoBehaviour
    {
        private readonly TimedActionState _action = new();

        private PlayerBoxer _player;
        private Phase0Telemetry _telemetry;
        private Transform _leftGlove;
        private Transform _rightGlove;
        private SphereCollider _headCollider;
        private SphereCollider _bodyCollider;
        private SphereCollider _leftGuardCollider;
        private SphereCollider _rightGuardCollider;
        private Vector3 _leftGuardLocal;
        private Vector3 _rightGuardLocal;
        private float _nextAttackTime;
        private bool _bodyAttack;
        private bool _resolvedThisAttack;
        private uint _rng = 0xC0FFEEu;

        private const float CommitSeconds = 0.34f;
        private const float ExtendSeconds = 0.17f;
        private const float RecoverSeconds = 0.48f;

        public bool CounterWindowOpen => _action.CounterWindowOpen;
        public string ActionLabel => _action.IsBusy ? $"{_action.Intent}:{_action.Phase}" : "READING";

        public void Initialize(
            PlayerBoxer player,
            Phase0Telemetry telemetry,
            Transform leftGlove,
            SphereCollider leftGuardCollider,
            Transform rightGlove,
            SphereCollider rightGuardCollider,
            SphereCollider headCollider,
            SphereCollider bodyCollider)
        {
            _player = player;
            _telemetry = telemetry;
            _leftGlove = leftGlove;
            _leftGuardCollider = leftGuardCollider;
            _rightGlove = rightGlove;
            _rightGuardCollider = rightGuardCollider;
            _headCollider = headCollider;
            _bodyCollider = bodyCollider;
            _leftGuardLocal = leftGlove.localPosition;
            _rightGuardLocal = rightGlove.localPosition;
            _nextAttackTime = Time.time + 1.2f;
        }

        private void Update()
        {
            if (_player == null) return;

            FacePlayer();
            if (!_action.IsBusy && Time.time >= _nextAttackTime)
            {
                StartAttack();
            }

            UpdateAttack();
        }

        private void FacePlayer()
        {
            Vector3 toward = _player.transform.position - transform.position;
            toward.y = 0f;
            if (toward.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(toward.normalized, Vector3.up);
            }
        }

        private void StartAttack()
        {
            int selection = NextInt(0, 4);
            PunchIntent intent = selection switch
            {
                0 => PunchIntent.Jab,
                1 => PunchIntent.Cross,
                2 => PunchIntent.Hook,
                _ => PunchIntent.Cross
            };
            _bodyAttack = selection == 3;
            if (_action.TryStart(intent))
            {
                _resolvedThisAttack = false;
                _telemetry?.RecordEvent(_bodyAttack ? "OPPONENT_COMMIT_BODY" : $"OPPONENT_COMMIT_{intent}");
            }
        }

        private void UpdateAttack()
        {
            ActionPhase prior = _action.Phase;
            _action.Step(Time.deltaTime, CommitSeconds, ExtendSeconds, RecoverSeconds);
            if (prior != _action.Phase && _action.Phase == ActionPhase.Guard)
            {
                _nextAttackTime = Time.time + NextFloat(0.65f, 1.2f);
            }

            Transform active = _action.Intent == PunchIntent.Cross ? _rightGlove : _leftGlove;
            Transform passive = active == _leftGlove ? _rightGlove : _leftGlove;
            Vector3 activeGuard = active == _leftGlove ? _leftGuardLocal : _rightGuardLocal;
            Vector3 passiveGuard = passive == _leftGlove ? _leftGuardLocal : _rightGuardLocal;
            passive.localPosition = Vector3.Lerp(passive.localPosition, passiveGuard, 14f * Time.deltaTime);

            if (!_action.IsBusy)
            {
                _leftGlove.localPosition = Vector3.Lerp(_leftGlove.localPosition, _leftGuardLocal, 14f * Time.deltaTime);
                _rightGlove.localPosition = Vector3.Lerp(_rightGlove.localPosition, _rightGuardLocal, 14f * Time.deltaTime);
                return;
            }

            Vector3 commitPose = activeGuard + new Vector3(active == _leftGlove ? -0.10f : 0.10f, 0.08f, 0.18f);
            Vector3 worldTarget = _bodyAttack
                ? _player.transform.TransformPoint(new Vector3(0f, 1.02f, 0.03f))
                : _player.transform.TransformPoint(new Vector3(0f, 1.62f, 0.03f));
            Vector3 targetLocal = transform.InverseTransformPoint(worldTarget);

            switch (_action.Phase)
            {
                case ActionPhase.Commit:
                    active.localPosition = Vector3.Lerp(activeGuard, commitPose, Smooth01(_action.NormalizedPhase(CommitSeconds)));
                    break;
                case ActionPhase.Extend:
                    active.localPosition = Vector3.Lerp(commitPose, targetLocal, Smooth01(_action.NormalizedPhase(ExtendSeconds)));
                    if (!_resolvedThisAttack && _action.NormalizedPhase(ExtendSeconds) >= 0.78f)
                    {
                        ResolveOpponentAttack(activeGuard, targetLocal);
                        _resolvedThisAttack = true;
                    }
                    break;
                case ActionPhase.Recover:
                    active.localPosition = Vector3.Lerp(targetLocal, activeGuard, Smooth01(_action.NormalizedPhase(RecoverSeconds)));
                    break;
            }
        }

        private void ResolveOpponentAttack(Vector3 localStart, Vector3 localEnd)
        {
            Vector3 start = transform.TransformPoint(localStart);
            Vector3 end = transform.TransformPoint(localEnd);
            CombatOutcome outcome = _player.ResolveOpponentPunch(start, end, 0.075f, _bodyAttack);
            _telemetry?.RecordOutcome("OPPONENT", outcome, false);
            BoxerFeedback.Emit(outcome);
        }

        public CombatOutcome ResolveIncomingPunch(Vector3 start, Vector3 end, float punchRadius)
        {
            float leftRadius = _leftGuardCollider.radius * MaxScale(_leftGuardCollider.transform);
            float rightRadius = _rightGuardCollider.radius * MaxScale(_rightGuardCollider.transform);
            if (!CounterWindowOpen &&
                (CombatGeometry.SegmentSphereIntersects(start, end, _leftGlove.position, punchRadius + leftRadius) ||
                 CombatGeometry.SegmentSphereIntersects(start, end, _rightGlove.position, punchRadius + rightRadius)))
            {
                return CombatOutcome.Block;
            }

            Vector3 headCenter = _headCollider.transform.TransformPoint(_headCollider.center);
            float headRadius = _headCollider.radius * MaxScale(_headCollider.transform);
            if (CombatGeometry.SegmentSphereIntersects(start, end, headCenter, punchRadius + headRadius))
            {
                return CombatOutcome.Hit;
            }

            Vector3 bodyCenter = _bodyCollider.transform.TransformPoint(_bodyCollider.center);
            float bodyRadius = _bodyCollider.radius * MaxScale(_bodyCollider.transform);
            return CombatGeometry.SegmentSphereIntersects(start, end, bodyCenter, punchRadius + bodyRadius)
                ? CombatOutcome.Hit
                : CombatOutcome.Miss;
        }

        private int NextInt(int minInclusive, int maxExclusive)
        {
            _rng = 1664525u * _rng + 1013904223u;
            uint range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(_rng % range);
        }

        private float NextFloat(float min, float max)
        {
            _rng = 1664525u * _rng + 1013904223u;
            float t = (_rng & 0x00FFFFFFu) / 16777215f;
            return Mathf.Lerp(min, max, t);
        }

        private static float Smooth01(float t) => t * t * (3f - 2f * t);

        private static float MaxScale(Transform value)
        {
            Vector3 scale = value.lossyScale;
            return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        }
    }
}
