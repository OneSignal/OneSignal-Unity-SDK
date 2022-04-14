/*
 * Modified MIT License
 *
 * Copyright 2022 OneSignal
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * 1. The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * 2. All copies of substantial portions of the Software may only be used in connection
 * with services provided by OneSignal.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OneSignalSDK {
    /// <summary>
    /// Pop up window which displays any additional required or optional setup steps by the SDK
    /// </summary>
    public sealed class OneSignalSetupWindow : EditorWindow {
        [MenuItem("Window/OneSignal SDK Setup")] public static void ShowWindow() {
            var window = GetWindow(typeof(OneSignalSetupWindow), false, _title);
            window.minSize = _minSize;
            window.Show();
        }

        public static void CloseWindow() {
            var window = GetWindow(typeof(OneSignalSetupWindow), false, _title);
            window.Close();
        }

        private static readonly Vector2 _minSize = new Vector2(300, 400);

        private const string _title = "OneSignal SDK Setup";
        private const string _description = "Additional steps required to get the OneSignal Unity SDK up and running";

        private IReadOnlyList<OneSignalSetupStep> _setupSteps;
        private readonly Queue<OneSignalSetupStep> _stepsToRun = new Queue<OneSignalSetupStep>();

        private bool _guiSetupComplete = false;
        private GUIStyle _summaryStyle;
        private GUIStyle _runStyle;
        private GUIStyle _detailsStyle;
        private GUIStyle _requiredStyle;
        private GUIStyle _optionalStyle;
        private Texture _checkTexture;
        private Texture _boxTexture;

        private Vector2 _scrollPosition;

        private void OnEnable() {
            var stepTypes = ReflectionHelpers.FindAllAssignableTypes<OneSignalSetupStep>("OneSignal");
            var steps     = new List<OneSignalSetupStep>();

            foreach (var stepType in stepTypes) {
                if (Activator.CreateInstance(stepType) is OneSignalSetupStep step)
                    steps.Add(step);
                else
                    Debug.LogWarning($"could not create setup step from type {stepType.Name}");
            }

            _setupSteps = steps;
        }

        private void OnGUI() {
            _setupGUI();

            GUILayout.Label(_description);
            EditorGUILayout.Separator();

            if (_setupSteps == null)
                return;

            var willDisableControls = _stepsToRun.Count > 0
                || EditorApplication.isUpdating
                || EditorApplication.isCompiling;

            EditorGUI.BeginDisabledGroup(willDisableControls);

            if (GUILayout.Button("Run All Steps")) {
                foreach (var step in _setupSteps)
                    _stepsToRun.Enqueue(step);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.BeginHorizontal();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var step in _setupSteps) {
                EditorGUILayout.BeginHorizontal();

                var sumContent = new GUIContent(step.Summary);
                var sumRect    = GUILayoutUtility.GetRect(sumContent, _summaryStyle);

                var checkRect = new Rect(sumRect.x, sumRect.y, sumRect.height, sumRect.height);
                GUI.DrawTexture(checkRect, step.IsStepCompleted ? _checkTexture : _boxTexture);

                sumRect.x += sumRect.height + EditorStyles.label.padding.left;
                GUI.Label(sumRect, sumContent);

                EditorGUI.BeginDisabledGroup(step.IsStepCompleted || willDisableControls);
                if (GUILayout.Button("Run", _runStyle) && !_stepsToRun.Contains(step))
                    _stepsToRun.Enqueue(step);

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();

                GUILayout.Label(step.Details, _detailsStyle);

                if (step.IsRequired)
                    GUILayout.Label("Required", _requiredStyle);
                else
                    GUILayout.Label("Optional", _optionalStyle);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }

        private void OnInspectorUpdate() {
            var runnerCount = _stepsToRun.Count + 1.0f;

            while (_stepsToRun.Count > 0) {
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

        private void _setupGUI() {
            if (_guiSetupComplete)
                return;

            _summaryStyle = EditorStyles.boldLabel;

            _detailsStyle          = new GUIStyle(GUI.skin.textField);
            _detailsStyle.wordWrap = true;

            _runStyle            = new GUIStyle(GUI.skin.button);
            _runStyle.fixedWidth = _minSize.x * .3f;

            _requiredStyle                  = new GUIStyle(EditorStyles.miniBoldLabel);
            _requiredStyle.normal.textColor = Color.red;

            _optionalStyle                  = new GUIStyle(EditorStyles.miniBoldLabel);
            _optionalStyle.normal.textColor = Color.yellow;
            _optionalStyle.fontStyle        = FontStyle.Italic;

            var checkContent = EditorGUIUtility.IconContent("TestPassed");
            _checkTexture = checkContent.image;

            var boxContent = EditorGUIUtility.IconContent("Warning");
            _boxTexture = boxContent.image;

            _guiSetupComplete = true;
        }
    }
}