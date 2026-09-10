using UnityEngine;

namespace BoxerP0
{
    public readonly struct ArmChainSolution
    {
        public readonly Vector3 Shoulder;
        public readonly Vector3 Elbow;
        public readonly Vector3 Wrist;
        public readonly float UpperLength;
        public readonly float ForearmLength;
        public readonly bool Clamped;

        public ArmChainSolution(Vector3 shoulder, Vector3 elbow, Vector3 wrist, float upperLength, float forearmLength, bool clamped)
        {
            Shoulder = shoulder;
            Elbow = elbow;
            Wrist = wrist;
            UpperLength = upperLength;
            ForearmLength = forearmLength;
            Clamped = clamped;
        }
    }

    /// <summary>
    /// Pure visual two-bone arm solver used by P1-B1.5T. Combat geometry remains authoritative elsewhere.
    /// </summary>
    public static class ArmChainMath
    {
        public static ArmChainSolution Solve(
            Vector3 shoulder,
            Vector3 requestedWrist,
            Vector3 polePoint,
            float upperArmLength,
            float forearmLength)
        {
            float maxReach = Mathf.Max(0.001f, upperArmLength + forearmLength);
            float minReach = Mathf.Max(0.001f, Mathf.Abs(upperArmLength - forearmLength) + 0.001f);
            Vector3 toRequested = requestedWrist - shoulder;
            float requestedDistance = toRequested.magnitude;
            Vector3 axis = requestedDistance > 0.0001f ? toRequested / requestedDistance : Vector3.forward;
            float solvedDistance = Mathf.Clamp(requestedDistance, minReach, maxReach - 0.001f);
            bool clamped = !Mathf.Approximately(solvedDistance, requestedDistance);
            Vector3 wrist = shoulder + axis * solvedDistance;

            float x = (upperArmLength * upperArmLength - forearmLength * forearmLength + solvedDistance * solvedDistance) /
                      (2f * solvedDistance);
            float hSq = Mathf.Max(0f, upperArmLength * upperArmLength - x * x);
            float h = Mathf.Sqrt(hSq);

            Vector3 pole = polePoint - shoulder;
            Vector3 bend = pole - axis * Vector3.Dot(pole, axis);
            if (bend.sqrMagnitude < 0.0001f)
            {
                bend = Vector3.Cross(axis, Vector3.up);
                if (bend.sqrMagnitude < 0.0001f) bend = Vector3.Cross(axis, Vector3.right);
            }
            bend.Normalize();

            Vector3 elbow = shoulder + axis * x + bend * h;
            return new ArmChainSolution(shoulder, elbow, wrist, upperArmLength, forearmLength, clamped);
        }

        public static float ElbowAngleDegrees(ArmChainSolution solution)
        {
            Vector3 a = solution.Shoulder - solution.Elbow;
            Vector3 b = solution.Wrist - solution.Elbow;
            return Vector3.Angle(a, b);
        }
    }

