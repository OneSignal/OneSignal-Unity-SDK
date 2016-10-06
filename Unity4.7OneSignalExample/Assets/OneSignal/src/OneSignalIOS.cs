/**
 * Modified MIT License
 * 
 * Copyright 2016 OneSignal
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

#if UNITY_IPHONE
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OneSignalPush.MiniJSON;

public class OneSignalIOS : OneSignalPlatform {

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _init(string listenerName, string appId, bool autoPrompt, bool inAppLaunchURLs, int displayOption, int logLevel, int visualLogLevel);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _registerForPushNotifications();
	
	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _sendTag(string tagName, string tagValue);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _sendTags(string tags);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _getTags();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _deleteTag(string key);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _deleteTags(string keys);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _idsAvailable();

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _setSubscription(bool enable);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _postNotification(string json);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _syncHashedEmail(string email);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static public void _promptLocation();

    [System.Runtime.InteropServices.DllImport("__Internal")]
	extern static public void _setLogLevel(int logLevel, int visualLogLevel);


	public OneSignalIOS(string gameObjectName, string appId, bool autoPrompt, bool inAppLaunchURLs, OneSignal.OSInFocusDisplayOption displayOption, OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {
	    _init(gameObjectName, appId, autoPrompt, inAppLaunchURLs, (int)displayOption, (int)logLevel, (int)visualLevel);
	}

	public void RegisterForPushNotifications() {
		_registerForPushNotifications();
	}

	public void SendTag(string tagName, string tagValue) {
		_sendTag(tagName, tagValue);
	}

	public void SendTags(IDictionary<string, string> tags) {
		_sendTags(Json.Serialize(tags));
	}

	public void GetTags() {
		_getTags();
	}

	public void DeleteTag(string key) {
		_deleteTag(key);
	}

	public void DeleteTags(IList<string> keys) {
		_deleteTags(Json.Serialize(keys));
	}

	public void IdsAvailable() {
		_idsAvailable();
	}

	public void SetSubscription(bool enable) {
		_setSubscription(enable);
	}

	public void PostNotification(Dictionary<string, object> data) {
		_postNotification(Json.Serialize(data));
	}


    public void SyncHashedEmail(string email) {
        _syncHashedEmail(email);
    }

    public void PromptLocation() {
        _promptLocation();
    }

	public void SetLogLevel(OneSignal.LOG_LEVEL logLevel, OneSignal.LOG_LEVEL visualLevel) {
		_setLogLevel((int)logLevel, (int)visualLevel);
	}
}
#endif