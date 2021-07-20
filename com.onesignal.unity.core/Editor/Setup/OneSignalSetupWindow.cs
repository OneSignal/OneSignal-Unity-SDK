using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pop up window which displays any additional required or optional setup steps by the SDK
/// </summary>
public sealed class OneSignalSetupWindow : EditorWindow
{
    [MenuItem("Window/" + OneSignalSettings.ProductName)]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(OneSignalSetupWindow), false, _title);
        window.Show();
    }
    
    public static void CloseWindow()
    {
        var window = GetWindow(typeof(OneSignalSetupWindow), false, _title);
        window.Close();
    }

    private const string _title = "OneSignal SDK Setup";
    private const string _description = "Additional steps required to get the OneSignal Unity SDK up and running";
    
    private IReadOnlyList<OneSignalSetupStep> _setupSteps;
    private readonly Queue<OneSignalSetupStep> _stepsToRun = new Queue<OneSignalSetupStep>();
    
    private bool _guiSetupComplete = false;
    private GUIStyle _summaryStyle;
    private GUIStyle _detailsStyle;
    private Texture _checkTexture;
    private Texture _boxTexture;
    
    private void OnEnable()
    {
        var stepTypes = _findAllAssignableTypes<OneSignalSetupStep>("OneSignal");
        var steps = new List<OneSignalSetupStep>();

        foreach (var stepType in stepTypes)
        {
            if (Activator.CreateInstance(stepType) is OneSignalSetupStep step)
                steps.Add(step);
            else
                Debug.LogWarning($"could not create setup step from type {stepType.Name}");
        }

        _setupSteps = steps;
    }

    private void OnGUI()
    {
        _setupGUI();

        GUILayout.Label(_description);
        EditorGUILayout.Separator();

        if (_setupSteps == null) 
            return;

        var willDisableControls = _stepsToRun.Count > 0 
            || EditorApplication.isUpdating 
            || EditorApplication.isCompiling;

        EditorGUI.BeginDisabledGroup(willDisableControls);
        if (GUILayout.Button("Run All Steps"))
        {
            foreach (var step in _setupSteps)
                _stepsToRun.Enqueue(step);
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Separator();
        
        foreach (var step in _setupSteps)
        {
            EditorGUILayout.BeginHorizontal();

            var sumContent = new GUIContent(step.Summary);
            var sumRect = GUILayoutUtility.GetRect(sumContent, _summaryStyle);

            var checkRect = new Rect(sumRect.x, sumRect.y, sumRect.height, sumRect.height);
            GUI.DrawTexture(checkRect, step.IsStepCompleted ? _checkTexture: _boxTexture);

            sumRect.x += sumRect.height + EditorStyles.label.padding.left;
            GUI.Label(sumRect, sumContent);
            
            EditorGUI.BeginDisabledGroup(step.IsStepCompleted || willDisableControls);
            if (GUILayout.Button("Run") && !_stepsToRun.Contains(step))
                _stepsToRun.Enqueue(step);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();

            GUILayout.Label(step.Details, _detailsStyle);
            EditorGUILayout.Separator();
        }
    }

    private void OnInspectorUpdate()
    {
        var runnerCount = _stepsToRun.Count + 1.0f;
        while (_stepsToRun.Count > 0)
        {
            var step = _stepsToRun.Dequeue();
            
            EditorUtility.DisplayProgressBar(
                "OneSignal Setup", 
                $"Running step \"{step.Summary}\"", 
                _stepsToRun.Count / runnerCount
            );
            
            step.RunStep();
        }
        EditorUtility.ClearProgressBar();
    }

    private void _setupGUI()
    {
        if (_guiSetupComplete)
            return;

        _summaryStyle = EditorStyles.boldLabel;
        
        _detailsStyle = new GUIStyle(GUI.skin.textField)
        {
            wordWrap = true
        };

        var checkContent = EditorGUIUtility.IconContent("TestPassed");
        _checkTexture = checkContent.image;

        var boxContent = EditorGUIUtility.IconContent("Warning");
        _boxTexture = boxContent.image;
        
        _guiSetupComplete = true;
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