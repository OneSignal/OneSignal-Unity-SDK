using System.Collections.Generic;

namespace Com.OneSignal.Editor
{
    static class ScopeRegistriesConfig
    {
        public static readonly string OneSignalScope = "com.onesignal-test";
        public static readonly string EDM4UName = "com.google.external-dependency-manager";
        public static readonly string EDM4UVersion = "1.2.153";

        public static readonly string GoogleScopeRegistryUrl = "https://unityregistry-pa.googleapis.com";

        public static ScopeRegistry GoogleScopeRegistry =>
            new ScopeRegistry("Game Package Registry by Google",
                GoogleScopeRegistryUrl,
                new HashSet<string>
                {
                    "com.google"
                });
    }
}
