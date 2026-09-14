using System.Collections.Generic;
using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// Presentation-only segmented reference shell for P1-EV.
    /// Every visible part follows an accepted anatomical transform and never writes gameplay transforms.
    /// </summary>
    [DefaultExecutionOrder(300)]
    public sealed class EVReferenceVisuals : MonoBehaviour
    {
        public const int ExpectedVisualPartCount = 17;

        sealed class RigPart
        {
            public Transform Visual;
            public Transform Anchor;
            public Transform Secondary;
            public Vector3 Offset;
            public float Width;
            public float Height;
            public float HeightFromLength;
            public float CameraBias;
            public bool FollowBone;
        }

        readonly List<Mesh> _meshes = new List<Mesh>();
        readonly List<Material> _materials = new List<Material>();
        readonly List<RigPart> _rigParts = new List<RigPart>();

        Camera _camera;
        Transform _opponent;
        Transform _player;
        bool _ready;

        public bool Ready => _ready;
        public bool RigBound => _ready && _rigParts.TrueForAll(part => part.Visual != null && part.Anchor != null);
        public int VisualPartCount => _rigParts.Count;

        public bool HasAnchor(Transform anchor)
        {
            return anchor != null && _rigParts.Exists(part => part.Anchor == anchor || part.Secondary == anchor);
        }

        public void Initialize(Transform opponent, Transform player, Transform contactTorso)
        {
            _opponent = opponent;
            _player = player;
            _camera = Camera.main ?? FindAnyObjectByType<Camera>();

            if (_opponent == null || _player == null || contactTorso == null || _camera == null) return;

            Shader rigSprite = Resources.Load<Shader>("EVRigSprite");
            if (rigSprite == null || !BuildRigVisuals(rigSprite)) return;

            // Ramirez is represented only by the segmented rig-bound reference shell.
            DisableRenderers(_opponent);

            // The POV glove sprites replace only the glove/cuff presentation. The player
            // arms remain visible and the authoritative glove transforms/colliders stay active.
            DisableRenderers(_player.Find("Player Left Glove"));
            DisableRenderers(_player.Find("Player Right Glove"));

            _ready = _rigParts.Count == ExpectedVisualPartCount;
            if (_ready) UpdateRigVisuals();
        }

        bool BuildRigVisuals(Shader shader)
        {
            Transform head = _opponent.Find("Opponent Head");
            Transform abdomen = _opponent.Find("R2 Abdomen");
            Transform chest = _opponent.Find("R2 Chest");
            Transform shortsVisual = _opponent.Find("Opponent Shorts Visual");
            Transform leftUpper = _opponent.Find("Left Upper Arm");
            Transform rightUpper = _opponent.Find("Right Upper Arm");
            Transform leftForearm = _opponent.Find("Left Forearm");
            Transform rightForearm = _opponent.Find("Right Forearm");
            Transform leftGlove = _opponent.Find("Opponent Left Glove");
            Transform rightGlove = _opponent.Find("Opponent Right Glove");
            Transform leftThigh = _opponent.Find("Left Thigh");
            Transform rightThigh = _opponent.Find("Right Thigh");
            Transform leftShin = _opponent.Find("Left Shin");
            Transform rightShin = _opponent.Find("Right Shin");
            Transform leftBoot = _opponent.Find("Left Shoe");
            Transform rightBoot = _opponent.Find("Right Shoe");
            Transform playerLeftGlove = _player.Find("Player Left Glove");
            Transform playerRightGlove = _player.Find("Player Right Glove");

            if (head == null || abdomen == null || chest == null || shortsVisual == null ||
                leftUpper == null || rightUpper == null || leftForearm == null || rightForearm == null ||
                leftGlove == null || rightGlove == null || leftThigh == null || rightThigh == null ||
                leftShin == null || rightShin == null || leftBoot == null || rightBoot == null ||
                playerLeftGlove == null || playerRightGlove == null)
                return false;

            Texture2D headTex = Segment("ramirez-head");
            Texture2D torsoTex = Segment("ramirez-torso");
            Texture2D shortsTex = Segment("ramirez-shorts");
            Texture2D armTex = Segment("ramirez-arm");
            Texture2D gloveTex = Segment("ramirez-glove");
            Texture2D thighTex = Segment("ramirez-thigh");
            Texture2D shinTex = Segment("ramirez-shin");
            Texture2D bootTex = Segment("ramirez-boot");
            Texture2D playerGloveTex = Segment("player-glove");

            if (headTex == null || torsoTex == null || shortsTex == null || armTex == null ||
                gloveTex == null || thighTex == null || shinTex == null || bootTex == null || playerGloveTex == null)
                return false;

            AddFixedRigPart(shader, "EV Rig Head", headTex, head, null,
                .43f, .48f, Vector3.zero, .016f, false, false);
            AddFixedRigPart(shader, "EV Rig Torso", torsoTex, abdomen, chest,
                .72f, .78f, new Vector3(0f, .10f, 0f), .012f, false, false);
            AddFixedRigPart(shader, "EV Rig Shorts", shortsTex, shortsVisual, null,
                .76f, .52f, Vector3.zero, .014f, false, false);

            AddBoneRigPart(shader, "EV Rig Left Upper Arm", armTex, leftUpper, .24f, 1.10f, .020f, true);
            AddBoneRigPart(shader, "EV Rig Right Upper Arm", armTex, rightUpper, .24f, 1.10f, .021f, false);
            AddBoneRigPart(shader, "EV Rig Left Forearm", armTex, leftForearm, .22f, 1.10f, .022f, true);
            AddBoneRigPart(shader, "EV Rig Right Forearm", armTex, rightForearm, .22f, 1.10f, .023f, false);
            AddFixedRigPart(shader, "EV Rig Left Glove", gloveTex, leftGlove, null,
                .30f, .34f, Vector3.zero, .028f, true, true);
            AddFixedRigPart(shader, "EV Rig Right Glove", gloveTex, rightGlove, null,
                .30f, .34f, Vector3.zero, .029f, false, true);

            AddBoneRigPart(shader, "EV Rig Left Thigh", thighTex, leftThigh, .31f, 1.15f, .013f, true);
            AddBoneRigPart(shader, "EV Rig Right Thigh", thighTex, rightThigh, .31f, 1.15f, .014f, false);
            AddBoneRigPart(shader, "EV Rig Left Shin", shinTex, leftShin, .25f, 1.15f, .015f, true);
            AddBoneRigPart(shader, "EV Rig Right Shin", shinTex, rightShin, .25f, 1.15f, .016f, false);
            AddFixedRigPart(shader, "EV Rig Left Boot", bootTex, leftBoot, null,
                .28f, .36f, new Vector3(0f, -.04f, 0f), .021f, true, false);
            AddFixedRigPart(shader, "EV Rig Right Boot", bootTex, rightBoot, null,
                .28f, .36f, new Vector3(0f, -.04f, 0f), .022f, false, false);

            // The reference asset is the approved left POV glove. Keep it unmirrored on
            // the anatomical left anchor and mirror it only for the anatomical right anchor.
            AddFixedRigPart(shader, "EV Rig Player Left Glove", playerGloveTex, playerLeftGlove, null,
                .43f, .49f, Vector3.zero, .034f, false, true);
            AddFixedRigPart(shader, "EV Rig Player Right Glove", playerGloveTex, playerRightGlove, null,
                .43f, .49f, Vector3.zero, .035f, true, true);

            return _rigParts.Count == ExpectedVisualPartCount;
        }

        static Texture2D Segment(string name)
        {
            return Resources.Load<Texture2D>("EV/ReferenceSegments/" + name);
        }

        void AddFixedRigPart(Shader shader, string name, Texture2D texture, Transform anchor, Transform secondary,
            float width, float height, Vector3 offset, float cameraBias, bool mirror, bool followBone)
        {
            Transform visual = CreateRigQuad(name, CreateRigMaterial(shader, texture), mirror);
            _rigParts.Add(new RigPart
            {
                Visual = visual,
                Anchor = anchor,
                Secondary = secondary,
                Offset = offset,
                Width = width,
                Height = height,
                CameraBias = cameraBias,
                FollowBone = followBone
            });
        }

        void AddBoneRigPart(Shader shader, string name, Texture2D texture, Transform anchor,
            float width, float heightFromLength, float cameraBias, bool mirror)
        {
            Transform visual = CreateRigQuad(name, CreateRigMaterial(shader, texture), mirror);
            _rigParts.Add(new RigPart
            {
                Visual = visual,
                Anchor = anchor,
                Width = width,
                HeightFromLength = heightFromLength,
                CameraBias = cameraBias,
                FollowBone = true
            });
        }

        Material CreateRigMaterial(Shader shader, Texture2D texture)
        {
            var material = new Material(shader) { mainTexture = texture };
            _materials.Add(material);
            return material;
        }

        Transform CreateRigQuad(string name, Material material, bool mirror)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            Mesh mesh = RigQuad(name + " Mesh", mirror);
            _meshes.Add(mesh);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = 40;
            return go.transform;
        }

        static Mesh RigQuad(string name, bool mirror)
        {
            float u0 = mirror ? 1f : 0f;
            float u1 = mirror ? 0f : 1f;
            var mesh = new Mesh { name = name };
            mesh.vertices = new[]
            {
                new Vector3(-.5f,-.5f,0f), new Vector3(.5f,-.5f,0f),
                new Vector3(.5f,.5f,0f), new Vector3(-.5f,.5f,0f)
            };
            mesh.uv = new[]
            {
                new Vector2(u0,0f), new Vector2(u1,0f),
                new Vector2(u1,1f), new Vector2(u0,1f)
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static void DisableRenderers(Transform root)
        {
            if (root == null) return;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = false;
        }

        void LateUpdate()
        {
            if (!_ready || _camera == null) return;
            UpdateRigVisuals();
        }

        void UpdateRigVisuals()
        {
            foreach (RigPart part in _rigParts)
            {
                Vector3 position = part.Secondary == null
                    ? part.Anchor.position
                    : Vector3.Lerp(part.Anchor.position, part.Secondary.position, .55f);
                position += _opponent.rotation * part.Offset;

                Vector3 normal = _camera.transform.position - position;
                if (normal.sqrMagnitude < .0001f) normal = -_camera.transform.forward;
                normal.Normalize();
                position += normal * part.CameraBias;

                // Use the anatomical/world up vector instead of camera.up. Phone/head roll
                // therefore cannot rotate the opponent like a single cardboard billboard.
                Vector3 up = part.FollowBone ? part.Anchor.up : Vector3.up;
                up = Vector3.ProjectOnPlane(up, normal);
                if (up.sqrMagnitude < .001f) up = Vector3.ProjectOnPlane(Vector3.up, normal);
                if (up.sqrMagnitude < .001f) up = part.Anchor.up;
                up.Normalize();

                float height = part.Height;
                if (part.HeightFromLength > 0f)
                    height = Mathf.Max(part.Anchor.lossyScale.y * 2f, .12f) * part.HeightFromLength;

                part.Visual.SetPositionAndRotation(position, Quaternion.LookRotation(normal, up));
                part.Visual.localScale = new Vector3(part.Width, height, 1f);
            }
        }

        void OnDestroy()
        {
            foreach (Mesh mesh in _meshes) if (mesh != null) Destroy(mesh);
            foreach (Material material in _materials) if (material != null) Destroy(material);
        }
    }
}
