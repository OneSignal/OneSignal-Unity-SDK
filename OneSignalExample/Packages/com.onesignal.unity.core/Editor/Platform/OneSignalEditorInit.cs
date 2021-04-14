using UnityEngine;

namespace Com.OneSignal.Editor
{
    static class OneSignalEditorInit
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            OneSignalPush.RegisterPlatform(new OneSignalEditor());

        }
    }
}
