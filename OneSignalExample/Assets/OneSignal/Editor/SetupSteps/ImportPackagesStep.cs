using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Checks for whether the OneSignal Unity Core package has been added to the project and does so if not
/// </summary>
public sealed class ImportPackagesStep : OneSignalSetupStep
{
    public override string Summary
        => "Import OneSignal packages";

    public override string Details
        => "Add the OneSignal registry and core, ios, and android packages to the project manifest so they will be" +
           "downloaded and imported";

    public override string DocumentationLink
        => "";

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
        
        manifest.AddDependency(_corePackageName, _coreVersion);
        manifest.AddDependency(_androidPackageName, _androidVersion);
        manifest.AddDependency(_iosPackageName, _iosVersion);

        manifest.ApplyChanges();
        AssetDatabase.Refresh();
    }
    
    private const string _packagesScope = "com.onesignal.unity";
    
    private static readonly string _corePackageName = $"{_packagesScope}.core";
    private static readonly string _androidPackageName = $"{_packagesScope}.android";
    private static readonly string _iosPackageName = $"{_packagesScope}.ios";
    
#if IS_ONESIGNAL_EXAMPLE_APP
    private static readonly string _coreVersion = $"file:../../{_corePackageName}";
    private static readonly string _androidVersion = $"file:../../{_androidPackageName}";
    private static readonly string _iosVersion = $"file:../../{_iosPackageName}";
    
    private const string _registryName = "npmjs";
    private const string _registryUrl = "https://registry.npmjs.org";
    
    // private const string _githubRegistryUrl = "https://npm.pkg.github.com/@OneSignal";
#else // todo
    private static readonly string _coreVersion = "";
    private static readonly string _androidVersion = "";
    private static readonly string _iosVersion = "";

    private const string _registryName = "npmjs";
    private const string _registryUrl = "https://registry.npmjs.org";
#endif

    private static readonly HashSet<string> _scopes = new HashSet<string> { _packagesScope };
    private static readonly ScopeRegistry _scopeRegistry = new ScopeRegistry(_registryName, _registryUrl, _scopes);
}