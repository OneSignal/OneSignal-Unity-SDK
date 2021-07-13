using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pop up window which displays any additional required or optional setup steps by the SDK
/// </summary>
public class OneSignalInstallerWindow : EditorWindow
{
    [MenuItem("OneSignal/Dependency Installer")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(OneSignalInstallerWindow), true, _title);
        window.Show();
    }

    private const string _title = "OneSignal Component Installer";
    private const string _description = "Additional steps required to get the OneSignal Unity SDK up and running";
    
    private IReadOnlyList<OneSignalInstallerStep> _installSteps;
    private readonly Queue<Action> _actionsToPerform = new Queue<Action>();
    
    private bool _guiSetupComplete = false;
    private GUIStyle _summaryStyle;
    private GUIStyle _detailsStyle;
    private Texture _checkTexture;
    private Texture _boxTexture;
    
    private void OnEnable()
    {
        var stepTypes = _findAllAssignableTypes<OneSignalInstallerStep>("OneSignal");
        var steps = new List<OneSignalInstallerStep>();

        foreach (var stepType in stepTypes)
        {
            if (Activator.CreateInstance(stepType) is OneSignalInstallerStep step)
                steps.Add(step);
            else
                Debug.LogWarning($"could not create install step from type {stepType.Name}");
        }

        _installSteps = steps;
    }

    private void OnGUI()
    {
        _setupGUI();

        GUILayout.Label(_description);
        EditorGUILayout.Separator();

        if (_installSteps == null) 
            return;

        if (GUILayout.Button("Run All Steps"))
        {
            foreach (var step in _installSteps)
                _actionsToPerform.Enqueue(step.Install);
        }
        
        EditorGUILayout.Separator();
        
        foreach (var step in _installSteps)
        {
            EditorGUILayout.BeginHorizontal();

            var sumContent = new GUIContent(step.Summary);
            var sumRect = GUILayoutUtility.GetRect(sumContent, _summaryStyle);

            var checkRect = new Rect(sumRect.x, sumRect.y, sumRect.height, sumRect.height);
            GUI.DrawTexture(checkRect, step.IsStepCompleted ? _checkTexture: _boxTexture);

            sumRect.x += sumRect.height + EditorStyles.label.padding.left;
            GUI.Label(sumRect, sumContent);

            EditorGUI.BeginDisabledGroup(step.IsStepCompleted);
            _handleButtonResult(GUILayout.Button("Install"), step.Install);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();

            GUILayout.Label(step.Details, _detailsStyle);
            EditorGUILayout.Separator();
        }
    }

    private void OnInspectorUpdate()
    {
        while (_actionsToPerform.Count > 0)
            _actionsToPerform.Dequeue().Invoke();
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

    private void _handleButtonResult(bool buttonResult, Action buttonAction)
    {
        if (buttonResult && !_actionsToPerform.Contains(buttonAction))
            _actionsToPerform.Enqueue(buttonAction);
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