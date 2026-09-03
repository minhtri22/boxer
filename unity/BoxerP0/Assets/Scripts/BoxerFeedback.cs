using UnityEngine;

namespace BoxerP0
{
    public sealed class BoxerFeedback : MonoBehaviour
    {
        private static BoxerFeedback _instance;
        private AudioSource _audio;
        private AudioClip _hit;
        private AudioClip _block;

        public bool AudioEnabled { get; private set; } = true;
        public bool HapticsEnabled { get; private set; } = true;

        private void Awake()
        {
            _instance = this;
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _hit = CreateImpactClip("P0Hit", 100f, 0.08f, 0.55f);
            _block = CreateImpactClip("P0Block", 180f, 0.055f, 0.28f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) AudioEnabled = !AudioEnabled;
            if (Input.GetKeyDown(KeyCode.H)) HapticsEnabled = !HapticsEnabled;
        }

        public static void Emit(CombatOutcome outcome)
        {
            if (_instance == null || outcome == CombatOutcome.None || outcome == CombatOutcome.Miss)
            {
                return;
            }

            if (_instance.AudioEnabled)
            {
                _instance._audio.PlayOneShot(outcome == CombatOutcome.Block ? _instance._block : _instance._hit);
            }

#if UNITY_IOS || UNITY_ANDROID
            if (_instance.HapticsEnabled && outcome == CombatOutcome.Hit)
            {
                Handheld.Vibrate();
            }
#endif
        }

        private static AudioClip CreateImpactClip(string name, float frequency, float seconds, float volume)
        {
            int sampleRate = 22050;
            int sampleCount = Mathf.CeilToInt(seconds * sampleRate);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - i / (float)sampleCount;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

