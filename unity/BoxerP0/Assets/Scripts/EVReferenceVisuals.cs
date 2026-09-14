using System.Collections.Generic;
using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// Presentation-only visual shell for P1-EV.
    /// Reads accepted combat anchors/action state and never writes gameplay transforms.
    /// </summary>
    [DefaultExecutionOrder(300)]
    public sealed class EVReferenceVisuals : MonoBehaviour
    {
        readonly List<Mesh> _meshes = new List<Mesh>();
        readonly List<Material> _materials = new List<Material>();

        Camera _camera;
        OpponentBoxer _opponentBoxer;
        Transform _opponent;
        Transform _player;
        Transform _leftPlayerGlove;
        Transform _rightPlayerGlove;

        Transform _opponentVisual;
        Transform _leftPlayerVisual;
        Transform _rightPlayerVisual;

        Material _hidden;
        Material _opponentMaterial;
        Material _playerGloveMaterial;
        Texture2D _hiddenKey;
        Texture2D _guard;
        Texture2D _straight;
        Texture2D _hook;
        Texture2D _playerGlove;

        bool _ready;

        public bool Ready => _ready;
        public int VisualPartCount => _ready ? 3 : 0;

        public void Initialize(Transform opponent, Transform player, Transform contactTorso)
        {
            _opponent = opponent;
            _player = player;
            _camera = Camera.main ?? FindAnyObjectByType<Camera>();
            _opponentBoxer = opponent != null ? opponent.GetComponent<OpponentBoxer>() : null;

            if (_opponent == null || _player == null || _camera == null || _opponentBoxer == null) return;

            _leftPlayerGlove = _player.Find("Player Left Glove");
            _rightPlayerGlove = _player.Find("Player Right Glove");
            if (_leftPlayerGlove == null || _rightPlayerGlove == null) return;

            _guard = Resources.Load<Texture2D>("P1V/ramirez-guard-chroma");
            _straight = Resources.Load<Texture2D>("P1V/ramirez-straight-chroma");
            _hook = Resources.Load<Texture2D>("P1V/ramirez-hook-chroma");
            _playerGlove = Resources.Load<Texture2D>("P1V/player-glove-left-chroma");
            Shader chroma = Resources.Load<Shader>("BoxerP1VChromaKey");
            if (_guard == null || _straight == null || _hook == null || _playerGlove == null || chroma == null) return;

            CreateHiddenMaterial();
            HideLegacyRenderers(_opponent);
            HideLegacyRenderers(_player);

            _opponentMaterial = CreateChromaMaterial(chroma, _guard);
            _playerGloveMaterial = CreateChromaMaterial(chroma, _playerGlove);

            // The camera sees the generated quad from its back side in the current
            // Round-2 camera convention. Compensate in UV space so branded details
            // such as RAMIREZ read normally in the final camera capture.
            _opponentVisual = CreateQuad("EV Ramirez Full Body", _opponentMaterial, 30, true);
            _leftPlayerVisual = CreateQuad("EV Player Left Glove", _playerGloveMaterial, 90, false);
            _rightPlayerVisual = CreateQuad("EV Player Right Glove", _playerGloveMaterial, 90, true);

            _ready = _opponentVisual != null && _leftPlayerVisual != null && _rightPlayerVisual != null;
            if (_ready) UpdateVisuals();
        }

        void CreateHiddenMaterial()
        {
            Shader shader = Resources.Load<Shader>("BoxerP1VChromaKey");
            _hidden = new Material(shader);
            _hiddenKey = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _hiddenKey.SetPixel(0, 0, Color.green);
            _hiddenKey.Apply(false, true);
            _hidden.mainTexture = _hiddenKey;
            _hidden.SetFloat("_Threshold", 0.01f);
            _hidden.SetFloat("_Softness", 0.01f);
            _materials.Add(_hidden);
        }

        Material CreateChromaMaterial(Shader shader, Texture2D texture)
        {
            var material = new Material(shader) { mainTexture = texture };
            material.SetFloat("_Threshold", 0.18f);
            material.SetFloat("_Softness", 0.20f);
            _materials.Add(material);
            return material;
        }

        void HideLegacyRenderers(Transform root)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                if (renderer.enabled) renderer.sharedMaterial = _hidden;
        }

        Transform CreateQuad(string name, Material material, int order, bool mirror)
        {
            var go = new GameObject(name);
            Mesh mesh = Quad(name + " Mesh", mirror);
            _meshes.Add(mesh);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            return go.transform;
        }

        static Mesh Quad(string name, bool mirror)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = new[]
            {
                new Vector3(-.5f,-.5f,0), new Vector3(.5f,-.5f,0),
                new Vector3(.5f,.5f,0), new Vector3(-.5f,.5f,0)
            };
            mesh.uv = mirror
                ? new[] { new Vector2(1,0), new Vector2(0,0), new Vector2(0,1), new Vector2(1,1) }
                : new[] { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        void LateUpdate()
        {
            if (!_ready || _camera == null) return;
            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            UpdateOpponentVisual();
            UpdatePlayerGlove(_leftPlayerVisual, _leftPlayerGlove, new Vector2(.20f, .16f));
            UpdatePlayerGlove(_rightPlayerVisual, _rightPlayerGlove, new Vector2(.80f, .16f));
        }

        void UpdateOpponentVisual()
        {
            Texture2D pose = _guard;
            if (_opponentBoxer.IsActionBusy)
                pose = P1VPresentationMath.PoseToken(_opponentBoxer.CurrentIntent) == "HOOK" ? _hook : _straight;
            if (_opponentMaterial.mainTexture != pose) _opponentMaterial.mainTexture = pose;

            // Presentation-only scale: the accepted mechanics root remains the
            // authority, while the full-body reference art is framed closer to the
            // supplied POV target. Keep the feet tied to the opponent root.
            float height = 2.45f;
            if (_opponentBoxer.IsActionBusy && _opponentBoxer.CurrentPhase == ActionPhase.Extend) height *= 1.025f;
            Vector3 center = _opponent.position + Vector3.up * (height * 0.5f);
            Vector3 towardCamera = (_camera.transform.position - center).normalized;
            _opponentVisual.position = center;
            _opponentVisual.rotation = Quaternion.LookRotation(towardCamera, _camera.transform.up);
            _opponentVisual.localScale = new Vector3(height * (2f / 3f), height, 1f);
        }

        void UpdatePlayerGlove(Transform visual, Transform anchor, Vector2 fallbackViewport)
        {
            Vector3 projected = _camera.WorldToViewportPoint(anchor.position);
            Vector2 anchorViewport = projected.z > 0.01f
                ? new Vector2(Mathf.Clamp(projected.x, .08f, .92f), Mathf.Clamp(projected.y, .06f, .30f))
                : fallbackViewport;
            Vector2 viewport = Vector2.Lerp(fallbackViewport, anchorViewport, .58f);

            const float depth = .62f;
            visual.position = _camera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, depth));
            visual.rotation = Quaternion.LookRotation((_camera.transform.position - visual.position).normalized, _camera.transform.up);

            float worldHeight = 2f * depth * Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * .5f) * .285f;
            float aspect = (float)_playerGlove.width / _playerGlove.height;
            visual.localScale = new Vector3(worldHeight * aspect, worldHeight, 1f);
        }

        void OnDestroy()
        {
            foreach (Mesh mesh in _meshes) if (mesh != null) Destroy(mesh);
            foreach (Material material in _materials) if (material != null) Destroy(material);
            if (_hiddenKey != null) Destroy(_hiddenKey);
        }
    }
}
