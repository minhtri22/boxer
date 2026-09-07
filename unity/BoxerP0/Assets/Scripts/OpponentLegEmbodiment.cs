using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// P1-B2 opponent-only lower-body embodiment:
    /// PELVIS → THIGH → KNEE JOINT → SHIN → FOOT
    /// Exactly TWO limb segments per leg: THIGH + SHIN. Knee is a JOINT only.
    /// Replaces the rectangular/capsule lower-body pedestal presentation.
    /// Visual-only; does not change combat, footwork, or AI logic.
    /// </summary>
    [DefaultExecutionOrder(150)]
    public sealed class OpponentLegEmbodiment : MonoBehaviour
    {
        [Header("P1-B2 Opponent Lower-Body Constants (Frozen)")]
        [SerializeField] private bool _enableDebugVisuals = false;
        [SerializeField] private float _pelvisHeight = 0.92f;
        [SerializeField] private float _hipWidth = 0.26f;
        [SerializeField] private float _thighLength = 0.46f;
        [SerializeField] private float _shinLength = 0.44f;
        [SerializeField] private float _footLength = 0.22f;
        [SerializeField] private float _kneeRadius = 0.055f;
        [SerializeField] private float _thighRadius = 0.075f;
        [SerializeField] private float _shinRadius = 0.055f;
        [SerializeField] private float _pelvisRadius = 0.13f;
        [SerializeField] private float _stanceWidth = 0.22f;
        [SerializeField] private float _stanceForward = 0.05f;

        // Frozen anthropometric constants (public for tests)
        public const float PelvisHeight = 0.92f;
        public const float HipWidth = 0.26f;
        public const float ThighLength = 0.46f;
        public const float ShinLength = 0.44f;
        public const float FootLength = 0.22f;
        public const float KneeRadius = 0.055f;
        public const float ThighRadius = 0.075f;
        public const float ShinRadius = 0.055f;

        private Transform _root;

        // Pelvis
        private Transform _pelvis;

        // Left leg
        private Transform _leftThigh;
        private Transform _leftKnee;
        private Transform _leftShin;
        private Transform _leftFoot;

        // Right leg
        private Transform _rightThigh;
        private Transform _rightKnee;
        private Transform _rightShin;
        private Transform _rightFoot;

        // Movement state
        private Vector3 _leftFootTargetLocal;
        private Vector3 _rightFootTargetLocal;
        private Vector3 _leftFootCurrentLocal;
        private Vector3 _rightFootCurrentLocal;

        private static readonly Color ShortsColor = new(0.18f, 0.19f, 0.21f, 1f);
        private static readonly Color Skin = new(0.56f, 0.31f, 0.22f, 1f);
        private static readonly Color KneeColor = new(0.68f, 0.40f, 0.29f, 1f);
        private static readonly Color FootColor = new(0.12f, 0.10f, 0.09f, 1f);

        public void Initialize(Transform opponentRoot)
        {
            _root = opponentRoot;
            BuildLegChain();
            CaptureNeutralStance();
        }

        private void BuildLegChain()
        {
            // PELVIS - anchors both legs
            _pelvis = CreateSphere("Opponent Pelvis", _root, ShortsColor,
                new Vector3(0f, PelvisHeight, 0f), _pelvisRadius);

            // LEFT THIGH (from pelvis to knee)
            _leftThigh = CreateCapsule("Opponent Left Thigh", _root, Skin, _thighRadius);
            _leftKnee = CreateSphere("Opponent Left Knee Joint", _root, KneeColor,
                Vector3.zero, _kneeRadius);
            _leftShin = CreateCapsule("Opponent Left Shin", _root, Skin, _shinRadius);
            _leftFoot = CreateFoot("Opponent Left Foot", _root);

            // RIGHT THIGH (from pelvis to knee)
            _rightThigh = CreateCapsule("Opponent Right Thigh", _root, Skin, _thighRadius);
            _rightKnee = CreateSphere("Opponent Right Knee Joint", _root, KneeColor,
                Vector3.zero, _kneeRadius);
            _rightShin = CreateCapsule("Opponent Right Shin", _root, Skin, _shinRadius);
            _rightFoot = CreateFoot("Opponent Right Foot", _root);
        }

        private void CaptureNeutralStance()
        {
            // Neutral stance: feet at hip width, slightly forward
            _leftFootTargetLocal = new Vector3(-_stanceWidth, 0.02f, _stanceForward);
            _rightFootTargetLocal = new Vector3(_stanceWidth, 0.02f, _stanceForward);
            _leftFootCurrentLocal = _leftFootTargetLocal;
            _rightFootCurrentLocal = _rightFootTargetLocal;

            PoseLegs();
        }

        private void Update()
        {
            if (_root == null) return;
            PoseLegs();
            if (_enableDebugVisuals) DrawDebugVisuals();
        }

        private void PoseLegs()
        {
            // Smoothly move feet toward targets
            float dt = Time.deltaTime;
            _leftFootCurrentLocal = Vector3.Lerp(_leftFootCurrentLocal, _leftFootTargetLocal, 8f * dt);
            _rightFootCurrentLocal = Vector3.Lerp(_rightFootCurrentLocal, _rightFootTargetLocal, 8f * dt);

            // Pelvis world position
            Vector3 pelvisWorld = _root.TransformPoint(new Vector3(0f, PelvisHeight, 0f));
            _pelvis.position = pelvisWorld;

            // Left leg: hip → knee → foot
            Vector3 leftHipWorld = _root.TransformPoint(new Vector3(-_hipWidth, PelvisHeight, 0f));
            Vector3 leftFootWorld = _root.TransformPoint(_leftFootCurrentLocal);
            PoseLeg(_leftThigh, _leftKnee, _leftShin, _leftFoot,
                leftHipWorld, leftFootWorld, true);

            // Right leg: hip → knee → foot
            Vector3 rightHipWorld = _root.TransformPoint(new Vector3(_hipWidth, PelvisHeight, 0f));
            Vector3 rightFootWorld = _root.TransformPoint(_rightFootCurrentLocal);
            PoseLeg(_rightThigh, _rightKnee, _rightShin, _rightFoot,
                rightHipWorld, rightFootWorld, false);
        }

        private void PoseLeg(Transform thigh, Transform knee, Transform shin, Transform foot,
            Vector3 hipWorld, Vector3 footWorld, bool left)
        {
            // Leg IK: solve knee position between hip and foot
            // Knee bends forward (toward +Z local, i.e. toward player)
            Vector3 hipToFoot = footWorld - hipWorld;
            float totalLength = ThighLength + ShinLength;
            float footDist = hipToFoot.magnitude;

            // Clamp foot distance to anatomical envelope
            Vector3 clampedFoot = footWorld;
            if (footDist > totalLength - 0.001f)
            {
                clampedFoot = hipWorld + hipToFoot.normalized * (totalLength - 0.001f);
                hipToFoot = clampedFoot - hipWorld;
                footDist = hipToFoot.magnitude;
            }

            // Knee forward bend direction (local forward = +Z)
            Vector3 forwardDir = _root.TransformDirection(Vector3.forward);
            Vector3 bendDir = forwardDir;

            // Solve knee using law of cosines (like ArmChainMath)
            Vector3 axis = hipToFoot / Mathf.Max(0.001f, footDist);
            float x = (ThighLength * ThighLength - ShinLength * ShinLength + footDist * footDist) /
                      (2f * footDist);
            float hSq = Mathf.Max(0f, ThighLength * ThighLength - x * x);
            float h = Mathf.Sqrt(hSq);

            // Project bend dir onto plane perpendicular to axis
            Vector3 bend = bendDir - axis * Vector3.Dot(bendDir, axis);
            if (bend.sqrMagnitude < 0.0001f)
            {
                bend = Vector3.Cross(axis, Vector3.up);
                if (bend.sqrMagnitude < 0.0001f) bend = Vector3.Cross(axis, Vector3.right);
            }
            bend.Normalize();

            Vector3 kneeWorld = hipWorld + axis * x + bend * h;

            // Place joints and segments
            knee.position = kneeWorld;
            SetSegmentBetween(thigh, hipWorld, kneeWorld, _thighRadius);
            SetSegmentBetween(shin, kneeWorld, clampedFoot, _shinRadius);

            // Foot: place at ground, oriented forward
            foot.position = clampedFoot;
            foot.rotation = _root.rotation * Quaternion.Euler(0f, 0f, 0f);
        }

        private static void SetSegmentBetween(Transform segment, Vector3 start, Vector3 end, float radius)
        {
            Vector3 delta = end - start;
            float length = Mathf.Max(0.001f, delta.magnitude);
            segment.position = (start + end) * 0.5f;
            segment.rotation = Quaternion.FromToRotation(Vector3.up, delta / length);
            segment.localScale = new Vector3(radius * 2f, length * 0.5f, radius * 2f);
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

        private Transform CreateFoot(string name, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.localScale = new Vector3(0.09f, 0.04f, FootLength);
            DisableCollider(go);
            ApplyColor(go.GetComponent<Renderer>(), FootColor);
            return go.transform;
        }

        private void DrawDebugVisuals()
        {
            if (_leftThigh != null && _leftKnee != null)
                Debug.DrawLine(_leftThigh.position, _leftKnee.position, Color.green);
            if (_leftKnee != null && _leftFoot != null)
                Debug.DrawLine(_leftKnee.position, _leftFoot.position, Color.green);
            if (_rightThigh != null && _rightKnee != null)
                Debug.DrawLine(_rightThigh.position, _rightKnee.position, Color.green);
            if (_rightKnee != null && _rightFoot != null)
                Debug.DrawLine(_rightKnee.position, _rightFoot.position, Color.green);
        }

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
    }
}