using System.Linq;
using UnityEditor;
using UnityEngine;

    [InitializeOnLoad]
    static class Bootstrapper
    {
        static Bootstrapper()
        {
            if (!AssetDatabase.FindAssets("onesignal-bootstrap-lock", new[] {"Assets"}).Any())
                InstallLatestOneSignalRelease();
            else
                Debug.LogWarning("'lock' file found. Bootstrap execution locked.");
        }

        static bool IsOneSignalCoreInstalled
        {
            get
            {
#if ONE_SIGNAL_INSTALLED
                return true;
#else
                return false;
#endif
            }
        }

        internal static void InstallLatestOneSignalRelease()
        {
            if (IsOneSignalCoreInstalled)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorUtility.DisplayDialog("Successes",
                        "OneSignal installation completed. Thank you!",
                        "Ok");
                    UninstallBootstrapper();
                };
                return;
            }

            if (FindRemainingDirectoriesOfOutdatedSDK(out var directories))
            {
                if (EditorUtility.DisplayDialog("OneSignal",
                    "The project contains an outdated version of OneSignal SDK! It has to be removed in order to continue the installation.",
                    "Remove and continue",
                    "Cancel installation"))
                {
                    CleanUpUtility.RemoveDirectories(directories);
                }
                else
                {
                    EditorApplication.delayCall += UninstallBootstrapper;
                    return;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("OneSignal",
                    "Installation started. Thank you!",
                    "Ok");
            }

            GitHubUtility.GetLatestRelease(BootstrapperConfig.GitHubRepositoryURL, Bootstrap);
        }

        static void UninstallBootstrapper()
        {
            UnityEditor.PackageManager.Client.Remove(BootstrapperConfig.BootstrapperPackageName);
            CleanUpUtility.RemoveDirectories(BootstrapperConfig.BootstrapperFolderPath);
        }

        static bool FindRemainingDirectoriesOfOutdatedSDK(out string[] directories)
        {
            directories = BootstrapperConfig.OutdatedSDKDirectories
                .Where(AssetDatabase.IsValidFolder)
                .ToArray();
            return directories.Any();
        }

        static void Bootstrap(string latestRelease)
        {
            var manifest = new Manifest();
            manifest.Fetch();

            var manifestUpdated = false;

            if (!manifest.IsRegistryPresent(BootstrapperConfig.NpmjsScopeRegistryUrl))
            {
                manifest.AddScopeRegistry(BootstrapperConfig.NpmjsScopeRegistry);
                manifestUpdated = true;
            }
            else
            {
                var npmjsScopeRegistry = manifest.GetScopeRegistry(BootstrapperConfig.NpmjsScopeRegistryUrl);
                if (!npmjsScopeRegistry.HasScope(BootstrapperConfig.OneSignalScope))
                {
                    npmjsScopeRegistry.AddScope(BootstrapperConfig.OneSignalScope);
                    manifestUpdated = true;
                }
            }

            // UnityEditor.PackageManager.Client.Add method doesn't work in Unity versions older then 2019.
            // Thus, we need to manually add dependencies.
            // Probably we need to make something similar to OneSignalUpdateRequest to get the latest package version.

            if (!manifest.IsDependencyPresent(BootstrapperConfig.OneSignalAndroidName))
            {
                manifest.AddDependency(BootstrapperConfig.OneSignalAndroidName, latestRelease);
                manifestUpdated = true;
            }

            if (!manifest.IsDependencyPresent(BootstrapperConfig.OneSignalIOSName))
            {
                manifest.AddDependency(BootstrapperConfig.OneSignalIOSName, latestRelease);
                manifestUpdated = true;
            }

            if (manifestUpdated)
            {
                manifest.ApplyChanges();
                AssetDatabase.Refresh();
            }
        }
    }
