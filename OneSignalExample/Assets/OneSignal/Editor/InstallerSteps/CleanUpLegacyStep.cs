using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class CleanUpLegacyStep : OneSignalInstallerStep
{
    public override string Summary
        => "Remove legacy files";

    public override string Details
        => "";

    public override string DocumentationLink
        => "";

    protected override bool _getIsStepCompleted()
    {
        return false;
    }

    protected override void _install()
    {
        
    }
}