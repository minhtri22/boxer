using UnityEngine;

namespace BoxerP0
{
    public sealed class PlayerBoxer : MonoBehaviour
    {
        private readonly TimedActionState _action = new();

        private BoxerInput _input;
        private OpponentBoxer _opponent;
        private Phase0Telemetry _telemetry;
        private Transform _head;
        private Transform _leftGlove;
        private Transform _rightGlove;
        private SphereCollider _headCollider;
        private SphereCollider _bodyCollider;
        private SphereCollider _leftGuardCollider;
        private SphereCollider _rightGuardCollider;
        private Vector3 _leftGuardLocal;
        private Vector3 _rightGuardLocal;
        private float _headVelocity;
        private bool _resolvedThisPunch;

        private const float CommitSeconds = 0.09f;
        private const float ExtendSeconds = 0.14f;
        private const float RecoverSeconds = 0.28f;

        public float HeadOffset { get; private set; }
        public bool GuardActive => !_action.IsBusy;
        public bool CombatEnabled { get; private set; } = true;
        public string ActionLabel => _action.IsBusy ? $"{PunchLabels.Display(_action.Intent)}:{_action.Phase}" : "GUARD";
        public Transform Head => _head;
        public Transform LeftGlove => _leftGlove;
        public Transform RightGlove => _rightGlove;
        public SphereCollider HeadCollider => _headCollider;
        public SphereCollider BodyCollider => _bodyCollider;
        public SphereCollider LeftGuardCollider => _leftGuardCollider;
        public SphereCollider RightGuardCollider => _rightGuardCollider;

        public void Initialize(
            BoxerInput input,
            OpponentBoxer opponent,
            Phase0Telemetry telemetry,
            Transform head,
            SphereCollider headCollider,
            SphereCollider bodyCollider,
            Transform leftGlove,
            SphereCollider leftGuardCollider,
            Transform rightGlove,
            SphereCollider rightGuardCollider)
        {
            _input = input;
            _opponent = opponent;
            _telemetry = telemetry;
            _head = head;
            _headCollider = headCollider;
            _bodyCollider = bodyCollider;
            _leftGlove = leftGlove;
            _leftGuardCollider = leftGuardCollider;
            _rightGlove = rightGlove;
            _rightGuardCollider = rightGuardCollider;
            _leftGuardLocal = leftGlove.localPosition;
            _rightGuardLocal = rightGlove.localPosition;
            _input.PunchRequested += OnPunchRequested;
        }

        private void Update()
        {
            if (_input == null) return;

            UpdateFootwork();
            FaceOpponent();
            UpdateHead();
            UpdatePunch();
        }

        public void SetCombatEnabled(bool enabled)
        {
            CombatEnabled = enabled;
            if (!enabled)
            {
                _action.ResetToGuard();
                _resolvedThisPunch = false;
            }
        }

        private void UpdateFootwork()
        {
            Vector2 intent = _input.MovementIntent;
            Vector3 delta = new Vector3(intent.x, 0f, intent.y) * (1.5f * Time.deltaTime);
            transform.position += delta;
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -2.25f, 2.25f);
            p.z = Mathf.Clamp(p.z, -1.3f, -0.05f);
            transform.position = p;
        }

        private void FaceOpponent()
        {
            if (_opponent == null) return;
            Vector3 toward = _opponent.transform.position - transform.position;
            toward.y = 0f;
            if (toward.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(toward.normalized, Vector3.up);
            }
        }

        private void UpdateHead()
        {
            float target = HeadMotionMath.ResolveOffset(_input.HeadAngleDegrees);
            HeadOffset = Mathf.SmoothDamp(HeadOffset, target, ref _headVelocity, 0.055f, 8f, Time.deltaTime);
            Vector3 local = _head.localPosition;
            local.x = HeadOffset;
            _head.localPosition = local;
        }

        private void OnPunchRequested(PunchIntent intent)
        {
            string token = PunchLabels.EventToken(intent);
            if (!CombatEnabled)
            {
                _telemetry?.RecordEvent($"PLAYER_BOUT_LOCK_REJECT_{token}");
                return;
            }

            if (_action.TryStart(intent))
            {
                _resolvedThisPunch = false;
                _telemetry?.RecordEvent($"PLAYER_PUNCH_{token}");
            }
            else
            {
                _telemetry?.RecordEvent($"PLAYER_SPAM_REJECT_{token}");
            }
        }