    /// <summary>
    /// P1-B1.5U visual-only anatomical chain with correct topology and natural boxing guard:
    /// SHOULDER → UPPER ARM → ELBOW JOINT → FOREARM → GLOVE
    /// Exactly TWO limb segments: UPPER ARM + FOREARM. Elbow is a JOINT only.
    /// Original glove transforms/colliders remain untouched and authoritative for combat.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class ArmVisualEmbodiment : MonoBehaviour
    {
        [Header("Anthropometric Constants (Frozen)")]
        [SerializeField] private bool _enableDebugVisuals = false;
        [SerializeField] private bool _showCombatTraceDebug = false;
        [SerializeField] private float _shoulderWidth = 0.38f;
        [SerializeField] private float _upperArmLength = 0.34f;
        [SerializeField] private float _forearmLength = 0.31f;
        [SerializeField] private float _jointRadius = 0.075f;
        [SerializeField] private float _upperArmRadius = 0.072f;  // Thicker than forearm for readability
        [SerializeField] private float _forearmRadius = 0.056f;   // Slightly thinner
        [SerializeField] private float _visualGloveRadius = 0.115f;

        // Frozen anthropometric constants
        public const float UpperArmLength = 0.34f;
        public const float ForearmLength = 0.31f;
        public const float MaxVisualReach = UpperArmLength + ForearmLength; // 0.65f
        
        public float UpperArmLengthProp => UpperArmLength;
        public float ForearmLengthProp => ForearmLength;
        public float MaxVisualReachProp => MaxVisualReach;

        // P1-B1.5V: Natural Boxing Guard Parameters (Frozen, refined from 5U)
        [Header("P1-B1.5V Natural Boxing Guard Parameters (Frozen)")]
        [SerializeField] private float _opponentGloveHeightOffset = 0.16f;      // Glove height above chest (raised toward face)
        [SerializeField] private float _opponentGloveForwardOffset = 0.16f;     // Glove forward from chest
        [SerializeField] private float _opponentGloveLateralInset = 0.16f;      // Glove inward from shoulder (tighter guard)
        [SerializeField] private float _opponentElbowInwardBias = 0.10f;        // Elbow inward from shoulder
        [SerializeField] private float _opponentElbowHeightOffset = -0.08f;     // Elbow dropped toward ribs
        [SerializeField] private float _opponentLeftRightHeightDiff = 0.04f;    // Lead hand higher than rear
        [SerializeField] private float _opponentElbowInwardBiasGuard = 0.16f;   // Elbow tucked close to torso in guard
        [SerializeField] private float _opponentElbowForwardBias = 0.04f;       // Elbow slightly forward of shoulder

        private PlayerBoxer _playerBoxer;
        private OpponentBoxer _opponentBoxer;

        private VisualArm _playerLeft;
        private VisualArm _playerRight;
        private VisualArm _opponentLeft;
        private VisualArm _opponentRight;

        private Vector3 _playerLeftGuardLocal;
        private Vector3 _playerRightGuardLocal;
        private Vector3 _opponentLeftGuardLocal;
        private Vector3 _opponentRightGuardLocal;

        private PunchIntent _playerIntent;
        private PunchIntent _opponentIntent;
        private ActionPhase _playerPhase;
        private ActionPhase _opponentPhase;
        private bool _playerBusy;
        private bool _opponentBusy;
        private bool _playerHasSnapshot;
        private string _playerStep = "NEUTRAL";
        private float _playerDistance = 1f;
        private Vector3 _opponentTargetLocal;
        private P1BodyRotationPose _opponentBodyRotation;
        private P1WeightTransferPose _opponentWeightTransfer;

        // Anthropometric constants - frozen per P1-B1.5T
        private const float BodyHeight = 1.8f;
        private const float ShoulderHeight = 1.43f;
        private const float ShoulderForward = 0.02f;
        private const float ShoulderHeightRatio = ShoulderHeight / BodyHeight; // ~0.794
        private const float ShoulderWidthRatio = 0.38f / BodyHeight; // ~0.211
        private const float UpperArmRatio = 0.34f / BodyHeight; // ~0.189
        private const float ForearmRatio = 0.31f / BodyHeight; // ~0.172
        private const float TotalArmRatio = 0.65f / BodyHeight; // ~0.361
        private const float ShoulderWidthRatio2 = 0.38f / BodyHeight;

        private sealed class VisualArm
        {
            public Transform Root;
            public Transform ShoulderJoint;
            public Transform UpperArm;
            public Transform ElbowJoint;
            public Transform Forearm;
            public Transform VisualGlove;
            public Transform OriginalGlove;
            public bool Left;
        }

        public void Initialize(PlayerBoxer playerBoxer, OpponentBoxer opponentBoxer, Phase0Telemetry telemetry)
        {
            _playerBoxer = playerBoxer;
            _opponentBoxer = opponentBoxer;

            _playerLeft = BuildArm("Player Left", playerBoxer.transform, playerBoxer.LeftGlove, true, true);
            _playerRight = BuildArm("Player Right", playerBoxer.transform, playerBoxer.RightGlove, false, true);

            Transform opponentRoot = opponentBoxer.transform;
            Transform oppLeftGlove = opponentRoot.Find("Opponent Left Glove");
            Transform oppRightGlove = opponentRoot.Find("Opponent Right Glove");
            _opponentLeft = BuildArm("Opponent Left", opponentRoot, oppLeftGlove, true, false);
            _opponentRight = BuildArm("Opponent Right", opponentRoot, oppRightGlove, false, false);

            _playerLeftGuardLocal = playerBoxer.LeftGlove.localPosition;
            _playerRightGuardLocal = playerBoxer.RightGlove.localPosition;
            if (oppLeftGlove != null) _opponentLeftGuardLocal = oppLeftGlove.localPosition;
            if (oppRightGlove != null) _opponentRightGuardLocal = oppRightGlove.localPosition;
        }

        public void SetDeveloperDebugVisible(bool visible)
        {
            _enableDebugVisuals = visible;
            _showCombatTraceDebug = visible;
        }

        private VisualArm BuildArm(string prefix, Transform root, Transform originalGlove, bool left, bool player)
        {
            // Shoulder X is relative to the character's own left/right, regardless of root rotation
            float localX = left ? -_shoulderWidth : _shoulderWidth;

            VisualArm arm = new()
            {
                Root = root,
                Left = left,
                OriginalGlove = originalGlove
            };

            // SHOULDER JOINT - small sphere at shoulder socket
            arm.ShoulderJoint = CreateSphere(prefix + " Shoulder Joint", root, Skin,
                new Vector3(localX, ShoulderHeight, ShoulderForward), _jointRadius * 1.08f);

            // UPPER ARM - single capsule from shoulder to elbow
            arm.UpperArm = CreateCapsule(prefix + " Upper Arm", root, Skin, _upperArmRadius);

            // ELBOW JOINT - small sphere at elbow (JOINT ONLY, not a limb segment)
            arm.ElbowJoint = CreateSphere(prefix + " Elbow Joint", root, ElbowColor, Vector3.zero, _jointRadius);

            // FOREARM - single capsule from elbow to wrist
            arm.Forearm = CreateCapsule(prefix + " Forearm", root, Skin, _forearmRadius);

            // VISUAL GLOVE - end effector at wrist
            arm.VisualGlove = CreateSphere(prefix + " Visual Glove", root,
                player ? PlayerGlove : OpponentGlove, Vector3.zero, _visualGloveRadius);

            if (originalGlove != null)
            {
                HideOriginalGlove(originalGlove);
            }

            return arm;
        }

        /// <summary>
        /// Hides the authoritative combat glove and any decorations (gold cuff/panel)
        /// added by BoxerVisualShell. Called every frame to defeat the ordering issue
        /// where BoxerVisualShell.Start() re-adds visible children after Initialize().
        /// </summary>
        private static void HideOriginalGlove(Transform originalGlove)
        {
            if (originalGlove == null) return;
            foreach (Renderer renderer in originalGlove.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = false;
        }

        private void Update()
        {
            if (_playerBoxer == null || _opponentBoxer == null) return;
            CacheState();
            UpdatePlayerArms();
            UpdateOpponentArms();
            // Re-hide original gloves every frame to defeat late decoration ordering.
            HideOriginalGlove(_playerLeft?.OriginalGlove);
            HideOriginalGlove(_playerRight?.OriginalGlove);
            HideOriginalGlove(_opponentLeft?.OriginalGlove);
            HideOriginalGlove(_opponentRight?.OriginalGlove);
            // Debug visuals are OFF by default (_enableDebugVisuals = false)
            if (_enableDebugVisuals) DrawDebugVisuals();
        }

        private void CacheState()
        {
            _playerBusy = _playerBoxer.IsActionBusy;
            _playerIntent = _playerBoxer.CurrentIntent;
            _playerPhase = _playerBoxer.CurrentPhase;
            _playerHasSnapshot = _playerBoxer.HasP1PunchSnapshot;
            if (_playerHasSnapshot)
            {
                _playerStep = _playerBoxer.P1PunchSnapshot.StepState;
                _playerDistance = _playerBoxer.P1PunchSnapshot.DistanceMeters;
            }

            _opponentBusy = _opponentBoxer.IsActionBusy;
            _opponentIntent = _opponentBoxer.CurrentIntent;
            _opponentPhase = _opponentBoxer.CurrentPhase;
            _opponentTargetLocal = _opponentBoxer.AttackTargetLocal;
            float bodyPhaseT = _opponentBusy
                ? _opponentBoxer.ActionNormalizedPhase(_opponentBoxer.CurrentActionPhaseDuration)
                : 0f;
            _opponentBodyRotation = P1BodyRotationMath.Sample(_opponentIntent, _opponentPhase, bodyPhaseT);
            _opponentWeightTransfer = P1WeightTransferMath.Sample(_opponentIntent, _opponentPhase, bodyPhaseT);
        }

        private void UpdatePlayerArms()
        {
            bool activeLeft = IsLeadHand(_playerIntent);
            VisualArm active = activeLeft ? _playerLeft : _playerRight;
            VisualArm passive = activeLeft ? _playerRight : _playerLeft;
            Vector3 activeGuard = activeLeft ? _playerLeftGuardLocal : _playerRightGuardLocal;
            Vector3 passiveGuard = activeLeft ? _playerRightGuardLocal : _playerLeftGuardLocal;

            if (!_playerBusy)
            {
                PoseArm(active, activeGuard, PunchFamily.None, ActionPhase.Guard, true);
                PoseArm(passive, passiveGuard, PunchFamily.None, ActionPhase.Guard, true);
                return;
            }

            Vector3 target = GetPlayerPunchTargetLocal(_playerIntent, activeLeft);
            Vector3 commit = GetPlayerPunchCommitPose(_playerIntent, activeGuard, activeLeft);
            if (_playerHasSnapshot)
            {
                target = P1PunchMechanics.ApplyA1StraightReach(_playerIntent, target, _playerStep);
                target = P1PunchMechanics.ApplyA3FamilyCoupling(_playerIntent, target, _playerDistance);
                target = P1PunchMechanics.ApplyA32UppercutDrive(_playerIntent, commit, target, _playerStep);
            }

            Vector3 desired = PhaseTarget(activeGuard, commit, target, _playerPhase,
                _playerBoxer.ActionNormalizedPhase(_playerBoxer.CurrentActionPhaseDuration));
            PoseArm(active, desired, PunchLabels.Family(_playerIntent), _playerPhase, true);
            PoseArm(passive, passiveGuard, PunchFamily.None, ActionPhase.Guard, true);
        }

        private void UpdateOpponentArms()
        {
            ApplyOpponentShoulderRotation(_opponentLeft);
            ApplyOpponentShoulderRotation(_opponentRight);

            bool activeLeft = IsLeadHand(_opponentIntent);
            VisualArm active = activeLeft ? _opponentLeft : _opponentRight;
            VisualArm passive = activeLeft ? _opponentRight : _opponentLeft;
            Vector3 activeGuard = activeLeft ? _opponentLeftGuardLocal : _opponentRightGuardLocal;
            Vector3 passiveGuard = activeLeft ? _opponentRightGuardLocal : _opponentLeftGuardLocal;

            if (!_opponentBusy)
            {
                // P1-B1.5U: Use natural boxing guard pose for idle opponent
                PoseArmOpponentGuard(active, activeGuard, activeLeft, false);
                PoseArmOpponentGuard(passive, passiveGuard, !activeLeft, false);
                return;
            }

            PunchFamily family = PunchLabels.Family(_opponentIntent);
            Vector3 commit = OpponentCommitPose(activeGuard, family, activeLeft);
            Vector3 desired = PhaseTarget(activeGuard, commit, _opponentTargetLocal, _opponentPhase,
                _opponentBoxer.ActionNormalizedPhase(_opponentBoxer.CurrentActionPhaseDuration));
            PoseArm(active, desired, family, _opponentPhase, false);
            PoseArm(passive, passiveGuard, PunchFamily.None, ActionPhase.Guard, false);
        }

        private void ApplyOpponentShoulderRotation(VisualArm arm)
        {
            if (arm == null || arm.ShoulderJoint == null) return;
            float localX = arm.Left ? -_shoulderWidth : _shoulderWidth;
            float effectiveTorsoYaw = P1StraightBodyCouplingMath.EffectiveTorsoYawDegrees(
                _opponentIntent,
                _opponentBodyRotation,
                _opponentWeightTransfer);
            Vector3 neutralShoulder = new(
                localX,
                ShoulderHeight,
                ShoulderForward + _opponentWeightTransfer.VisualForwardOffsetMeters);
            arm.ShoulderJoint.localPosition = P1BodyRotationMath.RotateLocalYaw(
                neutralShoulder, effectiveTorsoYaw);
        }

        private void PoseArm(VisualArm arm, Vector3 requestedWristLocal, PunchFamily family, ActionPhase phase, bool player)
        {
            if (arm == null || arm.Root == null) return;
            Vector3 shoulder = arm.ShoulderJoint.position;
            Vector3 requestedWrist = arm.Root.TransformPoint(requestedWristLocal);
            Vector3 poleLocal = FamilyPoleLocal(family, phase, arm.Left, player);
            Vector3 poleWorld = shoulder + arm.Root.TransformDirection(poleLocal);
            ArmChainSolution solved = ArmChainMath.Solve(shoulder, requestedWrist, poleWorld, UpperArmLength, ForearmLength);

            // Update joint positions
            arm.ShoulderJoint.position = solved.Shoulder;
            arm.ElbowJoint.position = solved.Elbow;
            arm.VisualGlove.position = solved.Wrist;

            // Place segments using explicit endpoints - each segment from its exact endpoints
            SetSegmentBetween(arm.UpperArm, solved.Shoulder, solved.Elbow, _upperArmRadius);
            SetSegmentBetween(arm.Forearm, solved.Elbow, solved.Wrist, _forearmRadius);
        }

        private void PoseArmOpponentGuard(VisualArm arm, Vector3 guardLocal, bool left, bool player)
        {
            // P1-B1.5V: Natural boxing guard pose for opponent.
            // Tighter, less flared guard: elbows tucked forward/inward toward ribs,
            // gloves raised toward the face with a natural lead/rear asymmetry.
            
            Vector3 shoulder = arm.ShoulderJoint.position;
            
            // Adjust for left/right
            float leftRightSign = left ? -1f : 1f;
            
            // Shoulder position (already set by BuildArm)
            Vector3 shoulderPos = shoulder;
            
            // Glove position in guard: raised toward face, forward, tucked inward
            Vector3 gloveTargetLocal = new Vector3(
                leftRightSign * (_shoulderWidth - _opponentGloveLateralInset), 
                _opponentGloveHeightOffset, 
                _opponentGloveForwardOffset
            );
            
            // Lead/rear height difference (lead slightly higher)
            float heightOffset = (left ? 1f : -1f) * _opponentLeftRightHeightDiff * 0.5f;
            gloveTargetLocal.y += heightOffset;
            
            // Elbow position: tucked inward and slightly forward (toward ribs), dropped below shoulder
            Vector3 elbowTargetLocal = new Vector3(
                leftRightSign * (_shoulderWidth - _opponentElbowInwardBias - _opponentElbowInwardBiasGuard), 
                _opponentElbowHeightOffset, 
                _opponentElbowForwardBias
            );
            elbowTargetLocal.y += heightOffset;
            
            // Update joint positions
            arm.ShoulderJoint.position = arm.ShoulderJoint.position; // Already at shoulder
            arm.ElbowJoint.position = shoulderPos + arm.Root.TransformDirection(elbowTargetLocal);
            arm.VisualGlove.position = shoulderPos + arm.Root.TransformDirection(gloveTargetLocal);
            
            // Place segments using explicit endpoints - each segment from its exact endpoints
            SetSegmentBetween(arm.UpperArm, arm.ShoulderJoint.position, arm.ElbowJoint.position, _upperArmRadius);
            SetSegmentBetween(arm.Forearm, arm.ElbowJoint.position, arm.VisualGlove.position, _forearmRadius);
        }

        private static Vector3 FamilyPoleLocal(PunchFamily family, ActionPhase phase, bool left, bool player)
        {
            float side = left ? -1f : 1f;
            float opponentFlip = player ? 1f : -1f;
            return family switch
            {
                PunchFamily.Hook => new Vector3(side * 0.70f, 0.20f, 0.08f * opponentFlip),
                PunchFamily.Uppercut => new Vector3(side * 0.42f, -0.52f, -0.10f * opponentFlip),
                PunchFamily.Overhand => new Vector3(side * 0.45f, 0.58f, -0.06f * opponentFlip),
                PunchFamily.Straight => new Vector3(side * 0.28f, 0.10f, -0.12f * opponentFlip),
                _ => new Vector3(side * 0.48f, -0.08f, -0.22f * opponentFlip)
            };
        }

        private static Vector3 PhaseTarget(Vector3 guard, Vector3 commit, Vector3 target, ActionPhase phase, float t)
        {
            t = Smooth01(Mathf.Clamp01(t));
            return phase switch
            {
                ActionPhase.Commit => Vector3.Lerp(guard, commit, t),
                ActionPhase.Extend => Vector3.Lerp(commit, target, t),
                ActionPhase.Recover => Vector3.Lerp(target, guard, t),
                _ => guard
            };
        }

        private static Vector3 OpponentCommitPose(Vector3 guard, PunchFamily family, bool left)
        {
            float side = left ? -1f : 1f;
            return family switch
            {
                PunchFamily.Hook => guard + new Vector3(side * 0.18f, 0.02f, 0.02f),
                PunchFamily.Uppercut => guard + new Vector3(side * 0.05f, -0.24f, -0.02f),
                PunchFamily.Overhand => guard + new Vector3(side * 0.08f, 0.24f, -0.04f),
                _ => guard + new Vector3(side * 0.08f, -0.04f, -0.06f)
            };
        }

        private static bool IsLeadHand(PunchIntent intent) => !PunchLabels.IsRearHand(intent);

        private static Vector3 GetPlayerPunchTargetLocal(PunchIntent intent, bool left)
        {
            return intent switch
            {
                PunchIntent.Jab => new Vector3(-0.10f, 1.46f, 1.26f),
                PunchIntent.Cross => new Vector3(0.04f, 1.44f, 1.34f),
                PunchIntent.LeadHook => new Vector3(0.11f, 1.39f, 1.05f),
                PunchIntent.RearHook => new Vector3(-0.11f, 1.39f, 1.05f),
                PunchIntent.LeadUppercut => new Vector3(-0.05f, 1.50f, 0.98f),
                PunchIntent.RearUppercut => new Vector3(0.05f, 1.51f, 1.04f),
                PunchIntent.LeadOverhand => new Vector3(-0.04f, 1.48f, 1.17f),
                PunchIntent.RearOverhand => new Vector3(0.04f, 1.50f, 1.24f),
                _ => new Vector3(left ? -0.22f : 0.22f, 1.38f, 0.48f)
            };
        }

        private static Vector3 GetPlayerPunchCommitPose(PunchIntent intent, Vector3 guard, bool left)
        {
            float side = left ? -1f : 1f;
            return PunchLabels.Family(intent) switch
            {
                PunchFamily.Hook => guard + new Vector3(side * 0.18f, -0.03f, 0.02f),
                PunchFamily.Uppercut => guard + new Vector3(side * 0.05f, -0.30f, -0.02f),
                PunchFamily.Overhand => guard + new Vector3(side * 0.08f, 0.24f, -0.05f),
                _ => guard + new Vector3(side * 0.08f, -0.06f, -0.08f)
            };
        }

        private Transform CreateSphere(string name, Transform parent, Color color, Vector3 localPosition, float radius)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = Vector3.one * (radius * 2f);
            DisableCollider(go);
            ApplyColor(go.GetComponent<Renderer>(), color);
            return go.transform;
        }

        private Transform CreateCapsule(string name, Transform parent, Color color, float radius)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.localScale = new Vector3(radius * 2f, 0.1f, radius * 2f);
            DisableCollider(go);
            ApplyColor(go.GetComponent<Renderer>(), color);
            return go.transform;
        }

