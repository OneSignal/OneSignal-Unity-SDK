using System.Linq;
using UnityEditor.Compilation;

/// <summary>
/// Checks for EDM4U assemblies and installs the package from its github releases
/// </summary>
public sealed class InstallEdm4uStep : OneSignalSetupStep {
    public override string Summary
        => $"Install EDM4U {_edm4UVersion}";

    public override string Details
        => $"Downloads and imports version {_edm4UVersion} from Google's repo. This library resolves dependencies " +
           $"among included libraries on Android.";

    public override bool IsRequired
        => true;

    protected override bool _getIsStepCompleted()
        => CompilationPipeline.GetPrecompiledAssemblyNames()
           .Any(assemblyName => assemblyName.StartsWith("Google.VersionHandler"));

    protected override void _runStep() {
        const string msg = "Downloading Google External Dependency Manager";
        UnityPackageInstaller.DownloadAndInstall(_edm4UPackageDownloadUrl, msg, result => {
            if (result)
                _shouldCheckForCompletion = true;
        });
    }

    private const string _edm4UVersion = "1.2.167";
    private static readonly string _edm4UPackageDownloadUrl
        = $"https://github.com/googlesamples/unity-jar-resolver/blob/v{_edm4UVersion}/external-dependency-manager-{_edm4UVersion}.unitypackage?raw=true";
}