        private void UpdatePunch()
        {
            ActionPhase previousPhase = _action.Phase;
            _action.Step(Time.deltaTime, CommitSeconds, ExtendSeconds, RecoverSeconds);
            if (previousPhase != _action.Phase && _action.Phase == ActionPhase.Extend)
            {
                _resolvedThisPunch = false;
            }

            Transform active = ActiveGlove(_action.Intent);
            Transform passive = active == _leftGlove ? _rightGlove : _leftGlove;

            if (!_action.IsBusy)
            {
                _leftGlove.localPosition = Vector3.Lerp(_leftGlove.localPosition, _leftGuardLocal, 18f * Time.deltaTime);
                _rightGlove.localPosition = Vector3.Lerp(_rightGlove.localPosition, _rightGuardLocal, 18f * Time.deltaTime);
                return;
            }

            Vector3 activeGuard = active == _leftGlove ? _leftGuardLocal : _rightGuardLocal;
            Vector3 passiveGuard = passive == _leftGlove ? _leftGuardLocal : _rightGuardLocal;
            passive.localPosition = Vector3.Lerp(passive.localPosition, passiveGuard, 18f * Time.deltaTime);

            Vector3 commitPose = activeGuard + new Vector3(active == _leftGlove ? -0.08f : 0.08f, -0.06f, -0.08f);
            Vector3 targetPose = PunchTargetLocal(_action.Intent, active == _leftGlove);

            switch (_action.Phase)
            {
                case ActionPhase.Commit:
                    active.localPosition = Vector3.Lerp(activeGuard, commitPose, _action.NormalizedPhase(CommitSeconds));
                    break;
                case ActionPhase.Extend:
                    active.localPosition = Vector3.Lerp(commitPose, targetPose, Smooth01(_action.NormalizedPhase(ExtendSeconds)));
                    if (!_resolvedThisPunch && _action.NormalizedPhase(ExtendSeconds) >= 0.72f)
                    {
                        ResolvePlayerPunch(active, activeGuard, targetPose);
                        _resolvedThisPunch = true;
                    }
                    break;
                case ActionPhase.Recover:
                    active.localPosition = Vector3.Lerp(targetPose, activeGuard, Smooth01(_action.NormalizedPhase(RecoverSeconds)));
                    break;
            }
        }

        private void ResolvePlayerPunch(Transform glove, Vector3 localStart, Vector3 localEnd)
        {
            if (!CombatEnabled || _opponent == null) return;
            Vector3 start = transform.TransformPoint(localStart);
            Vector3 end = transform.TransformPoint(localEnd);
            CombatOutcome outcome = _opponent.ResolveIncomingPunch(start, end, 0.09f);
            bool counter = outcome == CombatOutcome.Hit && _opponent.CounterWindowOpen;
            _telemetry?.RecordOutcome("PLAYER", outcome, counter);
            BoxerFeedback.Emit(outcome);
        }

        public CombatOutcome ResolveOpponentPunch(Vector3 start, Vector3 end, float punchRadius, bool bodyAttack)
        {
            if (!CombatEnabled) return CombatOutcome.Miss;

            float leftRadius = _leftGuardCollider.radius * MaxScale(_leftGuardCollider.transform);
            float rightRadius = _rightGuardCollider.radius * MaxScale(_rightGuardCollider.transform);
            if (!bodyAttack &&
                (CombatGeometry.SegmentSphereIntersects(start, end, _leftGlove.position, punchRadius + leftRadius) ||
                 CombatGeometry.SegmentSphereIntersects(start, end, _rightGlove.position, punchRadius + rightRadius)))
            {
                return CombatOutcome.Block;
            }

            SphereCollider target = bodyAttack ? _bodyCollider : _headCollider;
            float targetRadius = target.radius * MaxScale(target.transform);
            Vector3 center = target.transform.TransformPoint(target.center);
            return CombatGeometry.SegmentSphereIntersects(start, end, center, punchRadius + targetRadius)
                ? CombatOutcome.Hit
                : CombatOutcome.Miss;
        }

        private Transform ActiveGlove(PunchIntent intent)
        {
            return intent switch
            {
                PunchIntent.Cross => _rightGlove,
                PunchIntent.RearHook => _rightGlove,
                _ => _leftGlove
            };
        }

        private static Vector3 PunchTargetLocal(PunchIntent intent, bool left)
        {
            return intent switch
            {
                PunchIntent.Jab => new Vector3(-0.10f, 1.46f, 1.26f),
                PunchIntent.Cross => new Vector3(0.04f, 1.44f, 1.34f),
                PunchIntent.LeadHook => new Vector3(0.11f, 1.39f, 1.05f),
                PunchIntent.RearHook => new Vector3(-0.11f, 1.39f, 1.05f),
                _ => new Vector3(left ? -0.22f : 0.22f, 1.38f, 0.48f)
            };
        }

        private static float Smooth01(float t) => t * t * (3f - 2f * t);

        private static float MaxScale(Transform value)
        {
            Vector3 scale = value.lossyScale;
            return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.PunchRequested -= OnPunchRequested;
            }
        }
    }
}
