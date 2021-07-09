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
public class UpdateToPackagesStep : OneSignalInstallerStep
{
    public override string Summary
        => "Update to UPM packages";

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
    }
}