using UnityEngine;

namespace BoxerP0
{
    public readonly struct P1BodyRotationPose
    {
        public readonly float PelvisYawDegrees;
        public readonly float TorsoYawDegrees;
        public readonly float Amplitude;

        public P1BodyRotationPose(float pelvisYawDegrees, float torsoYawDegrees, float amplitude)
        {
            PelvisYawDegrees = pelvisYawDegrees;
            TorsoYawDegrees = torsoYawDegrees;
            Amplitude = amplitude;
        }
    }

    /// <summary>
    /// P1-C0 presentation-only hip/torso rotation baseline.
    /// It intentionally ignores punch family and never returns gameplay multipliers.
    /// </summary>
    public static class P1BodyRotationMath
    {
        public const float MaxPelvisYawDegrees = 7f;
        public const float MaxTorsoYawDegrees = 14f;
        public const float CommitEndAmplitude = 0.65f;

        public static P1BodyRotationPose Sample(PunchIntent intent, ActionPhase phase, float normalizedPhase)
        {
            if (intent == PunchIntent.None || phase == ActionPhase.Guard)
                return new P1BodyRotationPose(0f, 0f, 0f);

            float t = Smooth01(Mathf.Clamp01(normalizedPhase));
            float amplitude = phase switch
            {
                ActionPhase.Commit => Mathf.Lerp(0f, CommitEndAmplitude, t),
                ActionPhase.Extend => Mathf.Lerp(CommitEndAmplitude, 1f, t),
                ActionPhase.Recover => Mathf.Lerp(1f, 0f, t),
                _ => 0f
            };

            float sign = PunchLabels.IsRearHand(intent) ? 1f : -1f;
            return new P1BodyRotationPose(
                sign * MaxPelvisYawDegrees * amplitude,
                sign * MaxTorsoYawDegrees * amplitude,
                amplitude);
        }

        public static Vector3 RotateLocalYaw(Vector3 localPoint, float yawDegrees)
        {
            return Quaternion.Euler(0f, yawDegrees, 0f) * localPoint;
        }

        public static float OpponentPhaseDuration(ActionPhase phase)
        {
            return phase == ActionPhase.Commit ? 0.34f : phase == ActionPhase.Extend ? 0.17f : 0.48f;
        }

        private static float Smooth01(float t) => t * t * (3f - 2f * t);
    }

    /// <summary>
    /// Applies the P1-C0 state only to decorative opponent presentation.
    /// Combat/root geometry remains owned by OpponentBoxer.
    /// </summary>
    [DefaultExecutionOrder(210)]
    public sealed class OpponentBodyRotationEmbodiment : MonoBehaviour
    {
        private OpponentBoxer _opponent;
        private Transform _root;
        private Transform _shorts;
        private Transform _waistband;
        private Transform _leftShoulderVisual;
        private Transform _rightShoulderVisual;

        public P1BodyRotationPose CurrentPose { get; private set; }

        public void Initialize(OpponentBoxer opponent)
        {
            _opponent = opponent;
            _root = opponent != null ? opponent.transform : null;
            CurrentPose = new P1BodyRotationPose(0f, 0f, 0f);
        }

        private void Update()
        {
            if (_opponent == null || _root == null) return;

            ResolvePresentationAnchors();

            float phaseT = _opponent.IsActionBusy
                ? _opponent.ActionNormalizedPhase(P1BodyRotationMath.OpponentPhaseDuration(_opponent.CurrentPhase))
                : 0f;
            CurrentPose = P1BodyRotationMath.Sample(_opponent.CurrentIntent, _opponent.CurrentPhase, phaseT);

            Quaternion pelvisRotation = Quaternion.Euler(0f, CurrentPose.PelvisYawDegrees, 0f);
            if (_shorts != null) _shorts.localRotation = pelvisRotation;
            if (_waistband != null) _waistband.localRotation = pelvisRotation;

            if (_leftShoulderVisual != null)
            {
                _leftShoulderVisual.localPosition = P1BodyRotationMath.RotateLocalYaw(
                    new Vector3(-0.38f, 1.37f, 0f), CurrentPose.TorsoYawDegrees);
            }
            if (_rightShoulderVisual != null)
            {
                _rightShoulderVisual.localPosition = P1BodyRotationMath.RotateLocalYaw(
                    new Vector3(0.38f, 1.37f, 0f), CurrentPose.TorsoYawDegrees);
            }
        }

        private void ResolvePresentationAnchors()
        {
            if (_shorts == null) _shorts = _root.Find("Opponent Shorts Visual");
            if (_waistband == null) _waistband = _root.Find("Opponent Gold Waistband");
            if (_leftShoulderVisual != null && _rightShoulderVisual != null) return;

            Transform[] children = _root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name != "Opponent Shoulder Visual") continue;
                if (child.localPosition.x < 0f) _leftShoulderVisual = child;
                else if (child.localPosition.x > 0f) _rightShoulderVisual = child;
            }
        }
    }
}
