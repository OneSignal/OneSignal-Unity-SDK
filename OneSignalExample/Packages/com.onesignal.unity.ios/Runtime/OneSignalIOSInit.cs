using UnityEngine;

namespace Com.OneSignal.IOS
{
    static class OneSignalIOSInit
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            if(!Application.isEditor)
                OneSignal.RegisterPlatform(new OneSignalIOS());
        }
    }
}
