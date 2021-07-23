using UnityEngine;

static class OneSignalEditorInit
{
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        OneSignal.RegisterPlatform(new OneSignalEditor());
    }
}