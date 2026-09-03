using UnityEngine;

namespace BoxerP0
{
    public sealed class BoxerBootstrap : MonoBehaviour
    {
        private BoxerInput _input;
        private PlayerBoxer _player;
        private OpponentBoxer _opponent;
        private Phase0Telemetry _telemetry;
        private BoxerFeedback _feedback;
        private float _instructionUntil;
        private float _boutEnd;
        private float _smokeQuitAt = -1f;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            _instructionUntil = Time.unscaledTime + 6f;
            _boutEnd = Time.unscaledTime + 75f;
            ConfigureSmokeQuit();
            BuildLightingAndRing();
            BuildActors();
        }

        private void Update()
        {
            if (_smokeQuitAt > 0f && Time.unscaledTime >= _smokeQuitAt)
            {
                Debug.Log("P0_SMOKE_COMPLETE");
                Application.Quit(0);
            }
        }

        private void ConfigureSmokeQuit()
        {
            foreach (string argument in System.Environment.GetCommandLineArgs())
            {
                const string prefix = "-p0SmokeSeconds=";
                if (!argument.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) continue;
                if (float.TryParse(argument.Substring(prefix.Length), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float seconds) && seconds > 0f)
                {
                    _smokeQuitAt = Time.unscaledTime + seconds;
                }
            }
        }

