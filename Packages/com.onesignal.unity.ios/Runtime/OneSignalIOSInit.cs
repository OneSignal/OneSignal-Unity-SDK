using UnityEngine;

namespace OneSignalPush.IOS
{
    static class OneSignalIOSInit
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            OneSignal.RegisterPlatform(new OneSignalIOS());
        }
    }
}
