/*
 * Modified MIT License
 *
 * Copyright 2023 OneSignal
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

#import <OneSignalCore/OneSignalCore.h>
#import <OneSignalOSCore/OneSignalOSCore-Swift.h>
#import <OneSignalNotifications/OneSignalNotifications.h>
#import <OneSignalUser/OneSignalUser-Swift.h>
#import "OneSignalLiveActivities/OneSignalLiveActivities-Swift.h"
#import <OneSignalFramework/OneSignalFramework.h>
#import "OneSignalBridgeUtil.h"

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
    void _oneSignalEnterLiveActivity(const char* activityId, const char* token, int hashCode, BooleanResponseDelegate callback) {
        [OneSignal.LiveActivities enter:TO_NSSTRING(activityId)
                        withToken:TO_NSSTRING(token)
                        withSuccess:^(NSDictionary *result) { CALLBACK(YES); }
                        withFailure:^(NSError *error) { CALLBACK(NO); }];
    }

    void _oneSignalExitLiveActivity(const char* activityId, int hashCode, BooleanResponseDelegate callback) {
        [OneSignal.LiveActivities exit:TO_NSSTRING(activityId)
                        withSuccess:^(NSDictionary *result) { CALLBACK(YES); }
                        withFailure:^(NSError *error) { CALLBACK(NO); }];
    }


    void _oneSignalSetupDefaultLiveActivity(const char* optionsJson) {
        #if !TARGET_OS_MACCATALYST
        LiveActivitySetupOptions *laOptions = nil;

        if (optionsJson) {
            NSDictionary *optionsDict = oneSignalDictionaryFromJsonString(optionsJson);

            laOptions = [LiveActivitySetupOptions alloc];
            [laOptions setEnablePushToStart:[optionsDict[@"enablePushToStart"] boolValue]];
            [laOptions setEnablePushToUpdate:[optionsDict[@"enablePushToUpdate"] boolValue]];
        }

        if (@available(iOS 16.1, *)) {
            [OneSignalLiveActivitiesManagerImpl setupDefaultWithOptions:laOptions];
        } else {
            [OneSignalLog onesignalLog:ONE_S_LL_ERROR message:[NSString stringWithFormat:@"cannot setupDefault on iOS < 16.1"]];
        }
        #endif
    }

    void _oneSignalStartDefaultLiveActivity(const char* activityId, const char* attributesJson, const char* contentJson) {
        #if !TARGET_OS_MACCATALYST
        if (@available(iOS 16.1, *)) {
            NSDictionary *attributes = oneSignalDictionaryFromJsonString(attributesJson);
            NSDictionary *content = oneSignalDictionaryFromJsonString(contentJson);

            [OneSignalLiveActivitiesManagerImpl startDefault:TO_NSSTRING(activityId) attributes:attributes content:content];
        } else {
            [OneSignalLog onesignalLog:ONE_S_LL_ERROR message:[NSString stringWithFormat:@"cannot startDefault on iOS < 16.1"]];
        }
        #endif
    }

    void _oneSignalSetPushToStartToken(const char* activityType, const char* token) {
        #if !TARGET_OS_MACCATALYST
        NSError* err=nil;

        if (@available(iOS 17.2, *)) {
            [OneSignalLiveActivitiesManagerImpl setPushToStartToken:TO_NSSTRING(activityType) withToken:TO_NSSTRING(token) error:&err];
            if (err) {
                [OneSignalLog onesignalLog:ONE_S_LL_ERROR message:[NSString stringWithFormat:@"activityType must be the name of your ActivityAttributes struct"]];
            }
        } else {
            [OneSignalLog onesignalLog:ONE_S_LL_ERROR message:[NSString stringWithFormat:@"cannot setPushToStartToken on iOS < 17.2"]];
        }
        #endif
    }

    void _oneSignalRemovePushToStartToken(const char* activityType) {
        #if !TARGET_OS_MACCATALYST
        NSError* err=nil;
        if (@available(iOS 17.2, *)) {
            [OneSignalLiveActivitiesManagerImpl removePushToStartToken:TO_NSSTRING(activityType) error:&err];

            if (err) {
                [OneSignalLog onesignalLog:ONE_S_LL_ERROR message:[NSString stringWithFormat:@"activityType must be the name of your ActivityAttributes struct"]];
            }
        } else {
            [OneSignalLog onesignalLog:ONE_S_LL_ERROR message:[NSString stringWithFormat:@"cannot removePushToStartToken on iOS < 17.2"]];
        }
        #endif
    }
}