using System.Globalization;
using UnityEngine;

namespace BoxerP0
{
    public sealed class OpponentBoxer : MonoBehaviour
    {
        private readonly TimedActionState _action = new();
        private readonly P1CounterOpportunityState _counterOpportunity = new();

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
        private Vector3 _attackTargetLocal;
        private float _attackReachMeters;
        private bool _attackWasClamped;
        private float _nextAttackTime;
        private bool _bodyAttack;
        private bool _resolvedThisAttack;
        private Vector3 _targetCenterAtCommit;
        private Vector3 _playerRootAtCommit;
        private float _playerHeadOffsetAtCommit;
        private uint _rng = 0xC0FFEEu;
        private P1OpponentAttributeSet _attributes = P1OpponentAttributes.Resolve(P1OpponentProfile.Balanced);

        public Transform LeftGlove => _leftGlove;
        public Transform RightGlove => _rightGlove;
        public SphereCollider HeadCollider => _headCollider;
        public SphereCollider BodyCollider => _bodyCollider;
        public bool BodyAttack => _bodyAttack;
        public bool Round2Resolved => _resolvedThisAttack;
        public bool CounterWindowOpen => _counterOpportunity.IsOpen(_action.Phase);
        public string CounterOpportunityLabel => _counterOpportunity.Label(_action.Phase);
        public bool CombatEnabled { get; private set; } = true;
        public string ActionLabel => _action.IsBusy ? $"{PunchLabels.Display(_action.Intent)}:{_action.Phase}" : "READING";
        public uint AttackEventCount { get; private set; }

        // P1-B1.5: Visual embodiment read-only access
        public bool IsActionBusy => _action.IsBusy;
        public PunchIntent CurrentIntent => _action.IsBusy ? _action.Intent : PunchIntent.None;
        public ActionPhase CurrentPhase => _action.Phase;
        public float ActionNormalizedPhase(float phaseDuration) => _action.NormalizedPhase(phaseDuration);
        public float CurrentActionPhaseDuration => PhaseDuration(_action.Phase);
        public Vector3 AttackTargetLocal => _attackTargetLocal;
        public string AttributeProfileLabel => P1OpponentAttributes.ProfileToken(_attributes.Profile);
        public string AttributeInspectorText => _attributes.ToInspectorText();

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

        public void ConfigureAttributes(P1OpponentProfile profile)
        {
            _attributes = P1OpponentAttributes.Resolve(profile);
            _telemetry?.RecordEvent(
                $"P1_D_PROFILE PROFILE={AttributeProfileLabel} REACH_X={F(_attributes.ReachFactor)} GAP_X={F(_attributes.AttackGapFactor)} DURATION_X={F(_attributes.PhaseDurationFactor)}");
        }

        private void Update()
        {
            if (_player == null) return;

            UpdateSpacing();
            FacePlayer();
            if (!CombatEnabled)
            {
                ReturnToGuard();
                return;
            }

            if (!_action.IsBusy && Time.time >= _nextAttackTime && Vector3.Distance(transform.position, _player.transform.position) <= Round2Motion.BoxingBoundary)
            {
                StartAttack();
            }

            UpdateAttack();
        }

        public void SetCombatEnabled(bool enabled)
        {
            CombatEnabled = enabled;
            if (!enabled)
            {
                _action.ResetToGuard();
                _resolvedThisAttack = false;
                _counterOpportunity.Clear();
            }
            else
            {
                _nextAttackTime = Time.time + 0.6f;
            }
        }

        private void FacePlayer()
        {
            // P1-B1 fairness: once a punch commits it must not home/rotate after a retreating player.
            if (_action.IsBusy) return;

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
                2 => PunchIntent.LeadHook,
                _ => PunchIntent.Cross
            };
            _bodyAttack = selection == 3;
            if (_action.TryStart(intent))
            {
                _counterOpportunity.Clear();
                LockAttackTarget(intent);
                AttackEventCount++;
                _resolvedThisAttack = false;
                string baseEvent = _bodyAttack
                    ? "OPPONENT_COMMIT_BODY"
                    : $"OPPONENT_PUNCH_{PunchLabels.EventToken(intent)}";
                _telemetry?.RecordEvent(
                    $"{baseEvent} REACH={F(_attackReachMeters)} CLAMPED={(_attackWasClamped ? 1 : 0)}");
            }
        }

