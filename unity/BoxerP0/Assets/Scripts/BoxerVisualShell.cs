using UnityEngine;

namespace BoxerP0
{
    /// <summary>
    /// Lightweight P0.5 visual shell for the Web surrogate.
    /// Pure presentation only: no combat state, hit resolution, input mapping, or P1 systems.
    /// Everything is generated from cheap primitives so the shell can be removed/replaced later.
    /// </summary>
    public sealed class BoxerVisualShell : MonoBehaviour
    {
        private Phase0Telemetry _telemetry;
        private float _observedBoutStart = -1f;

        private GUIStyle _hudLabel;
        private GUIStyle _hudSmall;
        private GUIStyle _timerStyle;

        private const float TestBoutSeconds = 45f;

        private static readonly Color Charcoal = new(0.045f, 0.050f, 0.060f, 1f);
        private static readonly Color Brick = new(0.12f, 0.065f, 0.055f, 1f);
        private static readonly Color Canvas = new(0.29f, 0.27f, 0.23f, 1f);
        private static readonly Color Rope = new(0.43f, 0.35f, 0.27f, 1f);
        private static readonly Color Gold = new(0.90f, 0.62f, 0.15f, 1f);
        private static readonly Color GloveBlack = new(0.035f, 0.040f, 0.045f, 1f);
        private static readonly Color Skin = new(0.56f, 0.31f, 0.22f, 1f);
        private static readonly Color OpponentShorts = new(0.18f, 0.19f, 0.21f, 1f);
        private static readonly Color CrowdDark = new(0.055f, 0.060f, 0.070f, 1f);
        private static readonly Color WarmLight = new(1.00f, 0.67f, 0.25f, 1f);

        private void Start()
        {
            _telemetry = FindFirstObjectByType<Phase0Telemetry>();
            BuildWarehouseShell();
            RestyleExistingRing();
            RestylePlayerGloves();
            RestyleOpponent();
        }

        private void Update()
        {
            if (_telemetry == null) return;
            if (_observedBoutStart < 0f && _telemetry.BoutResult == "IN_PROGRESS")
            {
                _observedBoutStart = Time.unscaledTime;
            }
            else if (_telemetry.BoutResult != "IN_PROGRESS" && _telemetry.BoutResult != "PENDING")
            {
                // Preserve the last start so the timer stays at zero on the result screen.
            }
        }

        private void BuildWarehouseShell()
        {
            // Back/side walls: large unlit slabs, no colliders.
            CreateDecor(PrimitiveType.Cube, "Warehouse Back Wall", new Vector3(0f, 2.4f, 4.4f), new Vector3(8.0f, 4.8f, 0.18f), Brick);
            CreateDecor(PrimitiveType.Cube, "Warehouse Left Wall", new Vector3(-4.1f, 2.4f, 0.3f), new Vector3(0.18f, 4.8f, 8.0f), Charcoal);
            CreateDecor(PrimitiveType.Cube, "Warehouse Right Wall", new Vector3(4.1f, 2.4f, 0.3f), new Vector3(0.18f, 4.8f, 8.0f), Charcoal);
            CreateDecor(PrimitiveType.Cube, "Warehouse Ceiling", new Vector3(0f, 4.75f, 0.3f), new Vector3(8.2f, 0.12f, 8.2f), Charcoal);

            // Cheap overhead lamp + warm bulbs. Unlit emissive-look colors, not real lights.
            CreateDecor(PrimitiveType.Cylinder, "Hanging Lamp", new Vector3(0f, 3.7f, 1.3f), new Vector3(0.42f, 0.08f, 0.42f), new Color(0.14f, 0.12f, 0.09f, 1f));
            CreateDecor(PrimitiveType.Sphere, "Hanging Lamp Glow", new Vector3(0f, 3.58f, 1.3f), Vector3.one * 0.19f, WarmLight);

            for (int i = -4; i <= 4; i++)
            {
                float x = i * 0.78f;
                CreateDecor(PrimitiveType.Sphere, "String Bulb", new Vector3(x, 3.05f + Mathf.Abs(i) * 0.04f, 2.5f), Vector3.one * 0.075f, WarmLight);
            }

            // Crates/tires imply an underground venue without expensive assets.
            CreateDecor(PrimitiveType.Cube, "Crate L", new Vector3(-3.0f, 0.35f, 2.7f), new Vector3(0.75f, 0.70f, 0.75f), new Color(0.24f, 0.15f, 0.08f, 1f));
            CreateDecor(PrimitiveType.Cube, "Crate R", new Vector3(3.05f, 0.27f, 2.9f), new Vector3(0.62f, 0.54f, 0.62f), new Color(0.20f, 0.13f, 0.07f, 1f));

            BuildCrowdRow(-3.30f, 1f);
            BuildCrowdRow(3.30f, -1f);
            BuildBackCrowd();
        }

