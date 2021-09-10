using System.IO;
using System.Linq;
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
#if UNITY_2021_1_OR_NEWER
            "among included libraries on Android.\n\n<b>NOTE</b>: In Unity 2021+ the " +
            $"Google.IOSResolver_v{_edm4UVersion}.dll will be renamed to Google.IOSResolver.dll in order to resolve a bug.";
#else
           "among included libraries on Android.";
#endif

    public override bool IsRequired
        => true;

    protected override bool _getIsStepCompleted()
        => CompilationPipeline.GetPrecompiledAssemblyNames()
           .Any(assemblyName => assemblyName.StartsWith("Google.VersionHandler"));

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
            
#if UNITY_2021_1_OR_NEWER
            SessionState.SetBool(_shouldFix2021Bug, true);
#endif
            
            _shouldCheckForCompletion = true;
        });
    }
    
    [InitializeOnLoadMethod]
    public static void _fixUnity2021Bug() 
    {
        if (!SessionState.GetBool(_shouldFix2021Bug, false))
            return;

        SessionState.EraseBool(_shouldFix2021Bug);
        
        EditorApplication.delayCall += () => {
            File.Move(_iosDLLSourcePath, _iosDLLDestPath);
            File.Move(_iosDLLSourcePath + ".meta", _iosDLLDestPath + ".meta");
        };
    }

    private const string _edm4UVersion = "1.2.165";
    private const string _shouldFix2021Bug = "onesignal.installedm4u.shouldfix2021bug";
    private const string _iosDLLDestPath   = "Assets/ExternalDependencyManager/Editor/Google.IOSResolver.dll";
    private static readonly string _iosDLLSourcePath = $"Assets/ExternalDependencyManager/Editor/Google.IOSResolver_v{_edm4UVersion}.dll";

    static readonly string _edm4UPackageDownloadUrl
        = $"https://github.com/googlesamples/unity-jar-resolver/blob/v{_edm4UVersion}/external-dependency-manager-{_edm4UVersion}.unitypackage?raw=true";
}