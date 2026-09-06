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
    /// Pure visual two-bone arm solver used by P1-B1.5R. Combat geometry remains authoritative elsewhere.
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
    /// P1-B1.5R visual-only anatomical chain:
    /// shoulder joint -> upper arm -> explicit elbow joint -> forearm -> visual glove proxy.
    /// Original glove transforms/colliders remain untouched and authoritative for combat.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class ArmVisualEmbodiment : MonoBehaviour
    {
        [SerializeField] private bool _enableDebugVisuals = false;
        [SerializeField] private float _shoulderWidth = 0.38f;
        [SerializeField] private float _upperArmLength = 0.34f;
        [SerializeField] private float _forearmLength = 0.31f;
        [SerializeField] private float _jointRadius = 0.075f;
        [SerializeField] private float _armRadius = 0.060f;
        [SerializeField] private float _visualGloveRadius = 0.115f;

        public float UpperArmLength => _upperArmLength;
        public float ForearmLength => _forearmLength;
        public float MaxVisualReach => _upperArmLength + _forearmLength;

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

        private const float ShoulderHeight = 1.43f;
        private const float ShoulderForward = 0.02f;

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

        private VisualArm BuildArm(string prefix, Transform root, Transform originalGlove, bool left, bool player)
        {
            float localX = left ? -_shoulderWidth : _shoulderWidth;
            if (!player) localX = -localX; // opponent root faces 180 degrees

            VisualArm arm = new()
            {
                Root = root,
                Left = left,
                OriginalGlove = originalGlove
            };

            arm.ShoulderJoint = CreateSphere(prefix + " Shoulder Joint", root, Skin,
                new Vector3(localX, ShoulderHeight, ShoulderForward), _jointRadius * 1.08f);
            arm.UpperArm = CreateCapsule(prefix + " Upper Arm", root, Skin, _armRadius);
            arm.ElbowJoint = CreateSphere(prefix + " Elbow Joint", root, ElbowColor, Vector3.zero, _jointRadius);
            arm.Forearm = CreateCapsule(prefix + " Forearm", root, Skin, _armRadius * 0.92f);
            arm.VisualGlove = CreateSphere(prefix + " Visual Glove", root,
                player ? PlayerGlove : OpponentGlove, Vector3.zero, _visualGloveRadius);

            if (originalGlove != null)
            {
                foreach (Renderer renderer in originalGlove.GetComponentsInChildren<Renderer>(true))
                    renderer.enabled = false;
            }

            return arm;
        }

        private void Update()
        {
            if (_playerBoxer == null || _opponentBoxer == null) return;
            CacheState();
            UpdatePlayerArms();
            UpdateOpponentArms();
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
            }

            Vector3 desired = PhaseTarget(activeGuard, commit, target, _playerPhase,
                _playerBoxer.ActionNormalizedPhase(PhaseDuration(_playerPhase, true)));
            PoseArm(active, desired, PunchLabels.Family(_playerIntent), _playerPhase, true);
            PoseArm(passive, passiveGuard, PunchFamily.None, ActionPhase.Guard, true);
        }

        private void UpdateOpponentArms()
        {
            bool activeLeft = IsLeadHand(_opponentIntent);
            VisualArm active = activeLeft ? _opponentLeft : _opponentRight;
            VisualArm passive = activeLeft ? _opponentRight : _opponentLeft;
            Vector3 activeGuard = activeLeft ? _opponentLeftGuardLocal : _opponentRightGuardLocal;
            Vector3 passiveGuard = activeLeft ? _opponentRightGuardLocal : _opponentLeftGuardLocal;

            if (!_opponentBusy)
            {
                PoseArm(active, activeGuard, PunchFamily.None, ActionPhase.Guard, false);
                PoseArm(passive, passiveGuard, PunchFamily.None, ActionPhase.Guard, false);
                return;
            }

            PunchFamily family = PunchLabels.Family(_opponentIntent);
            Vector3 commit = OpponentCommitPose(activeGuard, family, activeLeft);
            Vector3 desired = PhaseTarget(activeGuard, commit, _opponentTargetLocal, _opponentPhase,
                _opponentBoxer.ActionNormalizedPhase(PhaseDuration(_opponentPhase, false)));
            PoseArm(active, desired, family, _opponentPhase, false);
            PoseArm(passive, passiveGuard, PunchFamily.None, ActionPhase.Guard, false);
        }

        private void PoseArm(VisualArm arm, Vector3 requestedWristLocal, PunchFamily family, ActionPhase phase, bool player)
        {
            if (arm == null || arm.Root == null) return;
            Vector3 shoulder = arm.ShoulderJoint.position;
            Vector3 requestedWrist = arm.Root.TransformPoint(requestedWristLocal);
            Vector3 poleLocal = FamilyPoleLocal(family, phase, arm.Left, player);
            Vector3 poleWorld = shoulder + arm.Root.TransformDirection(poleLocal);
            ArmChainSolution solved = ArmChainMath.Solve(shoulder, requestedWrist, poleWorld, _upperArmLength, _forearmLength);

            arm.ShoulderJoint.position = shoulder;
            arm.ElbowJoint.position = solved.Elbow;
            arm.VisualGlove.position = solved.Wrist;
            PlaceSegment(arm.UpperArm, solved.Shoulder, solved.Elbow, _armRadius);
            PlaceSegment(arm.Forearm, solved.Elbow, solved.Wrist, _armRadius * 0.92f);
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

        private static float PhaseDuration(ActionPhase phase, bool player)
        {
            if (player)
                return phase == ActionPhase.Commit ? 0.09f : phase == ActionPhase.Extend ? 0.14f : 0.28f;
            return phase == ActionPhase.Commit ? 0.34f : phase == ActionPhase.Extend ? 0.17f : 0.48f;
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

        private static void PlaceSegment(Transform segment, Vector3 a, Vector3 b, float radius)
        {
            Vector3 delta = b - a;
            float length = Mathf.Max(0.001f, delta.magnitude);
            segment.position = (a + b) * 0.5f;
            segment.rotation = Quaternion.FromToRotation(Vector3.up, delta / length);
            segment.localScale = new Vector3(radius * 2f, length * 0.5f, radius * 2f);
        }

        private void DrawDebugVisuals()
        {
            DrawArmDebug(_playerLeft); DrawArmDebug(_playerRight);
            DrawArmDebug(_opponentLeft); DrawArmDebug(_opponentRight);
        }

        private static void DrawArmDebug(VisualArm arm)
        {
            if (arm == null) return;
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