        private void BuildCrowdRow(float x, float facingSign)
        {
            for (int i = 0; i < 7; i++)
            {
                float z = -1.6f + i * 0.72f;
                float height = 1.45f + (i % 3) * 0.10f;
                GameObject body = CreateDecor(PrimitiveType.Capsule, "Crowd Silhouette", new Vector3(x, height * 0.48f, z), new Vector3(0.34f, height * 0.48f, 0.28f), CrowdDark);
                body.transform.rotation = Quaternion.Euler(0f, facingSign > 0f ? 90f : -90f, 0f);
                CreateDecor(PrimitiveType.Sphere, "Crowd Head", new Vector3(x, height + 0.05f, z), Vector3.one * 0.22f, new Color(0.11f, 0.095f, 0.085f, 1f));
            }
        }

        private void BuildBackCrowd()
        {
            for (int i = 0; i < 9; i++)
            {
                float x = -2.8f + i * 0.70f;
                float z = 3.25f;
                float h = 1.45f + ((i + 1) % 4) * 0.08f;
                CreateDecor(PrimitiveType.Capsule, "Back Crowd", new Vector3(x, h * 0.48f, z), new Vector3(0.30f, h * 0.48f, 0.25f), CrowdDark);
                CreateDecor(PrimitiveType.Sphere, "Back Crowd Head", new Vector3(x, h + 0.04f, z), Vector3.one * 0.20f, new Color(0.10f, 0.085f, 0.075f, 1f));
            }
        }

        private void RestyleExistingRing()
        {
            GameObject floor = GameObject.Find("Neutral Ring Floor");
            if (floor != null)
            {
                floor.name = "Underground Ring Canvas";
                ApplyColor(floor.GetComponent<Renderer>(), Canvas);
            }

            GameObject[] roots = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject go in roots)
            {
                if (go.name == "Ring Rope") ApplyColor(go.GetComponent<Renderer>(), Rope);
            }

            // Generic championship motif; intentionally no real-world federation branding.
            CreateDecor(PrimitiveType.Cylinder, "Generic Championship Medallion", new Vector3(0f, 0.018f, -1.65f), new Vector3(0.48f, 0.018f, 0.48f), Gold).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            CreateDecor(PrimitiveType.Cube, "Generic Championship Strap", new Vector3(0f, 0.010f, -1.65f), new Vector3(1.85f, 0.018f, 0.30f), new Color(0.04f, 0.18f, 0.10f, 1f));
        }

        private void RestylePlayerGloves()
        {
            RestyleGlove("Player Left Glove");
            RestyleGlove("Player Right Glove");
        }

        private void RestyleGlove(string name)
        {
            GameObject glove = GameObject.Find(name);
            if (glove == null) return;
            ApplyColor(glove.GetComponent<Renderer>(), GloveBlack);

            GameObject cuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cuff.name = name + " Gold Cuff";
            cuff.transform.SetParent(glove.transform, false);
            cuff.transform.localPosition = new Vector3(0f, -0.54f, 0f);
            cuff.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cuff.transform.localScale = new Vector3(0.82f, 0.15f, 0.82f);
            DisableCollider(cuff);
            ApplyColor(cuff.GetComponent<Renderer>(), Gold);

            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name + " Gold Panel";
            panel.transform.SetParent(glove.transform, false);
            panel.transform.localPosition = new Vector3(0f, 0.03f, 0.48f);
            panel.transform.localScale = new Vector3(0.58f, 0.58f, 0.08f);
            DisableCollider(panel);
            ApplyColor(panel.GetComponent<Renderer>(), new Color(0.72f, 0.47f, 0.10f, 1f));
        }

