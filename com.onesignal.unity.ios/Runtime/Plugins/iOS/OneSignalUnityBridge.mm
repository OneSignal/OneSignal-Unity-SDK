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

#import <OneSignalNotifications/OneSignalNotifications.h>
#import <OneSignalUser/OneSignalUser-Swift.h>
#import <OneSignalFramework/OneSignalFramework.h>

typedef void (*BooleanResponseDelegate)(int hashCode, bool response);

/*
 * Helpers
 */

#define CALLBACK(value) callback(hashCode, value)
#define TO_NSSTRING(cstr) cstr ? [NSString stringWithUTF8String:cstr] : nil

/*
 * Bridge methods
 */

extern "C" {
    void _initialize(const char* appId) {
        [OneSignal initialize:TO_NSSTRING(appId) withLaunchOptions:nil];
    }

    void _login(const char* externalId) {
        [OneSignal login:TO_NSSTRING(externalId)];
    }

    void _loginWithJwtBearerToken(const char* externalId, const char* token) {
        [OneSignal login:TO_NSSTRING(externalId) withToken:TO_NSSTRING(token)];
    }

    void _logout() {
        [OneSignal logout];
    }

    void _setConsentGiven(bool consent) {
        [OneSignal setConsentGiven:consent];
    }

    void _setConsentRequired(bool required) {
        [OneSignal setConsentRequired:required];
    }

    void _setLaunchURLsInApp(bool launchInApp) {
        [OneSignal setLaunchURLsInApp:launchInApp];
    }

    void _enterLiveActivity(const char* activityId, const char* token, int hashCode, BooleanResponseDelegate callback) {
        [OneSignal enterLiveActivity:TO_NSSTRING(activityId)
                        withToken:TO_NSSTRING(token)
                        withSuccess:^(NSDictionary *result) { CALLBACK(YES); }
                        withFailure:^(NSError *error) { CALLBACK(NO); }];
    }

    void _exitLiveActivity(const char* activityId, int hashCode, BooleanResponseDelegate callback) {
        [OneSignal exitLiveActivity:TO_NSSTRING(activityId)
                        withSuccess:^(NSDictionary *result) { CALLBACK(YES); }
                        withFailure:^(NSError *error) { CALLBACK(NO); }];
    }
}