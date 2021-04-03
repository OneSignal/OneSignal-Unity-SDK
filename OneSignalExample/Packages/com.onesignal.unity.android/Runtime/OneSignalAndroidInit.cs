using UnityEngine;

namespace Com.OneSignal.Android
{
    static class OneSignalAndroidInit
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            if(!Application.isEditor)
                OneSignal.RegisterPlatform(new OneSignalAndroid());
        }
    }
}