        private void RestyleOpponent()
        {
            GameObject body = GameObject.Find("Opponent Body");
            GameObject head = GameObject.Find("Opponent Head");
            GameObject root = GameObject.Find("Opponent");
            if (body != null) ApplyColor(body.GetComponent<Renderer>(), Skin);
            if (head != null) ApplyColor(head.GetComponent<Renderer>(), new Color(0.48f, 0.25f, 0.18f, 1f));
            if (root == null) return;

            GameObject shorts = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shorts.name = "Opponent Shorts Visual";
            shorts.transform.SetParent(root.transform, false);
            shorts.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            shorts.transform.localScale = new Vector3(0.72f, 0.33f, 0.48f);
            DisableCollider(shorts);
            ApplyColor(shorts.GetComponent<Renderer>(), OpponentShorts);

            GameObject waistband = GameObject.CreatePrimitive(PrimitiveType.Cube);
            waistband.name = "Opponent Gold Waistband";
            waistband.transform.SetParent(root.transform, false);
            waistband.transform.localPosition = new Vector3(0f, 0.91f, 0f);
            waistband.transform.localScale = new Vector3(0.75f, 0.08f, 0.50f);
            DisableCollider(waistband);
            ApplyColor(waistband.GetComponent<Renderer>(), Gold);

            AddOpponentShoulder(root.transform, new Vector3(-0.38f, 1.37f, 0f));
            AddOpponentShoulder(root.transform, new Vector3(0.38f, 1.37f, 0f));

            GameObject left = GameObject.Find("Opponent Left Glove");
            GameObject right = GameObject.Find("Opponent Right Glove");
            if (left != null) ApplyColor(left.GetComponent<Renderer>(), new Color(0.72f, 0.66f, 0.52f, 1f));
            if (right != null) ApplyColor(right.GetComponent<Renderer>(), new Color(0.72f, 0.66f, 0.52f, 1f));
        }

        private void AddOpponentShoulder(Transform root, Vector3 localPosition)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shoulder.name = "Opponent Shoulder Visual";
            shoulder.transform.SetParent(root, false);
            shoulder.transform.localPosition = localPosition;
            shoulder.transform.localScale = new Vector3(0.30f, 0.38f, 0.30f);
            DisableCollider(shoulder);
            ApplyColor(shoulder.GetComponent<Renderer>(), Skin);
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawGameHud();
        }

        private void EnsureStyles()
        {
            if (_hudLabel != null) return;
            _hudLabel = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 48, 12, 24),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _hudSmall = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.height / 70, 10, 18),
                alignment = TextAnchor.MiddleLeft
            };
            _timerStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = Mathf.Clamp(Screen.height / 44, 14, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void DrawGameHud()
        {
            float margin = Mathf.Max(12f, Screen.width * 0.025f);
            float hudWidth = Mathf.Min(280f, Screen.width * 0.36f);
            float barWidth = hudWidth - 10f;
            float top = Mathf.Max(12f, Screen.height * 0.018f);

            DrawFighterHud(new Rect(margin, top, hudWidth, 88f), "LV 12  BOXER", barWidth, true);
            DrawFighterHud(new Rect(Screen.width - margin - hudWidth, top, hudWidth, 88f), "LV 15  OPPONENT", barWidth, false);

            string timerText = "TRAINING";
            if (_telemetry != null && _telemetry.BoutResult == "IN_PROGRESS" && _observedBoutStart >= 0f)
            {
                float remaining = Mathf.Max(0f, TestBoutSeconds - (Time.unscaledTime - _observedBoutStart));
                timerText = $"0:{Mathf.CeilToInt(remaining):00}";
            }
            else if (_telemetry != null && _telemetry.BoutResult != "PENDING" && _telemetry.BoutResult != "IN_PROGRESS")
            {
                timerText = "0:00";
            }

            float timerWidth = Mathf.Min(115f, Screen.width * 0.17f);
            GUI.Box(new Rect((Screen.width - timerWidth) * 0.5f, top, timerWidth, 46f), timerText, _timerStyle);
            GUI.Label(new Rect((Screen.width - 170f) * 0.5f, top + 48f, 170f, 22f), "P0 TEST HUD", _hudSmall);
        }

        private void DrawFighterHud(Rect rect, string title, float barWidth, bool player)
        {
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 3f, rect.width - 16f, 24f), title, _hudLabel);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 28f, 68f, 18f), "HP", _hudSmall);
            DrawBar(new Rect(rect.x + 42f, rect.y + 31f, barWidth - 42f, 12f), player ? 0.72f : 0.82f, new Color(0.78f, 0.10f, 0.07f, 1f));
            GUI.Label(new Rect(rect.x + 8f, rect.y + 51f, 68f, 18f), "STAMINA", _hudSmall);
            DrawBar(new Rect(rect.x + 74f, rect.y + 54f, barWidth - 74f, 12f), player ? 0.66f : 0.74f, new Color(0.94f, 0.67f, 0.10f, 1f));
        }

        private static void DrawBar(Rect rect, float fill, Color color)
        {
            Color old = GUI.color;
            GUI.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = color;
            Rect filled = rect;
            filled.width *= Mathf.Clamp01(fill);
            GUI.DrawTexture(filled, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static GameObject CreateDecor(PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            DisableCollider(go);
            ApplyColor(go.GetComponent<Renderer>(), color);
            return go;
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
    }
}
