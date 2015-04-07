/**
 * Modified MIT License
 * 
 * Copyright 2015 OneSignal
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControllerExample : MonoBehaviour {

	private static string extraMessage;

	void Start () {
		extraMessage = null;

        // Enable line below to debug issues with setuping OneSignal. (logLevel, visualLogLevel)
        //OneSignal.SetLogLevel(OneSignal.LOG_LEVEL.INFO, OneSignal.LOG_LEVEL.INFO);

        // The only required method you need to call to setup OneSignal to recieve push notifications.
        // Call before using any other methods on OneSignal.
		// Should only be called once when your game is loaded.
        // OneSignal.Init(OneSignal_AppId, GoogleProjectNumber, NotificationReceivedHandler(optional));
        OneSignal.Init("b2f7f966-d8cc-11e4-bed1-df8f05be55ba", "703322744261", HandleNotification);
	}

	// Gets called when the user opens the notification or gets one while in your game.
	// The name of the method can be anything as long as the signature matches.
	// Method must be static or this object should be marked as DontDestroyOnLoad
	private static void HandleNotification(string message, Dictionary<string, object> additionalData, bool isActive) {
		print("GameControllerExample:HandleNotification");
		print(message);

		// When isActive is true this means the user is currently in your game.
		// Use isActive and your own game logic so you don't interupt the user with a popup or menu when they are in the middle of playing your game.
		if (additionalData != null) {
			if (additionalData.ContainsKey("discount")) {
                extraMessage = (string)additionalData["discount"];
				// Take user to your store.
			}
			else if (additionalData.ContainsKey("actionSelected")) {
				// actionSelected equals the id on the button the user pressed.
				// actionSelected will equal "__DEFAULT__" when the notification itself was tapped when buttons were present.
				extraMessage = "Pressed ButtonId: " + additionalData["actionSelected"];
			}
		}
	}

	// Test Menu
	// Includes SendTag/SendTags and getting the userID and pushToken
	void OnGUI () {
		GUIStyle customTextSize = new GUIStyle("button");
		customTextSize.fontSize = 30;

		GUIStyle guiBoxStyle = new GUIStyle("box");
		guiBoxStyle.fontSize = 30;

		GUI.Box(new Rect(10, 10, 390, 250), "Test Menu", guiBoxStyle);

		if (GUI.Button (new Rect (60, 80, 300, 60), "SendTags", customTextSize)) {
			// You can tags users with key value pairs like this:
            OneSignal.SendTag("UnityTestKey", "TestValue");
			// Or use an IDictionary if you need to set more than one tag.
            OneSignal.SendTags(new Dictionary<string, string>() { { "UnityTestKey2", "value2" }, { "UnityTestKey3", "value3" } });

			// You can delete a single tag with it's key.
            // OneSignal.DeleteTag("UnityTestKey");
			// Or delete many with an IList.
            // OneSignal.DeleteTags(new List<string>() {"UnityTestKey2", "UnityTestKey3" });
		}

		if (GUI.Button (new Rect (60, 170, 300, 60), "GetIds", customTextSize)) {
            OneSignal.GetIdsAvailable((userId, pushToken) => {
				extraMessage = "UserID:\n" + userId + "\n\nPushToken:\n" + pushToken;
			});
		}

		if (extraMessage != null) {
			guiBoxStyle.alignment = TextAnchor.UpperLeft;
			guiBoxStyle.wordWrap = true;
			GUI.Box (new Rect (10, 300, Screen.width - 20, Screen.height - 310), extraMessage, guiBoxStyle);
		}
	}
}
