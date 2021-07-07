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
        var stepTypes = _findAllAssignableTypes<InstallStep>("OneSignal");
        var steps = new List<InstallStep>();

        foreach (var stepType in stepTypes)
        {
            if (Activator.CreateInstance(stepType) is InstallStep step)
                steps.Add(step);
            else
                Debug.LogWarning($"could not create install step from type {stepType.Name}");
        }

        _installSteps = steps;
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
    
    private static IEnumerable<Type> _findAllAssignableTypes<T>(string assemblyFilter)
    {
        var assignableType = typeof(T);
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var filteredAssemblies = assemblies.Where(assembly 
            => assembly.FullName.Contains(assemblyFilter));
        
        var allTypes = filteredAssemblies.SelectMany(assembly => assembly.GetTypes());
        var assignableTypes = allTypes.Where(type 
            => type != assignableType && assignableType.IsAssignableFrom(type));

        return assignableTypes;
    }
}