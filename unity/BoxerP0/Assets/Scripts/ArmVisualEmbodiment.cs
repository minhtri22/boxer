using System;
using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// P1-B1.5 Arm Embodiment + Punch Readability
    /// Visual-only arm chain: shoulder → upper arm → elbow → forearm → glove
    /// Does NOT affect combat/hit detection. Purely visual readability layer.
    /// </summary>
    [DefaultExecutionOrder(200)] // After PlayerBoxer/OpponentBoxer (which run at default 0)
    public sealed class ArmVisualEmbodiment : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool _enableDebugVisuals = false;
        [SerializeField] private float _shoulderWidth = 0.38f;
        [SerializeField] private float _upperArmLength = 0.30f;
        [SerializeField] private float _forearmLength = 0.26f;

        // References
        private PlayerBoxer _playerBoxer;
        private OpponentBoxer _opponentBoxer;
        private Phase0Telemetry _telemetry;

        // Player arm chain
        private Transform _playerLeftShoulder;
        private Transform _playerLeftElbow;
        private Transform _playerLeftForearm;
        private Transform _playerRightShoulder;
        private Transform _playerRightElbow;
        private Transform _playerRightForearm;

        // Opponent arm chain
        private Transform _opponentLeftShoulder;
        private Transform _opponentLeftElbow;
        private Transform _opponentLeftForearm;
        private Transform _opponentRightShoulder;
        private Transform _opponentRightElbow;
        private Transform _opponentRightForearm;

        // Guard positions (local to boxer root)
        private Vector3 _playerLeftGuardLocal;
        private Vector3 _playerRightGuardLocal;
        private Vector3 _opponentLeftGuardLocal;
        private Vector3 _opponentRightGuardLocal;

        // Cached state for visual update (read via public properties/methods)
        private PunchIntent _cachedPlayerIntent;
        private PunchIntent _cachedOpponentIntent;
        private ActionPhase _cachedPlayerPhase;
        private ActionPhase _cachedOpponentPhase;
        private bool _cachedPlayerIsBusy;
        private bool _cachedOpponentIsBusy;
        private string _cachedPlayerStepState;
        private float _cachedPlayerDistanceMeters;
        private bool _cachedPlayerHasSnapshot;
        private Vector3 _cachedOpponentAttackTargetLocal;

        private const float ShoulderHeightOffset = 1.37f;
        private const float ShoulderForwardOffset = 0.0f;

        public void Initialize(
            PlayerBoxer playerBoxer,
            OpponentBoxer opponentBoxer,
            Phase0Telemetry telemetry)
        {
            _playerBoxer = playerBoxer;
            _opponentBoxer = opponentBoxer;
            _telemetry = telemetry;

            BuildPlayerArmChain();
            BuildOpponentArmChain();
            CaptureGuardPositions();
        }

        private void BuildPlayerArmChain()
        {
            Transform playerRoot = _playerBoxer.transform;

            // Left arm
            _playerLeftShoulder = CreateArmSegment("Player Left Shoulder", playerRoot, Skin, new Vector3(-_shoulderWidth, ShoulderHeightOffset, ShoulderForwardOffset), new Vector3(0.12f, 0.14f, 0.12f));
            _playerLeftElbow = CreateArmSegment("Player Left Elbow", _playerLeftShoulder, Skin, new Vector3(0f, -_upperArmLength * 0.5f, 0f), new Vector3(0.09f, _upperArmLength * 0.5f, 0.09f));
            _playerLeftForearm = CreateArmSegment("Player Left Forearm", _playerLeftElbow, Skin, new Vector3(0f, -_forearmLength * 0.5f, 0f), new Vector3(0.08f, _forearmLength * 0.5f, 0.08f));

            // Right arm
            _playerRightShoulder = CreateArmSegment("Player Right Shoulder", playerRoot, Skin, new Vector3(_shoulderWidth, ShoulderHeightOffset, ShoulderForwardOffset), new Vector3(0.12f, 0.14f, 0.12f));
            _playerRightElbow = CreateArmSegment("Player Right Elbow", _playerRightShoulder, Skin, new Vector3(0f, -_upperArmLength * 0.5f, 0f), new Vector3(0.09f, _upperArmLength * 0.5f, 0.09f));
            _playerRightForearm = CreateArmSegment("Player Right Forearm", _playerRightElbow, Skin, new Vector3(0f, -_forearmLength * 0.5f, 0f), new Vector3(0.08f, _forearmLength * 0.5f, 0.08f));
        }

        private void BuildOpponentArmChain()
        {
            Transform opponentRoot = _opponentBoxer.transform;

            // Opponent faces player (rotated 180), so shoulders are mirrored
            _opponentLeftShoulder = CreateArmSegment("Opponent Left Shoulder", opponentRoot, Skin, new Vector3(_shoulderWidth, ShoulderHeightOffset, ShoulderForwardOffset), new Vector3(0.12f, 0.14f, 0.12f));
            _opponentLeftElbow = CreateArmSegment("Opponent Left Elbow", _opponentLeftShoulder, Skin, new Vector3(0f, -_upperArmLength * 0.5f, 0f), new Vector3(0.09f, _upperArmLength * 0.5f, 0.09f));
            _opponentLeftForearm = CreateArmSegment("Opponent Left Forearm", _opponentLeftElbow, Skin, new Vector3(0f, -_forearmLength * 0.5f, 0f), new Vector3(0.08f, _forearmLength * 0.5f, 0.08f));

            _opponentRightShoulder = CreateArmSegment("Opponent Right Shoulder", opponentRoot, Skin, new Vector3(-_shoulderWidth, ShoulderHeightOffset, ShoulderForwardOffset), new Vector3(0.12f, 0.14f, 0.12f));
            _opponentRightElbow = CreateArmSegment("Opponent Right Elbow", _opponentRightShoulder, Skin, new Vector3(0f, -_upperArmLength * 0.5f, 0f), new Vector3(0.09f, _upperArmLength * 0.5f, 0.09f));
            _opponentRightForearm = CreateArmSegment("Opponent Right Forearm", _opponentRightElbow, Skin, new Vector3(0f, -_forearmLength * 0.5f, 0f), new Vector3(0.08f, _forearmLength * 0.5f, 0.08f));
        }

        private Transform CreateArmSegment(string name, Transform parent, Color color, Vector3 localPosition, Vector3 localScale)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            segment.name = name;
            segment.transform.SetParent(parent, false);
            segment.transform.localPosition = localPosition;
            segment.transform.localScale = localScale;
            segment.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Capsule upright
            DisableCollider(segment);
            ApplyColor(segment.GetComponent<Renderer>(), color);
            return segment.transform;
        }

        private void CaptureGuardPositions()
        {
            _playerLeftGuardLocal = _playerBoxer.LeftGlove.localPosition;
            _playerRightGuardLocal = _playerBoxer.RightGlove.localPosition;

            Transform opponentRoot = _opponentBoxer.transform;
            Transform oppLeftGlove = opponentRoot.Find("Opponent Left Glove");
            Transform oppRightGlove = opponentRoot.Find("Opponent Right Glove");
            if (oppLeftGlove != null) _opponentLeftGuardLocal = oppLeftGlove.localPosition;
            if (oppRightGlove != null) _opponentRightGuardLocal = oppRightGlove.localPosition;
        }

        private void Update()
        {
            if (_playerBoxer == null || _opponentBoxer == null) return;

            CacheState();
            UpdatePlayerArms();
            UpdateOpponentArms();

            if (_enableDebugVisuals)
            {
                DrawDebugVisuals();
            }
        }

        private void CacheState()
        {
            // Cache player state using public properties
            _cachedPlayerIsBusy = _playerBoxer.IsActionBusy;
            _cachedPlayerIntent = _playerBoxer.CurrentIntent;
            _cachedPlayerPhase = _playerBoxer.CurrentPhase;
            _cachedPlayerStepState = _playerBoxer.HasP1PunchSnapshot ? _playerBoxer.P1PunchSnapshot.StepState : "NEUTRAL";
            _cachedPlayerDistanceMeters = _playerBoxer.HasP1PunchSnapshot ? _playerBoxer.P1PunchSnapshot.DistanceMeters : 1.0f;
            _cachedPlayerHasSnapshot = _playerBoxer.HasP1PunchSnapshot;

            // Cache opponent state using public properties
            _cachedOpponentIsBusy = _opponentBoxer.IsActionBusy;
            _cachedOpponentIntent = _opponentBoxer.CurrentIntent;
            _cachedOpponentPhase = _opponentBoxer.CurrentPhase;
            _cachedOpponentAttackTargetLocal = _opponentBoxer.AttackTargetLocal;
        }

        private void UpdatePlayerArms()
        {
            PunchIntent intent = _cachedPlayerIntent;
            ActionPhase phase = _cachedPlayerPhase;

            bool left = IsLeadHand(intent);
            Transform activeShoulder = left ? _playerLeftShoulder : _playerRightShoulder;
            Transform activeElbow = left ? _playerLeftElbow : _playerRightElbow;
            Transform activeForearm = left ? _playerLeftForearm : _playerRightForearm;
            Transform passiveShoulder = left ? _playerRightShoulder : _playerLeftShoulder;
            Transform passiveElbow = left ? _playerRightElbow : _playerLeftElbow;
            Transform passiveForearm = left ? _playerRightForearm : _playerLeftForearm;

            Vector3 activeGuardLocal = left ? _playerLeftGuardLocal : _playerRightGuardLocal;
            Vector3 passiveGuardLocal = left ? _playerRightGuardLocal : _playerLeftGuardLocal;

            if (!_cachedPlayerIsBusy)
            {
                UpdateArmToGuard(activeShoulder, activeElbow, activeForearm, activeGuardLocal);
                UpdateArmToGuard(passiveShoulder, passiveElbow, passiveForearm, passiveGuardLocal);
                return;
            }

            Vector3 targetLocal = GetPlayerPunchTargetLocal(intent, left);
            Vector3 commitPose = GetPlayerPunchCommitPose(intent, activeGuardLocal, left);

            if (_cachedPlayerHasSnapshot)
            {
                targetLocal = P1PunchMechanics.ApplyA1StraightReach(intent, targetLocal, _cachedPlayerStepState);
                targetLocal = P1PunchMechanics.ApplyA3FamilyCoupling(intent, targetLocal, _cachedPlayerDistanceMeters);
            }

            UpdateArmChain(activeShoulder, activeElbow, activeForearm, activeGuardLocal, commitPose, targetLocal, phase, true);
            UpdateArmToGuard(passiveShoulder, passiveElbow, passiveForearm, passiveGuardLocal);
        }

        private void UpdateOpponentArms()
        {
            PunchIntent intent = _cachedOpponentIntent;
            ActionPhase phase = _cachedOpponentPhase;

            bool left = IsLeadHand(intent);
            Transform activeShoulder = left ? _opponentLeftShoulder : _opponentRightShoulder;
            Transform activeElbow = left ? _opponentLeftElbow : _opponentRightElbow;
            Transform activeForearm = left ? _opponentLeftForearm : _opponentRightForearm;
            Transform passiveShoulder = left ? _opponentRightShoulder : _opponentLeftShoulder;
            Transform passiveElbow = left ? _opponentRightElbow : _opponentLeftElbow;
            Transform passiveForearm = left ? _opponentRightForearm : _opponentLeftForearm;

            Vector3 activeGuardLocal = left ? _opponentLeftGuardLocal : _opponentRightGuardLocal;
            Vector3 passiveGuardLocal = left ? _opponentRightGuardLocal : _opponentLeftGuardLocal;

            if (!_cachedOpponentIsBusy)
            {
                UpdateArmToGuard(activeShoulder, activeElbow, activeForearm, activeGuardLocal);
                UpdateArmToGuard(passiveShoulder, passiveElbow, passiveForearm, passiveGuardLocal);
                return;
            }

            Vector3 targetLocal = _cachedOpponentAttackTargetLocal;
            Vector3 commitPose = activeGuardLocal + new Vector3(left ? -0.10f : 0.10f, 0.08f, 0.18f);

            UpdateArmChain(activeShoulder, activeElbow, activeForearm, activeGuardLocal, commitPose, targetLocal, phase, false);
            UpdateArmToGuard(passiveShoulder, passiveElbow, passiveForearm, passiveGuardLocal);
        }

        private void UpdateArmChain(
            Transform shoulder,
            Transform elbow,
            Transform forearm,
            Vector3 guardLocal,
            Vector3 commitPoseLocal,
            Vector3 targetLocal,
            ActionPhase phase,
            bool isPlayer)
        {
            Vector3 shoulderWorld = shoulder.parent.TransformPoint(shoulder.localPosition);
            Vector3 gloveTargetWorld;

            float commitDuration = isPlayer ? 0.09f : 0.34f;
            float extendDuration = isPlayer ? 0.14f : 0.17f;
            float recoverDuration = isPlayer ? 0.28f : 0.48f;

            float normalizedPhase;
            if (isPlayer)
            {
                normalizedPhase = _playerBoxer.ActionNormalizedPhase(
                    phase == ActionPhase.Commit ? commitDuration :
                    phase == ActionPhase.Extend ? extendDuration : recoverDuration);
            }
            else
            {
                normalizedPhase = _opponentBoxer.ActionNormalizedPhase(
                    phase == ActionPhase.Commit ? commitDuration :
                    phase == ActionPhase.Extend ? extendDuration : recoverDuration);
            }

            switch (phase)
            {
                case ActionPhase.Commit:
                    gloveTargetWorld = shoulder.parent.TransformPoint(Vector3.Lerp(guardLocal, commitPoseLocal, Smooth01(normalizedPhase)));
                    break;
                case ActionPhase.Extend:
                    gloveTargetWorld = shoulder.parent.TransformPoint(Vector3.Lerp(commitPoseLocal, targetLocal, Smooth01(normalizedPhase)));
                    break;
                case ActionPhase.Recover:
                    gloveTargetWorld = shoulder.parent.TransformPoint(Vector3.Lerp(targetLocal, guardLocal, Smooth01(normalizedPhase)));
                    break;
                default:
                    gloveTargetWorld = shoulder.parent.TransformPoint(guardLocal);
                    break;
            }

            SolveArmIK(shoulder, elbow, forearm, gloveTargetWorld);
        }

        private void UpdateArmToGuard(Transform shoulder, Transform elbow, Transform forearm, Vector3 guardLocal)
        {
            Vector3 guardWorld = shoulder.parent.TransformPoint(guardLocal);
            SolveArmIK(shoulder, elbow, forearm, guardWorld);
        }

        private void SolveArmIK(Transform shoulder, Transform elbow, Transform forearm, Vector3 targetWorld)
        {
            Vector3 shoulderWorld = shoulder.position;
            Vector3 shoulderToTarget = targetWorld - shoulderWorld;
            float distance = shoulderToTarget.magnitude;

            float maxReach = _upperArmLength + _forearmLength;

            Vector3 clampedTarget = distance > maxReach
                ? shoulderWorld + shoulderToTarget.normalized * maxReach
                : targetWorld;

            Vector3 dirToTarget = (clampedTarget - shoulderWorld).normalized;
            float d = Vector3.Distance(shoulderWorld, clampedTarget);

            float cosShoulder = (d > 0.001f)
                ? Mathf.Clamp((_upperArmLength * _upperArmLength + d * d - _forearmLength * _forearmLength) / (2f * _upperArmLength * d), -1f, 1f)
                : 1f;
            float shoulderAngle = Mathf.Acos(cosShoulder);

            Vector3 elbowDir = Vector3.Cross(dirToTarget, Vector3.right).normalized;
            Vector3 elbowPos = shoulderWorld + dirToTarget * _upperArmLength * cosShoulder + elbowDir * _upperArmLength * Mathf.Sin(shoulderAngle);

            shoulder.rotation = Quaternion.LookRotation(dirToTarget, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            elbow.position = elbowPos;
            elbow.rotation = Quaternion.LookRotation((clampedTarget - elbowPos).normalized, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            forearm.position = clampedTarget;
            forearm.rotation = elbow.rotation;
        }

        private static bool IsLeadHand(PunchIntent intent)
        {
            return !PunchLabels.IsRearHand(intent);
        }

        private Vector3 GetPlayerPunchTargetLocal(PunchIntent intent, bool left)
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

        private Vector3 GetPlayerPunchCommitPose(PunchIntent intent, Vector3 guard, bool left)
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

        private static float Smooth01(float t) => t * t * (3f - 2f * t);

        private void DrawDebugVisuals()
        {
            Color debugColor = Color.yellow;
            float duration = 0f;

            // Player left arm
            if (_playerLeftShoulder != null && _playerLeftElbow != null)
                Debug.DrawLine(_playerLeftShoulder.position, _playerLeftElbow.position, debugColor, duration);
            if (_playerLeftElbow != null && _playerLeftForearm != null)
                Debug.DrawLine(_playerLeftElbow.position, _playerLeftForearm.position, debugColor, duration);
            if (_playerLeftForearm != null)
            {
                Vector3 glovePos = _playerBoxer.LeftGlove.position;
                Debug.DrawLine(_playerLeftForearm.position, glovePos, debugColor, duration);
            }

            // Player right arm
            if (_playerRightShoulder != null && _playerRightElbow != null)
                Debug.DrawLine(_playerRightShoulder.position, _playerRightElbow.position, debugColor, duration);
            if (_playerRightElbow != null && _playerRightForearm != null)
                Debug.DrawLine(_playerRightElbow.position, _playerRightForearm.position, debugColor, duration);
            if (_playerRightForearm != null)
            {
                Vector3 glovePos = _playerBoxer.RightGlove.position;
                Debug.DrawLine(_playerRightForearm.position, glovePos, debugColor, duration);
            }

            // Opponent left arm
            if (_opponentLeftShoulder != null && _opponentLeftElbow != null)
                Debug.DrawLine(_opponentLeftShoulder.position, _opponentLeftElbow.position, debugColor, duration);
            if (_opponentLeftElbow != null && _opponentLeftForearm != null)
                Debug.DrawLine(_opponentLeftElbow.position, _opponentLeftForearm.position, debugColor, duration);
            if (_opponentLeftForearm != null)
            {
                Transform oppLeftGlove = _opponentBoxer.transform.Find("Opponent Left Glove");
                if (oppLeftGlove != null)
                    Debug.DrawLine(_opponentLeftForearm.position, oppLeftGlove.position, debugColor, duration);
            }

            // Opponent right arm
            if (_opponentRightShoulder != null && _opponentRightElbow != null)
                Debug.DrawLine(_opponentRightShoulder.position, _opponentRightElbow.position, debugColor, duration);
            if (_opponentRightElbow != null && _opponentRightForearm != null)
                Debug.DrawLine(_opponentRightElbow.position, _opponentRightForearm.position, debugColor, duration);
            if (_opponentRightForearm != null)
            {
                Transform oppRightGlove = _opponentBoxer.transform.Find("Opponent Right Glove");
                if (oppRightGlove != null)
                    Debug.DrawLine(_opponentRightForearm.position, oppRightGlove.position, debugColor, duration);
            }
        }

        private static void DisableCollider(GameObject go)
        {
            Collider c = go.GetComponent<Collider>();
            if (c != null) c.enabled = false;
        }

        private static void ApplyColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;

            Shader shader = Resources.Load<Shader>("BoxerP0UnlitColor");
            if (shader != null)
            {
                Material material = new(shader);
                material.color = color;
                renderer.sharedMaterial = material;
                return;
            }

            Material fallback = renderer.material;
            if (fallback == null) return;
            if (fallback.HasProperty("_BaseColor")) fallback.SetColor("_BaseColor", color);
            if (fallback.HasProperty("_Color")) fallback.SetColor("_Color", color);
        }

        // Color constants
        private static readonly Color Skin = new(0.56f, 0.31f, 0.22f, 1f);
        private static readonly Color GloveBlack = new(0.035f, 0.040f, 0.045f, 1f);
        private static readonly Color OpponentGlove = new(0.72f, 0.66f, 0.52f, 1f);
    }
}