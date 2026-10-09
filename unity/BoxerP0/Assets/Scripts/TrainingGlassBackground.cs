using UnityEngine;

namespace BoxerP0
{
    // Built-in pipeline, presentation only. Capture once per frame, blur at reduced
    // resolution, and sample only behind training cards. The playable scene is sharp.
    [RequireComponent(typeof(Camera))]
    public sealed class TrainingGlassBackground : MonoBehaviour
    {
        Material _blur;
        RenderTexture _capture, _scratch, _glass;
        public Texture Background => _glass;
        public int BufferWidth => _glass == null ? 0 : _glass.width;
        public int BufferHeight => _glass == null ? 0 : _glass.height;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_blur == null)
                _blur = new Material(Resources.Load<Shader>("TrainingGlassBlur"));
            int width = Mathf.Min(256, Mathf.Max(1, source.width / 3));
            int height = Mathf.Min(512, Mathf.Max(1, source.height / 3));
            if (_glass == null || _glass.width != width || _glass.height != height)
            {
                ReleaseBuffers();
                _capture = Buffer(width, height); _scratch = Buffer(width, height); _glass = Buffer(width, height);
            }
            Graphics.Blit(source, _capture);
            _blur.SetVector("_Direction", new Vector4(1f / width, 0, 0, 0));
            Graphics.Blit(_capture, _scratch, _blur);
            _blur.SetVector("_Direction", new Vector4(0, 1f / height, 0, 0));
            Graphics.Blit(_scratch, _glass, _blur);
            Graphics.Blit(source, destination);
        }

        static RenderTexture Buffer(int width, int height)
        {
            var texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
                { name = "Training glass (no depth)", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            texture.Create(); return texture;
        }

        public void Draw(Rect card)
        {
            Color prior = GUI.color;
            if (_glass != null)
            {
                Vector3 top = GUI.matrix.MultiplyPoint3x4(new Vector3(card.x, card.y, 0));
                Vector3 bottom = GUI.matrix.MultiplyPoint3x4(new Vector3(card.xMax, card.yMax, 0));
                GUI.color = Color.white;
                GUI.DrawTextureWithTexCoords(card, _glass, new Rect(top.x / Screen.width,
                    1 - bottom.y / Screen.height, (bottom.x - top.x) / Screen.width,
                    (bottom.y - top.y) / Screen.height));
            }
            GUI.color = new Color(.012f, .016f, .019f, .34f);
            GUI.DrawTexture(card, Texture2D.whiteTexture);
            GUI.color = prior;
        }

        void ReleaseBuffers()
        {
            foreach (var texture in new[] { _capture, _scratch, _glass })
                if (texture != null) { texture.Release(); Destroy(texture); }
            _capture = _scratch = _glass = null;
        }
        void OnDisable() => ReleaseBuffers();
        void OnDestroy() { ReleaseBuffers(); if (_blur != null) Destroy(_blur); }
    }
}
