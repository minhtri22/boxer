using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoxerP0
{
    // Presentation-only adapter. The qualified Round2 rig remains the sole writer
    // of combat/contact transforms; Blender bones follow those anchors afterward.
    [DefaultExecutionOrder(282)]
    public sealed class Blender3DVisualFollower : MonoBehaviour
    {
        sealed class SegmentDriver
        {
            readonly Transform _bone;
            readonly Vector3 _restDirection;
            readonly Quaternion _restRotation;

            public SegmentDriver(Transform bone)
            {
                _bone = bone;
                _restDirection = bone.up;
                _restRotation = bone.rotation;
            }

            public Transform Bone => _bone;

            public void Apply(Vector3 start, Vector3 end)
            {
                Vector3 direction = end - start;
                if (direction.sqrMagnitude < 1e-8f) return;
                _bone.SetPositionAndRotation(
                    start,
                    Quaternion.FromToRotation(_restDirection, direction.normalized) * _restRotation);
            }
        }

        sealed class OrientationDriver
        {
            readonly Transform _bone;
            readonly Quaternion _sourceToBone;

            public OrientationDriver(Transform bone, Quaternion sourceRotation)
            {
                _bone = bone;
                _sourceToBone = Quaternion.Inverse(sourceRotation) * bone.rotation;
            }

            public void Apply(Vector3 position, Quaternion sourceRotation)
            {
                _bone.SetPositionAndRotation(position, sourceRotation * _sourceToBone);
            }
        }

        sealed class PlayerRig
        {
            public GameObject Instance;
            public Transform RootBone, ForearmBone, HandBone;
            public Quaternion RootCorrection, ForearmCorrection, HandCorrection;
        }

        OpponentBoxer _opponent;
        PlayerBoxer _player;
        Transform _opponentModel;
        Transform _modelRoot, _pelvis, _spine, _chest, _neck, _head;
        SegmentDriver _leftClavicle, _rightClavicle, _leftUpper, _rightUpper, _leftForearm, _rightForearm;
        SegmentDriver _leftHand, _rightHand, _leftThigh, _rightThigh, _leftShin, _rightShin;
        OrientationDriver _leftFoot, _rightFoot;
        PlayerRig _playerLeft, _playerRight;
        readonly List<GameObject> _instances = new();
        readonly List<Renderer> _hiddenRenderers = new();

        public bool Ready { get; private set; }
        public Transform OpponentVisualRoot => _opponentModel;
        public int VisibleRendererCount { get; private set; }
        public float MaxAnchorError { get; private set; }

        public bool Initialize()
        {
            GameObject opponentPrefab = Resources.Load<GameObject>("Boxer3D/Ramirez_UAT3");
            GameObject glovePrefab = Resources.Load<GameObject>("Boxer3D/PlayerPOVGlove_UAT3");
            if (opponentPrefab == null || glovePrefab == null) return false;

            _opponent = FindFirstObjectByType<OpponentBoxer>();
            _player = FindFirstObjectByType<PlayerBoxer>();
            if (_opponent == null || _player == null) return false;

            GameObject opponentInstance = Instantiate(opponentPrefab);
            opponentInstance.name = "Blender Ramirez UAT3";
            _instances.Add(opponentInstance);
            _opponentModel = opponentInstance.transform;
            _opponentModel.SetPositionAndRotation(_opponent.transform.position, _opponent.transform.rotation);
            _opponentModel.localScale = Vector3.one;

            Transform rig = FindDeep(_opponentModel, "RamirezRig");
            _modelRoot = Require(rig, "root");
            _pelvis = Require(rig, "pelvis");
            _spine = Require(rig, "spine");
            _chest = Require(rig, "chest");
            _neck = Require(rig, "neck");
            _head = Require(rig, "head");
            _leftClavicle = new SegmentDriver(Require(rig, "clavicle.L"));
            _rightClavicle = new SegmentDriver(Require(rig, "clavicle.R"));
            _leftUpper = new SegmentDriver(Require(rig, "upper_arm.L"));
            _rightUpper = new SegmentDriver(Require(rig, "upper_arm.R"));
            _leftForearm = new SegmentDriver(Require(rig, "forearm.L"));
            _rightForearm = new SegmentDriver(Require(rig, "forearm.R"));
            _leftHand = new SegmentDriver(Require(rig, "hand.L"));
            _rightHand = new SegmentDriver(Require(rig, "hand.R"));
            _leftThigh = new SegmentDriver(Require(rig, "thigh.L"));
            _rightThigh = new SegmentDriver(Require(rig, "thigh.R"));
            _leftShin = new SegmentDriver(Require(rig, "shin.L"));
            _rightShin = new SegmentDriver(Require(rig, "shin.R"));

            Transform leftShoe = Require(_opponent.transform, "Left Shoe");
            Transform rightShoe = Require(_opponent.transform, "Right Shoe");
            _leftFoot = new OrientationDriver(Require(rig, "foot.L"), leftShoe.rotation);
            _rightFoot = new OrientationDriver(Require(rig, "foot.R"), rightShoe.rotation);

            _playerLeft = CreatePlayerRig(glovePrefab, "Blender Player Left POV", true);
            _playerRight = CreatePlayerRig(glovePrefab, "Blender Player Right POV", false);

            HideAuthoritativeVisuals(_opponent.transform);
            HideAuthoritativeVisuals(_player.transform);
            ApplyReferenceMaterials(opponentInstance);
            ApplyPlayerMaterials(_playerLeft.Instance);
            ApplyPlayerMaterials(_playerRight.Instance);

            Ready = true;
            LateUpdate();
            return true;
        }

        PlayerRig CreatePlayerRig(GameObject prefab, string name, bool left)
        {
            GameObject instance = Instantiate(prefab);
            instance.name = name;
            _instances.Add(instance);
            Transform rig = FindDeep(instance.transform, "POVGloveRig");
            var result = new PlayerRig
            {
                Instance = instance,
                RootBone = Require(rig, "root"),
                ForearmBone = Require(rig, "forearm"),
                HandBone = Require(rig, "hand")
            };
            Transform elbow = Require(_player.transform, left ? "Left Elbow" : "Right Elbow");
            Transform glove = Require(_player.transform, (left ? "Player Left" : "Player Right") + " Glove");
            Vector3 direction = (glove.position - elbow.position).normalized;
            result.RootCorrection = Quaternion.Inverse(Quaternion.LookRotation(direction, _player.transform.up)) * result.RootBone.rotation;
            result.ForearmCorrection = Quaternion.Inverse(Quaternion.LookRotation(direction, _player.transform.up)) * result.ForearmBone.rotation;
            result.HandCorrection = Quaternion.Inverse(Quaternion.LookRotation(direction, _player.transform.up)) * result.HandBone.rotation;
            return result;
        }

        void LateUpdate()
        {
            if (!Ready) return;

            _opponentModel.SetPositionAndRotation(_opponent.transform.position, _opponent.transform.rotation);
            _modelRoot.SetPositionAndRotation(_opponent.transform.position, _opponent.transform.rotation);

            Transform r2Pelvis = Require(_opponent.transform, "R2 Pelvis");
            Transform r2Chest = Require(_opponent.transform, "R2 Chest");
            Transform r2Neck = Require(_opponent.transform, "R2 Neck");
            Transform headAnchor = Require(_opponent.transform, "Opponent Head");
            Transform shorts = Require(_opponent.transform, "Opponent Shorts Visual");

            Vector3 up = _opponent.transform.up;
            _pelvis.SetPositionAndRotation(r2Pelvis.position + up * .01f, shorts.rotation);
            _spine.SetPositionAndRotation(Vector3.Lerp(r2Pelvis.position, r2Chest.position, .43f), r2Chest.rotation);
            _chest.SetPositionAndRotation(r2Chest.position - up * .12f, r2Chest.rotation);
            _neck.SetPositionAndRotation(r2Neck.position - up * .07f, r2Chest.rotation);
            _head.SetPositionAndRotation(headAnchor.position - up * .095f, _opponent.transform.rotation);

            DriveArm(true, _leftClavicle, _leftUpper, _leftForearm, _leftHand);
            DriveArm(false, _rightClavicle, _rightUpper, _rightForearm, _rightHand);
            DriveLeg(true, _leftThigh, _leftShin, _leftFoot);
            DriveLeg(false, _rightThigh, _rightShin, _rightFoot);
            DrivePlayer(true, _playerLeft);
            DrivePlayer(false, _playerRight);

            MaxAnchorError = Mathf.Max(MaxAnchorError,
                Vector3.Distance(_leftHand.Bone.position, Require(_opponent.transform, "Opponent Left Glove").position));
        }

        void DriveArm(bool left, SegmentDriver clavicle, SegmentDriver upper, SegmentDriver forearm, SegmentDriver hand)
        {
            string side = left ? "Left" : "Right";
            Transform shoulder = Require(_opponent.transform, side + " Shoulder");
            Transform elbow = Require(_opponent.transform, side + " Elbow");
            Transform glove = Require(_opponent.transform, "Opponent " + side + " Glove");
            Vector3 chest = Require(_opponent.transform, "R2 Chest").position;
            Vector3 clavicleStart = Vector3.Lerp(chest, shoulder.position, .36f);
            clavicle.Apply(clavicleStart, shoulder.position);
            upper.Apply(shoulder.position, elbow.position);
            forearm.Apply(elbow.position, glove.position);
            Vector3 dir = (glove.position - elbow.position).normalized;
            hand.Apply(glove.position, glove.position + dir * .11f);
        }

        void DriveLeg(bool left, SegmentDriver thigh, SegmentDriver shin, OrientationDriver foot)
        {
            string side = left ? "Left" : "Right";
            Transform thighAnchor = Require(_opponent.transform, side + " Thigh");
            Transform knee = Require(_opponent.transform, side + " Knee");
            Transform shoe = Require(_opponent.transform, side + " Shoe");
            Vector3 hip = thighAnchor.position * 2f - knee.position;
            thigh.Apply(hip, knee.position);
            shin.Apply(knee.position, shoe.position);
            foot.Apply(shoe.position, shoe.rotation);
        }

        void DrivePlayer(bool left, PlayerRig rig)
        {
            string side = left ? "Left" : "Right";
            Transform elbow = Require(_player.transform, side + " Elbow");
            Transform glove = Require(_player.transform, "Player " + side + " Glove");
            Vector3 direction = (glove.position - elbow.position).normalized;
            Quaternion aim = Quaternion.LookRotation(direction, _player.transform.up);
            Vector3 handHead = glove.position - direction * .12f;
            rig.RootBone.SetPositionAndRotation(elbow.position - direction * .05f, aim * rig.RootCorrection);
            rig.ForearmBone.SetPositionAndRotation(elbow.position, aim * rig.ForearmCorrection);
            rig.HandBone.SetPositionAndRotation(handHead, aim * rig.HandCorrection);
        }

        void HideAuthoritativeVisuals(Transform root)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.enabled) continue;
                renderer.enabled = false;
                _hiddenRenderers.Add(renderer);
            }
        }

        void ApplyReferenceMaterials(GameObject instance)
        {
            Texture2D headTexture = Resources.Load<Texture2D>("EV/RamirezHead");
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.name.Equals("Head", StringComparison.OrdinalIgnoreCase))
                    ApplyTexture(renderer, headTexture);
            }
            VisibleRendererCount += CountVisible(instance);
        }

        void ApplyPlayerMaterials(GameObject instance)
        {
            Texture2D glove = Resources.Load<Texture2D>("EV/GloveLeather");
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.name.Contains("POVGlove") || renderer.name.Equals("POVCuff", StringComparison.OrdinalIgnoreCase))
                    ApplyTexture(renderer, glove);
            }
            VisibleRendererCount += CountVisible(instance);
        }

        static void ApplyTexture(Renderer renderer, Texture2D texture)
        {
            if (texture == null) return;
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_MainTex")) material.mainTexture = texture;
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            }
        }

        static int CountVisible(GameObject instance)
        {
            int count = 0;
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                if (renderer.enabled) count++;
            return count;
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }

        static Transform Require(Transform root, string name)
        {
            Transform value = FindDeep(root, name);
            if (value == null) throw new InvalidOperationException("Missing Blender/Round2 visual anchor: " + name);
            return value;
        }

        void OnDestroy()
        {
            foreach (Renderer renderer in _hiddenRenderers)
                if (renderer != null) renderer.enabled = true;
            foreach (GameObject instance in _instances)
                if (instance != null) Destroy(instance);
        }
    }
}