        private void LockAttackTarget(PunchIntent intent)
        {
            Transform active = ActiveGlove(intent);
            Vector3 guardLocal = active == _leftGlove ? _leftGuardLocal : _rightGuardLocal;
            Vector3 startWorld = transform.TransformPoint(guardLocal);
            Vector3 desiredWorld = _bodyAttack
                ? _player.transform.TransformPoint(new Vector3(0f, 1.02f, 0.03f))
                : _player.transform.TransformPoint(new Vector3(0f, 1.62f, 0.03f));

            _attackReachMeters = _bodyAttack ? _attributes.BodyReachMeters : _attributes.HeadReachMeters;
            Vector3 clampedWorld = OpponentReachMath.ClampEndpoint(startWorld, desiredWorld, _attackReachMeters);
            _attackWasClamped = (clampedWorld - desiredWorld).sqrMagnitude > 0.000001f;
            Vector3 aim = transform.InverseTransformPoint(desiredWorld);
            Vector3 family = Round2Motion.Endpoint(intent, "NEUTRAL", Vector3.Distance(transform.position, _player.transform.position));
            family.z *= _attributes.ReachFactor;
            // Capture once. Neither orientation, root nor aim follows the target after commit.
            _attackTargetLocal = new Vector3(Mathf.Clamp(aim.x, -0.15f, 0.15f), _bodyAttack ? 1.14f : family.y, Mathf.Min(aim.z, family.z));
            _targetCenterAtCommit = CurrentPlayerTargetCenter();
            _playerRootAtCommit = _player.transform.position;
            _playerHeadOffsetAtCommit = _player.HeadOffset;
        }

        private void UpdateAttack()
        {
            ActionPhase prior = _action.Phase;
            _action.Step(Time.deltaTime, _attributes.CommitSeconds, _attributes.ExtendSeconds, _attributes.RecoverSeconds);
            if (prior != _action.Phase && _action.Phase == ActionPhase.Guard)
            {
                _counterOpportunity.Clear();
                _nextAttackTime = Time.time + NextFloat(_attributes.AttackGapMinSeconds, _attributes.AttackGapMaxSeconds);
            }

        }

        public void CompleteRound2Attack(CombatOutcome outcome, string reason, Vector3 start, Vector3 end)
        {
            if (!CombatEnabled) return;
            if (_resolvedThisAttack) return;
            _resolvedThisAttack = true;
            P1CounterOpportunity opportunity = P1CounterGeometry.Evaluate(
                start,
                end,
                _targetCenterAtCommit,
                CurrentPlayerTargetCenter(),
                Round2Motion.GloveRadius + CurrentPlayerTargetRadius(),
                _bodyAttack,
                _player.HeadOffset - _playerHeadOffsetAtCommit,
                PlanarDistance(_playerRootAtCommit, _player.transform.position),
                outcome);
            _counterOpportunity.Arm(opportunity);
            if (opportunity.Armed)
            {
                _telemetry?.RecordEvent(opportunity.ToSemanticEvent());
            }
            _telemetry?.RecordOutcome("OPPONENT", outcome, false, reason);
            BoxerFeedback.Emit(outcome);
        }

        public bool ConsumeCounterOpportunity()
        {
            return _counterOpportunity.Consume(_action.Phase);
        }

        public CombatOutcome ResolveIncomingPunch(Vector3 start, Vector3 end, float punchRadius)
        {
            return ResolveIncomingPunch(start, end, punchRadius, out _);
        }

