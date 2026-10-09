using System.Runtime.InteropServices;

namespace BoxerP0
{
    // Browser media is presentation only. The bootstrap owns all combat state.
    public static class RingPresentation
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void BoxerRingCommand(int command, int token);
#endif
        public static void Command(int command, int token = 0)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BoxerRingCommand(command, token);
#endif
        }
    }
}
