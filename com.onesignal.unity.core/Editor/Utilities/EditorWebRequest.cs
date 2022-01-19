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
using UnityEditor;
using UnityEngine.Networking;

namespace OneSignalSDK {
/// <summary>
/// The EditorWebRequest is a wrapper around the `UnityWebRequest` that can work in Edit mode.
/// </summary>
class EditorWebRequest {
    Action<UnityWebRequest> m_OnComplete = null;

    bool m_ShowProgress = false;
    string m_ProgressDialogTitle = string.Empty;

    /// <summary>
    /// Create an EditorWebRequest for HTTP GET.
    ///  Use the method to create a `EditorWebRequest`.
    ///  Set the target `URL` to the `uri` with a `string` or `Uri` argument.
    /// No custom flags or headers are set.
    /// </summary>
    /// <param name="url">The URI of the resource to retrieve via HTTP GET.</param>
    public static EditorWebRequest Get(string url) {
        var unityRequest = UnityWebRequest.Get(url);

        return new EditorWebRequest(unityRequest);
    }

    /// <summary>
    /// Create new `EditorWebRequest` instance based on `UnityWebRequest`.
    /// </summary>
    /// <param name="request"></param>
    public EditorWebRequest(UnityWebRequest request) {
        UnityRequest = request;
    }

    /// <summary>
    /// Request will display editor progress dialog with the given title.
    /// </summary>
    /// <param name="title">Editor progress dialog title.</param>
    public void AddEditorProgressDialog(string title) {
        m_ShowProgress        = true;
        m_ProgressDialogTitle = title;
    }

    /// <summary>
    /// Begin communicating with the remote server.
    /// </summary>
    /// <param name="callback">Communication callback.</param>
    public void Send(Action<UnityWebRequest> callback) {
        m_OnComplete             =  callback;
        EditorApplication.update += OnEditorUpdate;
        UnityRequest.SendWebRequest();
    }

    /// <summary>
    /// The `UnityWebRequest` instance that is stored inside `EditorWebRequest`.
    /// </summary>
    public UnityWebRequest UnityRequest { get; } = null;

    /// <summary>
    /// The shortcut for `UnityRequest.downloadHandler.text`.
    /// </summary>
    public string DataAsText => UnityRequest.downloadHandler.text;

    void OnEditorUpdate() {
        if (m_ShowProgress) {
            var progress = $"Download Progress: {Convert.ToInt32(UnityRequest.downloadProgress * 100f)}%";
            EditorUtility.DisplayProgressBar(m_ProgressDialogTitle, progress, UnityRequest.downloadProgress);
        }

        if (UnityRequest.isDone) {
            if (m_ShowProgress) {
                EditorUtility.ClearProgressBar();
            }

            EditorApplication.update -= OnEditorUpdate;
            m_OnComplete.Invoke(UnityRequest);
            }
        }
    }
}