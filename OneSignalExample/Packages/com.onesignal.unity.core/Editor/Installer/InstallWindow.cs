using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class InstallWindow : EditorWindow
{
    [MenuItem("OneSignal/Install")]
    public static void Show()
    {
        var window = GetWindow(typeof(InstallWindow), true, _title);
        window.Show();
    }

    private const string _title = "OneSignal Component Installer";
    private const string _header = "";
    private IReadOnlyList<InstallStep> _installSteps;
    
    private void OnEnable()
    {
    }

    private void OnGUI()
    {
        GUILayout.Label(_header, EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        if (_installSteps == null) 
            return;
        
        foreach (var step in _installSteps)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(step.Summary);
            
            var buttonText = step.IsStepCompleted ? "Reinstall" : "Install";
            if (GUILayout.Button(buttonText))
                step.Install();
            
            EditorGUILayout.EndHorizontal();

            GUILayout.Label(step.Details);
            EditorGUILayout.Separator();
        }
    }
}