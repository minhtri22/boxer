using UnityEngine;

namespace BoxerP0
{
    // A read-only presentation chain. Elbows tuck toward the ribs rather than flare
    // outside the portrait frustum. Never writes combat anchors, reach, or colliders.
    public sealed class PlayerPOVArmVisual : MonoBehaviour
    {
        const int Rings = 25, Sides = 32;
        readonly Vector3[] _vertices = new Vector3[Rings * Sides];
        readonly Vector2[] _uv = new Vector2[Rings * Sides];
        Mesh _mesh;
        Material _skin;
        public Vector3 Shoulder { get; private set; }
        public Vector3 Elbow { get; private set; }
        public Vector3 Wrist { get; private set; }
        public float AnchorError { get; private set; }

        public void Initialize()
        {
            _mesh = new Mesh { name = "POV continuous upper arm, elbow and forearm" };
            _mesh.MarkDynamic();
            int[] indices = new int[(Rings - 1) * Sides * 6];
            int index = 0;
            for (int ring = 0; ring < Rings; ring++)
                for (int side = 0; side < Sides; side++)
                {
                    _uv[ring * Sides + side] = new Vector2(side / (float)Sides, ring / (float)(Rings - 1));
                    if (ring == Rings - 1) continue;
                    int a = ring * Sides + side, b = ring * Sides + (side + 1) % Sides;
                    indices[index++] = a; indices[index++] = b; indices[index++] = a + Sides;
                    indices[index++] = b; indices[index++] = b + Sides; indices[index++] = a + Sides;
                }
            _mesh.vertices = _vertices; _mesh.uv = _uv; _mesh.triangles = indices;
            gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _skin = new Material(Resources.Load<Shader>("POVArmSkin")) { color = new Color(.53f, .30f, .205f) };
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = _skin;
        }

        public void Apply(Vector3 shoulder, Vector3 wrist, Transform player, bool left)
        {
            // The shoulder and glove remain the shared world anchors. Only this
            // non-contact elbow uses an inward pole for a natural POV guard.
            float inward = left ? .26f : -.26f;
            Vector3 pole = shoulder + player.TransformDirection(new Vector3(inward, -.5f, .20f));
            var solution = ArmChainMath.Solve(shoulder, wrist, pole,
                ArmVisualEmbodiment.UpperArmLength, ArmVisualEmbodiment.ForearmLength);
            Shoulder = shoulder; Elbow = solution.Elbow; Wrist = wrist;
            AnchorError = Vector3.Distance(Wrist, wrist);
            Vector3 upper = (Elbow - Shoulder).normalized, forearm = (Wrist - Elbow).normalized;
            // One hinge-plane axis for the whole chain. Recomputing cross(tangent,
            // forward) per ring flips 180 degrees at a bent elbow and tears the mesh.
            Vector3 across = Vector3.Cross(upper, forearm).normalized;
            if (across.sqrMagnitude < .1f) across = player.right;
            Vector3 cuff = Wrist - forearm * .135f;
            float trim = Mathf.Min(.10f, (Elbow - Shoulder).magnitude * .4f, (cuff - Elbow).magnitude * .65f);
            Vector3 entry = Elbow - upper * trim, exit = Elbow + forearm * trim;
            for (int ring = 0; ring < Rings; ring++)
            {
                float t = ring / (float)(Rings - 1);
                // Rounded centerline as well as normals: a sharp centerline with
                // rotating rings self-intersects on the inner side of a tight guard.
                Vector3 center, tangent;
                float radius;
                if (t < .36f)
                {
                    float u = t / .36f;
                    center = Vector3.Lerp(Shoulder, entry, u); tangent = upper;
                    radius = Mathf.Lerp(.076f, .056f, u);
                }
                else if (t <= .70f)
                {
                    float u = (t - .36f) / .34f;
                    center = (1-u)*(1-u)*entry + 2*u*(1-u)*Elbow + u*u*exit;
                    tangent = ((1-u)*(Elbow-entry) + u*(exit-Elbow)).normalized;
                    radius = Mathf.Lerp(.056f, .053f, u);
                }
                else
                {
                    float u = (t - .70f) / .30f;
                    center = Vector3.Lerp(exit, cuff, u); tangent = forearm;
                    radius = Mathf.Lerp(.053f, .043f, u);
                }
                Vector3 depth = Vector3.Cross(tangent, across).normalized;
                for (int side = 0; side < Sides; side++)
                {
                    float angle = side * Mathf.PI * 2 / Sides;
                    _vertices[ring * Sides + side] = transform.InverseTransformPoint(center
                        + across * (Mathf.Cos(angle) * radius)
                        + depth * (Mathf.Sin(angle) * radius * .88f));
                }
            }
            _mesh.vertices = _vertices;
            _mesh.RecalculateNormals(); _mesh.RecalculateBounds();
        }

        void OnDestroy() { if (_mesh != null) Destroy(_mesh); if (_skin != null) Destroy(_skin); }
    }
}
