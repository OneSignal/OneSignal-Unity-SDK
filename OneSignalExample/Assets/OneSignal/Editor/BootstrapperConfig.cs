using System.Collections.Generic;

    static class BootstrapperConfig
    {
        public static readonly string OneSignalScope = "com.onesignal.unity";
        public static readonly string BootstrapperPackageName = $"{OneSignalScope}.bootstrap";

        public static readonly string OneSignalCoreName = $"{OneSignalScope}.core";
        public static readonly string OneSignalIOSName = $"{OneSignalScope}.ios";
        public static readonly string OneSignalAndroidName = $"{OneSignalScope}.android";

        public static readonly string GoogleScopeRegistryUrl = "https://unityregistry-pa.googleapis.com";
        public static readonly string NpmjsScopeRegistryUrl = "https://registry.npmjs.org/";

        public const string GitHubRepositoryURL = @"ssh://git@github.com:OneSignal/OneSignal-Unity-SDK.git";
        public const string BootstrapperFolderPath = @"Assets/OneSignalBootstrap";

        public static readonly string[] OutdatedSDKDirectories =
        {
            @"Assets/OneSignal",
            @"Assets/Plugins/Android/OneSignalConfig.plugin",
        };

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
                    OneSignalScope
                });
    }
