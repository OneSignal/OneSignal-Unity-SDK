/**
 * Modified MIT License
 * 
 * Copyright 2017 OneSignal
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

using OneSignalPush.MiniJSON;
using System.Collections.Generic;

class OneSignalPlatformHelper {
   internal static OSPermissionSubscriptionState parsePermissionSubscriptionState(OneSignalPlatform platform, string jsonStr) {
      var stateDict = Json.Deserialize(jsonStr) as Dictionary<string, object>;

      var state = new OSPermissionSubscriptionState();
      state.permissionStatus = platform.parseOSPermissionState(stateDict["permissionStatus"]);
      state.subscriptionStatus = platform.parseOSSubscriptionState(stateDict["subscriptionStatus"]);

      if (stateDict.ContainsKey("emailSubscriptionStatus"))
         state.emailSubscriptionStatus = platform.parseOSEmailSubscriptionState (stateDict ["emailSubscriptionStatus"]);
      
	  return state;
   }

   internal static OSPermissionStateChanges parseOSPermissionStateChanges(OneSignalPlatform platform, string stateChangesJSONString) {
      var stateChangesJson = Json.Deserialize(stateChangesJSONString) as Dictionary<string, object>;

      var permissionStateChanges = new OSPermissionStateChanges();
      permissionStateChanges.to = platform.parseOSPermissionState(stateChangesJson["to"]);
      permissionStateChanges.from = platform.parseOSPermissionState(stateChangesJson["from"]);

      return permissionStateChanges;
   }

   internal static OSSubscriptionStateChanges parseOSSubscriptionStateChanges(OneSignalPlatform platform, string stateChangesJSONString) {
      var stateChangesJson = Json.Deserialize(stateChangesJSONString) as Dictionary<string, object>;

      var permissionStateChanges = new OSSubscriptionStateChanges();
      permissionStateChanges.to = platform.parseOSSubscriptionState(stateChangesJson["to"]);
      permissionStateChanges.from = platform.parseOSSubscriptionState(stateChangesJson["from"]);

      return permissionStateChanges;
   }

   internal static OSEmailSubscriptionStateChanges parseOSEmailSubscriptionStateChanges(OneSignalPlatform platform, string stateChangesJSONString) {
      var stateChangesJson = Json.Deserialize(stateChangesJSONString) as Dictionary<string, object>;

      var emailStateChanges = new OSEmailSubscriptionStateChanges();
      emailStateChanges.to = platform.parseOSEmailSubscriptionState (stateChangesJson ["to"]);
      emailStateChanges.from = platform.parseOSEmailSubscriptionState (stateChangesJson ["from"]);

      return emailStateChanges;
   }
}
