using UnityEngine;


static class OneSignalIOSInit
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        if (!Application.isEditor)
            OneSignal.RegisterPlatform(new OneSignalIOS());
    }
}