        public CombatOutcome ResolveIncomingPunch(Vector3 start, Vector3 end, float punchRadius, out string reason)
        {
            if (!CombatEnabled)
            {
                reason = "COMBAT_DISABLED";
                return CombatOutcome.Miss;
            }

            float leftRadius = _leftGuardCollider.radius * MaxScale(_leftGuardCollider.transform);
            float rightRadius = _rightGuardCollider.radius * MaxScale(_rightGuardCollider.transform);
            if (!CounterWindowOpen &&
                (CombatGeometry.SegmentSphereIntersects(start, end, _leftGlove.position, punchRadius + leftRadius) ||
                 CombatGeometry.SegmentSphereIntersects(start, end, _rightGlove.position, punchRadius + rightRadius)))
            {
                reason = "OPPONENT_GUARD_INTERSECTION";
                return CombatOutcome.Block;
            }

            Vector3 headCenter = _headCollider.transform.TransformPoint(_headCollider.center);
            float headRadius = _headCollider.radius * MaxScale(_headCollider.transform);
            if (CombatGeometry.SegmentSphereIntersects(start, end, headCenter, punchRadius + headRadius))
            {
                reason = "OPPONENT_HEAD_INTERSECTION";
                return CombatOutcome.Hit;
            }

            Vector3 bodyCenter = _bodyCollider.transform.TransformPoint(_bodyCollider.center);
            float bodyRadius = _bodyCollider.radius * MaxScale(_bodyCollider.transform);
            if (CombatGeometry.SegmentSphereIntersects(start, end, bodyCenter, punchRadius + bodyRadius))
            {
                reason = "OPPONENT_BODY_INTERSECTION";
                return CombatOutcome.Hit;
            }

            reason = "NO_OPPONENT_TARGET_INTERSECTION";
            return CombatOutcome.Miss;
        }

        private Transform ActiveGlove(PunchIntent intent)
        {
            return PunchLabels.IsRearHand(intent) ? _rightGlove : _leftGlove;
        }

        private Vector3 CurrentPlayerTargetCenter()
        {
            SphereCollider target = _bodyAttack ? _player.BodyCollider : _player.HeadCollider;
            return target.transform.TransformPoint(target.center);
        }

        private float CurrentPlayerTargetRadius()
        {
            SphereCollider target = _bodyAttack ? _player.BodyCollider : _player.HeadCollider;
            return target.radius * MaxScale(target.transform);
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private void ReturnToGuard()
        {
            // Round2CombatRig owns both guard glove transforms.
        }

        private void UpdateSpacing()
        {
            if (!CombatEnabled || _action.IsBusy) return;
            Vector3 delta = _player.transform.position - transform.position;
            delta.y = 0f;
            float distance = delta.magnitude;
            if (distance < 0.001f) return;
            Vector3 direction = delta / distance;
            float desired = Round2Motion.CloseBoundary + Round2Motion.HeadRadius;
            if(EVContactSurface.Ready)desired=EVContactSurface.PreferredDistance;
            float speed = distance > desired + 0.045f ? 0.36f : distance < desired - 0.045f ? -0.32f : 0f;
            Vector3 lateral = Vector3.Cross(Vector3.up, direction) * (Mathf.Sin(Time.time * 0.8f) * 0.10f);
            Vector3 next = transform.position + (direction * speed + lateral) * Time.deltaTime;
            next.x = Mathf.Clamp(next.x, -2.0f, 2.0f);
            next.z = Mathf.Clamp(next.z, -0.15f, 2.25f);
            transform.position = next;
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

        private static string F(float value) => value.ToString("F3", CultureInfo.InvariantCulture);
        private static float Smooth01(float t) => t * t * (3f - 2f * t);

        private float PhaseDuration(ActionPhase phase)
        {
            return phase switch
            {
                ActionPhase.Commit => _attributes.CommitSeconds,
                ActionPhase.Extend => _attributes.ExtendSeconds,
                ActionPhase.Recover => _attributes.RecoverSeconds,
                _ => 1f
            };
        }

        private static float MaxScale(Transform value)
        {
            Vector3 scale = value.lossyScale;
            return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        }
    }
}