        // Helper: place a capsule segment between two exact endpoints
        private static void SetSegmentBetween(Transform segment, Vector3 start, Vector3 end, float radius)
        {
            Vector3 delta = end - start;
            float length = Mathf.Max(0.001f, delta.magnitude);
            segment.position = (start + end) * 0.5f;
            segment.rotation = Quaternion.FromToRotation(Vector3.up, delta / length);
            segment.localScale = new Vector3(radius * 2f, length * 0.5f, radius * 2f);
        }

        private void DrawDebugVisuals()
        {
            // Debug visuals are OFF by default (_enableDebugVisuals = false)
            // Only draws when explicitly enabled for developer debugging
            if (!_enableDebugVisuals) return;
            
            DrawArmDebug(_playerLeft); DrawArmDebug(_playerRight);
            DrawArmDebug(_opponentLeft); DrawArmDebug(_opponentRight);
        }

        private static void DrawArmDebug(VisualArm arm)
        {
            if (arm == null) return;
            // Yellow = upper arm, Cyan = forearm, Magenta = combat glove offset
            Debug.DrawLine(arm.ShoulderJoint.position, arm.ElbowJoint.position, Color.yellow);
            Debug.DrawLine(arm.ElbowJoint.position, arm.VisualGlove.position, Color.cyan);
            if (arm.OriginalGlove != null) Debug.DrawLine(arm.VisualGlove.position, arm.OriginalGlove.position, Color.magenta);
        }

        private static float Smooth01(float t) => t * t * (3f - 2f * t);

        private static void DisableCollider(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
        }

        private static void ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;
            Shader shader = Resources.Load<Shader>("BoxerP0UnlitColor");
            if (shader != null)
            {
                Material material = new(shader) { color = color };
                renderer.sharedMaterial = material;
                return;
            }
            Material fallback = renderer.material;
            if (fallback == null) return;
            if (fallback.HasProperty("_BaseColor")) fallback.SetColor("_BaseColor", color);
            if (fallback.HasProperty("_Color")) fallback.SetColor("_Color", color);
        }

        private static readonly Color Skin = new(0.56f, 0.31f, 0.22f, 1f);
        private static readonly Color ElbowColor = new(0.68f, 0.40f, 0.29f, 1f);
        private static readonly Color PlayerGlove = new(0.035f, 0.040f, 0.045f, 1f);
        private static readonly Color OpponentGlove = new(0.72f, 0.66f, 0.52f, 1f);
    }
}
