using System.Collections.Generic;

namespace OneSignalPush.Editor
{
    static class ScopeRegistriesConfig
    {
        public static readonly string EDM4UName = "com.google.external-dependency-manager";
        public static readonly string EDM4UVersion = "1.2.153";

        public static readonly string StansAssetsFoundationName = "com.stansassets.foundation";
        public static readonly string StansAssetsFoundationVersion = "1.0.3";

        public static readonly string GoogleScopeRegistryUrl = "https://unityregistry-pa.googleapis.com";
        public static readonly string NpmjsScopeRegistryUrl = "https://registry.npmjs.org/";

        public static ScopeRegistry GoogleScopeRegistry =>
            new ScopeRegistry("Game Package Registry by Google",
                GoogleScopeRegistryUrl,
                new HashSet<string>
                {
                    "com.google"
                });

        public static ScopeRegistry NpmjsScopeRegistry =>
            new ScopeRegistry("npmjs",
                NpmjsScopeRegistryUrl,
                new HashSet<string>
                {
                    "com.stansassets"
                });
    }
}
