using UnityEngine;

namespace OneSignalPush.Android
{
    static class OneSignalIOSInit
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            OneSignal.RegisterPlatform(new OneSignalAndroid());
        }
    }
}
