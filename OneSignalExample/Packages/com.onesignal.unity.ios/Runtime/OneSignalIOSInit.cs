using UnityEngine;

namespace Com.OneSignal.IOS
{
    static class OneSignalIOSInit
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            if(!Application.isEditor)
                OneSignalPush.RegisterPlatform(new OneSignalIOS());
        }
    }
}