        private void BuildLightingAndRing()
        {
            if (FindFirstObjectByType<Light>() == null)
            {
                GameObject lightObject = new("Directional Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                lightObject.transform.rotation = Quaternion.Euler(45f, -25f, 0f);
            }

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Neutral Ring Floor";
            floor.transform.position = new Vector3(0f, -0.08f, 0.2f);
            floor.transform.localScale = new Vector3(5.2f, 0.15f, 5.2f);
            ApplyColor(floor.GetComponent<Renderer>(), new Color(0.16f, 0.18f, 0.20f));

            CreateBoundary(new Vector3(0f, 0.6f, 2.75f), new Vector3(5.4f, 0.08f, 0.08f));
            CreateBoundary(new Vector3(0f, 0.6f, -2.35f), new Vector3(5.4f, 0.08f, 0.08f));
            CreateBoundary(new Vector3(2.55f, 0.6f, 0.2f), new Vector3(0.08f, 0.08f, 5.2f));
            CreateBoundary(new Vector3(-2.55f, 0.6f, 0.2f), new Vector3(0.08f, 0.08f, 5.2f));
        }

        private void BuildActors()
        {
            GameObject systems = new("P0 Systems");
            _input = systems.AddComponent<BoxerInput>();
            _telemetry = systems.AddComponent<Phase0Telemetry>();
            _feedback = systems.AddComponent<BoxerFeedback>();

            GameObject playerRoot = new("Player Boxer");
            playerRoot.transform.position = new Vector3(0f, 0f, -0.7f);
            _player = playerRoot.AddComponent<PlayerBoxer>();

            GameObject headObject = new("Player Head");
            headObject.transform.SetParent(playerRoot.transform, false);
            headObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            SphereCollider headCollider = headObject.AddComponent<SphereCollider>();
            headCollider.radius = 0.18f;

            GameObject cameraObject = new("POV Camera");
            cameraObject.transform.SetParent(headObject.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0.02f, 0.01f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.04f;
            cameraObject.AddComponent<AudioListener>();

            GameObject bodyObject = new("Player Body Target");
            bodyObject.transform.SetParent(playerRoot.transform, false);
            bodyObject.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            SphereCollider bodyCollider = bodyObject.AddComponent<SphereCollider>();
            bodyCollider.radius = 0.29f;

            (Transform leftPlayerGlove, SphereCollider leftPlayerCollider) = CreateGlove(
                "Player Left Glove", playerRoot.transform, new Vector3(-0.22f, 1.38f, 0.48f), new Color(0.15f, 0.35f, 0.95f));
            (Transform rightPlayerGlove, SphereCollider rightPlayerCollider) = CreateGlove(
                "Player Right Glove", playerRoot.transform, new Vector3(0.22f, 1.38f, 0.48f), new Color(0.15f, 0.35f, 0.95f));

            GameObject opponentRoot = new("Opponent");
            opponentRoot.transform.position = new Vector3(0f, 0f, 2.15f);
            opponentRoot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            _opponent = opponentRoot.AddComponent<OpponentBoxer>();

            GameObject opponentBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            opponentBody.name = "Opponent Body";
            opponentBody.transform.SetParent(opponentRoot.transform, false);
            opponentBody.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            opponentBody.transform.localScale = new Vector3(0.58f, 0.62f, 0.42f);
            ApplyColor(opponentBody.GetComponent<Renderer>(), new Color(0.44f, 0.43f, 0.41f));
            Collider originalBodyCollider = opponentBody.GetComponent<Collider>();
            Destroy(originalBodyCollider);
            SphereCollider opponentBodyCollider = opponentBody.AddComponent<SphereCollider>();
            opponentBodyCollider.radius = 0.48f;

            GameObject opponentHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            opponentHead.name = "Opponent Head";
            opponentHead.transform.SetParent(opponentRoot.transform, false);
            opponentHead.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            opponentHead.transform.localScale = Vector3.one * 0.36f;
            ApplyColor(opponentHead.GetComponent<Renderer>(), new Color(0.70f, 0.57f, 0.48f));
            SphereCollider opponentHeadCollider = opponentHead.GetComponent<SphereCollider>();
            opponentHeadCollider.radius = 0.5f;

            (Transform leftOpponentGlove, SphereCollider leftOpponentCollider) = CreateGlove(
                "Opponent Left Glove", opponentRoot.transform, new Vector3(-0.22f, 1.38f, -0.45f), new Color(0.90f, 0.16f, 0.12f));
            (Transform rightOpponentGlove, SphereCollider rightOpponentCollider) = CreateGlove(
                "Opponent Right Glove", opponentRoot.transform, new Vector3(0.22f, 1.38f, -0.45f), new Color(0.90f, 0.16f, 0.12f));

            _player.Initialize(
                _input,
                _opponent,
                _telemetry,
                headObject.transform,
                headCollider,
                bodyCollider,
                leftPlayerGlove,
                leftPlayerCollider,
                rightPlayerGlove,
                rightPlayerCollider);

            _opponent.Initialize(
                _player,
                _telemetry,
                leftOpponentGlove,
                leftOpponentCollider,
                rightOpponentGlove,
                rightOpponentCollider,
                opponentHeadCollider,
                opponentBodyCollider);

            _telemetry.InputSource = _input;
            _telemetry.Player = _player;
            _telemetry.Opponent = _opponent;
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = Mathf.Clamp(Screen.height / 45, 14, 24);
            GUI.skin.box.fontSize = GUI.skin.label.fontSize;

            if (Time.unscaledTime < _instructionUntil)
            {
                GUI.Box(new Rect(20, 20, Mathf.Min(Screen.width - 40, 650), 125),
                    "Left thumb moves.\nRight thumb punches.\nMove the phone to move your head.\nRelease actions to return to guard.");
            }

            if (_input == null || _player == null || _opponent == null || _telemetry == null) return;

            string debug =
                $"HEAD {_input.HeadAngleDegrees:F1}° → {_player.HeadOffset:F2}m [{_input.HeadInputSource}]\n" +
                $"MOVE {_input.MovementIntent.x:F2},{_input.MovementIntent.y:F2}\n" +
                $"PLAYER {_player.ActionLabel}  GUARD {(_player.GuardActive ? "HIGH" : "OPEN")}\n" +
                $"OPP {_opponent.ActionLabel}  COUNTER {(_opponent.CounterWindowOpen ? "OPEN" : "CLOSED")}\n" +
                $"LAST {_telemetry.LastOutcome} / {_telemetry.LastEvent}\n" +
                $"BOUT {Mathf.Max(0f, _boutEnd - Time.unscaledTime):F0}s";
            GUI.Box(new Rect(20, Screen.height - 190, Mathf.Min(Screen.width - 40, 680), 170), debug);

            if (!Application.isMobilePlatform)
            {
                GUI.Box(new Rect(Screen.width - 285, 20, 265, 155),
                    "EDITOR SYNTHETIC\nWASD feet · Q/E head\nJ jab · K cross · L hook\nR recalibrate · M audio · H haptic");
            }

            if (Time.unscaledTime >= _boutEnd)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 180, Screen.height * 0.5f - 55, 360, 110),
                    "BOUT COMPLETE\nDo you want to fight again?");
            }
        }

        private static (Transform transform, SphereCollider collider) CreateGlove(
            string name,
            Transform parent,
            Vector3 localPosition,
            Color color)
        {
            GameObject glove = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glove.name = name;
            glove.transform.SetParent(parent, false);
            glove.transform.localPosition = localPosition;
            glove.transform.localScale = new Vector3(0.24f, 0.21f, 0.28f);
            ApplyColor(glove.GetComponent<Renderer>(), color);
            SphereCollider collider = glove.GetComponent<SphereCollider>();
            collider.radius = 0.5f;
            return (glove.transform, collider);
        }

        private static void CreateBoundary(Vector3 position, Vector3 scale)
        {
            GameObject boundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boundary.name = "Ring Rope";
            boundary.transform.position = position;
            boundary.transform.localScale = scale;
            ApplyColor(boundary.GetComponent<Renderer>(), new Color(0.68f, 0.68f, 0.72f));
        }

        private static void ApplyColor(Renderer renderer, Color color)
        {
            Material material = renderer.material;
            if (material == null) return;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }
    }
}
