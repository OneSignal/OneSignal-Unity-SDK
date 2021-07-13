using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class ImportPackagesStep : OneSignalInstallerStep
{
    public override string Summary
        => "Import OneSignal packages";

    public override string Details
        => "";

    public override string DocumentationLink
        => "";

    #if ONE_SIGNAL_INSTALLED
    protected override bool _getIsStepCompleted() => true;
    #else
    protected override bool _getIsStepCompleted() => false;
    #endif
    
    protected override void _install()
    {
#if IS_ONESIGNAL_EXAMPLE_APP
        var manifest = new Manifest();
        manifest.Fetch();
        manifest.AddDependency(
            "com.onesignal.unity.core",
            "file:../../com.onesignal.unity.core"
        );
        
        manifest.AddDependency(
            "com.onesignal.unity.android",
            "file:../../com.onesignal.unity.android"
        );
        
        manifest.AddDependency(
            "com.onesignal.unity.ios",
            "file:../../com.onesignal.unity.ios"
        );
        
        manifest.ApplyChanges();
        AssetDatabase.Refresh();
#else 
        
#endif
    }
}