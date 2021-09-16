using System;
using UnityEngine;

/// <summary>
/// todo
/// </summary>
public sealed class GUILayoutScope : IDisposable {
    /// <summary>
    /// todo
    /// </summary>
    public static GUILayoutScope BeginVertical {
        get {
            GUILayout.BeginVertical();
            return new GUILayoutScope(GUILayout.EndVertical);
        }
    }

    /// <summary>
    /// todo
    /// </summary>
    public static GUILayoutScope BeginHorizontal {
        get {
            GUILayout.BeginHorizontal();
            return new GUILayoutScope(GUILayout.EndHorizontal);
        }
    }
        
    /// <summary>
    /// todo
    /// </summary>
    public void Dispose() => _endMethod?.Invoke();
        
    private readonly Action _endMethod;
    internal GUILayoutScope(Action endMethod) => _endMethod = endMethod;
}