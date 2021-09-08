using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Checks for whether the OneSignal Unity Core package has been added to the project and does so if not
/// </summary>
public sealed class ImportPackagesStep : OneSignalSetupStep
{
    public override string Summary
        => "Import OneSignal packages";

    public override string Details
        => "Add the OneSignal registry and core, ios, and android packages to the project manifest so they will be " +
           "downloaded and imported";

    public override bool IsRequired 
        => true;

    #if ONE_SIGNAL_INSTALLED
    protected override bool _getIsStepCompleted() => true;
    #else
    protected override bool _getIsStepCompleted() => false;
    #endif
    
    protected override void _runStep()
    {
        var manifest = new Manifest();
        manifest.Fetch();
        
        manifest.AddScopeRegistry(_scopeRegistry);
        
        var scopeRegistry = manifest.GetScopeRegistry(_registryUrl);
        scopeRegistry.AddScope(_packagesScope);

#if UNITY_2017_3_OR_NEWER
        manifest.ApplyChanges();
        
        var addRequest = UnityEditor.PackageManager.Client.Add(_coreVersion);
        while (!addRequest.IsCompleted) { }
        
        addRequest = UnityEditor.PackageManager.Client.Add(_androidVersion);
        while (!addRequest.IsCompleted) { }
        
        addRequest = UnityEditor.PackageManager.Client.Add(_iosVersion);
        while (!addRequest.IsCompleted) { }
#else
        manifest.AddDependency(_corePackageName, _coreVersion);
        manifest.AddDependency(_androidPackageName, _androidVersion);
        manifest.AddDependency(_iosPackageName, _iosVersion);

        manifest.ApplyChanges();
        AssetDatabase.Refresh();
#endif
        OneSignalSetupWindow.CloseWindow();
        SessionState.SetBool(_shouldShowWindowKey, true);
    }

#if ONE_SIGNAL_INSTALLED
    [InitializeOnLoadMethod]
    public static void _showCoreInstallerWindow() 
    {
        if (!SessionState.GetBool(_shouldShowWindowKey, false))
            return;
        
        SessionState.EraseBool(_shouldShowWindowKey);
        EditorApplication.delayCall += OneSignalSetupWindow.ShowWindow;
    }
#endif

    private const string _shouldShowWindowKey = "onesignal.importpackage.shouldshow";
    private const string _packagesScope = "com.onesignal";
    
    private static readonly string _corePackageName = $"{_packagesScope}.unity.core";
    private static readonly string _androidPackageName = $"{_packagesScope}.unity.android";
    private static readonly string _iosPackageName = $"{_packagesScope}.unity.ios";
    
#if IS_ONESIGNAL_EXAMPLE_APP
    private static readonly string _coreVersion = $"file:../../{_corePackageName}";
    private static readonly string _androidVersion = $"file:../../{_androidPackageName}";
    private static readonly string _iosVersion = $"file:../../{_iosPackageName}";
    
    private const string _registryName = "npmjs";
    private const string _registryUrl = "https://registry.npmjs.org";
#else
    private static string _coreVersion => $"{_corePackageName}@{_version}";
    private static string _androidVersion => $"{_androidPackageName}@{_version}";
    private static string _iosVersion => $"{_iosPackageName}@{_version}";

    private const string _registryName = "npmjs";
    private const string _registryUrl = "https://registry.npmjs.org";

    private static readonly string _versionPath = Path.Combine("Assets", "OneSignal", "VERSION");
    private static string _version => File.ReadAllText(_versionPath);
#endif

    private static readonly HashSet<string> _scopes = new HashSet<string> { _packagesScope };
    private static readonly ScopeRegistry _scopeRegistry = new ScopeRegistry(_registryName, _registryUrl, _scopes);
}