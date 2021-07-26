using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// Checks for EDM4U assemblies and installs the package from its github releases
/// </summary>
public sealed class InstallEdm4uStep : OneSignalSetupStep
{
    public override string Summary
        => $"Install EDM4U {_edm4UVersion}";

    public override string Details
        => $"Downloads and imports version {_edm4UVersion} from Google's repo. This library resolves dependencies " +
           $"among included libraries on Android.";

    public override bool IsRequired 
        => true;

    protected override bool _getIsStepCompleted()
    {
        var precompiledAssemblies = CompilationPipeline.GetPrecompiledAssemblyNames();
        foreach (var assemblyName in precompiledAssemblies)
        {
            if (assemblyName.StartsWith("Google.VersionHandler"))
                return true;
        }

        return false;
    }

    protected override void _runStep()
    {
        var request = EditorWebRequest.Get(_edm4UPackageDownloadUrl);
        request.AddEditorProgressDialog("Downloading Google External Dependency Manager");
        request.Send(unityRequest =>
        {
            if (unityRequest.error != null)
            {
                EditorUtility.DisplayDialog("Package Download failed.", unityRequest.error, "Ok");
                return;
            }

            //Asset folder name remove
            var projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            var tmpPackageFile = projectPath + FileUtil.GetUniqueTempPathInProject() + ".unityPackage";

            File.WriteAllBytes(tmpPackageFile, unityRequest.downloadHandler.data);

            AssetDatabase.ImportPackage(tmpPackageFile, false);
            _shouldCheckForCompletion = true;
        });
    }

    private const string _edm4UVersion = "1.2.165";

    static readonly string _edm4UPackageDownloadUrl = $"https://github.com/googlesamples/unity-jar-resolver/blob/v{_edm4UVersion}/external-dependency-manager-{_edm4UVersion}.unitypackage?raw=true";
}