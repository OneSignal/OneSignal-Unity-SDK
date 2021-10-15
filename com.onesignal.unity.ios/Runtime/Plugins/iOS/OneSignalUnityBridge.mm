/*
 * Modified MIT License
 *
 * Copyright 2021 OneSignal
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

#import <OneSignal/OneSignal.h>
#import "NSObject+ReadProperties.h"

typedef void (*BooleanResponseDelegate)(bool response);
typedef void (*StringResponseDelegate)(const char* response);
typedef void (*StateChangeDelegate)(const char* current, const char* previous);

/*
 * Helpers
 */

template <typename TObj>
TObj objFromJsonString(const char* jsonString) {
    NSData* jsonData = [[NSString stringWithUTF8String:jsonString] dataUsingEncoding:NSUTF8StringEncoding];
    NSError* error = nil;
    TObj arr = [NSJSONSerialization JSONObjectWithData:jsonData options:0 error:&error];

    if (error != nil)
        return nil;

    return arr;
}

const char* jsonStringFromDictionary(NSDictionary *dictionary) {
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dictionary options:0 error:&error];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    return [jsonString UTF8String];
}

void handleOutcomeResult(OSOutcomeEvent *outcome, BooleanResponseDelegate callback) {
    if (outcome != nil)
        callback(YES);
    else
        callback(NO);
}

/*
 * Observer singleton for global callbacks
 */

@interface OneSignalObserver : NSObject <OSPermissionObserver,
                                         OSSubscriptionObserver,
                                         OSEmailSubscriptionObserver,
                                         OSSMSSubscriptionObserver,
                                         OSInAppMessageLifecycleHandler>

+ (instancetype) sharedObserver;
@property StateChangeDelegate permissionDelegate;
@property StateChangeDelegate subscriptionDelegate;
@property StateChangeDelegate emailDelegate;
@property StateChangeDelegate smsDelegate;
@property StringResponseDelegate iamWillDisplayDelegate;
@property StringResponseDelegate iamDidDisplayDelegate;
@property StringResponseDelegate iamWillDismissDelegate;
@property StringResponseDelegate iamDidDismissDelegate;

@end

@implementation OneSignalObserver

+ (instancetype) sharedObserver {
    static dispatch_once_t pred = 0;
    static id _sharedObject = nil;
    dispatch_once(&pred, ^{
        _sharedObject = [[self alloc] init];
    });
    return _sharedObject;
}

- (instancetype) init {
    if (self = [super init]) {
        [OneSignal setInAppMessageLifecycleHandler:self];
        [OneSignal addPermissionObserver:self];
        [OneSignal addSubscriptionObserver:self];
        [OneSignal addEmailSubscriptionObserver:self];
        [OneSignal addSMSSubscriptionObserver:self];
    }

    return self;
}

- (void)onOSPermissionChanged:(OSPermissionStateChanges * _Nonnull)stateChanges {
    if (_permissionDelegate != nil) {
        auto curr = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        auto prev = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        _permissionDelegate(curr, prev);
    }
}

- (void)onOSSubscriptionChanged:(OSSubscriptionStateChanges * _Nonnull)stateChanges {
    if (_subscriptionDelegate != nil) {
        auto curr = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        auto prev = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        _subscriptionDelegate(curr, prev);
    }
}

- (void)onOSEmailSubscriptionChanged:(OSEmailSubscriptionStateChanges * _Nonnull)stateChanges {
    if (_emailDelegate != nil) {
        auto curr = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        auto prev = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        _emailDelegate(curr, prev);
    }
}

- (void)onOSSMSSubscriptionChanged:(OSSMSSubscriptionStateChanges * _Nonnull)stateChanges {
    if (_smsDelegate != nil) {
        auto curr = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        auto prev = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        _smsDelegate(curr, prev);
    }
}

- (void)onWillDisplayInAppMessage:(OSInAppMessage *)message {
    if (_iamWillDisplayDelegate != nil)
        _iamWillDisplayDelegate([message.messageId UTF8String]);
}

- (void)onDidDisplayInAppMessage:(OSInAppMessage *)message {
    if (_iamDidDisplayDelegate != nil)
        _iamDidDisplayDelegate([message.messageId UTF8String]);
}

- (void)onWillDismissInAppMessage:(OSInAppMessage *)message {
    if (_iamWillDismissDelegate != nil)
        _iamWillDismissDelegate([message.messageId UTF8String]);
}

- (void)onDidDismissInAppMessage:(OSInAppMessage *)message {
    if (_iamDidDismissDelegate != nil)
        _iamDidDismissDelegate([message.messageId UTF8String]);
}

@end

/*
 * Bridge methods
 */

extern "C" {

    void _setNotificationReceivedCallback(StringResponseDelegate callback) {
        [OneSignal setNotificationWillShowInForegroundHandler:^(OSNotification *notification, OSNotificationDisplayResponse completion) {
            NSString *stringResponse = [notification stringify];
            callback([stringResponse UTF8String]);
        }];
    }

    void _setNotificationOpenedCallback(StringResponseDelegate callback) {
        [OneSignal setNotificationOpenedHandler:^(OSNotificationOpenedResult * _Nonnull result) {
            NSString *stringResponse = [result stringify];
            callback([stringResponse UTF8String]);
        }];
    }

    void _setInAppMessageWillDisplayCallback(StringResponseDelegate callback) {
        [[OneSignalObserver sharedObserver] setIamWillDisplayDelegate:callback];
    }

    void _setInAppMessageDidDisplayCallback(StringResponseDelegate callback) {
        [[OneSignalObserver sharedObserver] setIamDidDisplayDelegate:callback];
    }

    void _setInAppMessageWillDismissCallback(StringResponseDelegate callback) {
        [[OneSignalObserver sharedObserver] setIamWillDismissDelegate:callback];
    }

    void _setInAppMessageDidDismissCallback(StringResponseDelegate callback) {
        [[OneSignalObserver sharedObserver] setIamDidDismissDelegate:callback];
    }

    void _setInAppMessageClickedCallback(StringResponseDelegate callback) {
        [OneSignal setInAppMessageClickHandler:^(OSInAppMessageAction * _Nonnull action) {
            NSString *stringResponse = [action propertiesAsJsonString];
            callback([stringResponse UTF8String]);
        }];
    }

    void _setPermissionStateChangedCallback(StateChangeDelegate callback) {
        [[OneSignalObserver sharedObserver] setPermissionDelegate:callback];
    }

    void _setSubscriptionStateChangedCallback(StateChangeDelegate callback) {
        [[OneSignalObserver sharedObserver] setSubscriptionDelegate:callback];
    }

    void _setEmailSubscriptionStateChangedCallback(StateChangeDelegate callback) {
        [[OneSignalObserver sharedObserver] setEmailDelegate:callback];
    }

    void _setSMSSubscriptionStateChangedCallback(StateChangeDelegate callback) {
        [[OneSignalObserver sharedObserver] setSmsDelegate:callback];
    }

    void _setPrivacyConsent(bool consent) {
        [OneSignal consentGranted: consent];
    }

    bool _getPrivacyConsent() {
        return false; // todo - doesn't exist
    }

    void _setRequiresPrivacyConsent(bool required) {
        [OneSignal setRequiresUserPrivacyConsent: required];
    }

    bool _getRequiresPrivacyConsent() {
        return [OneSignal requiresUserPrivacyConsent];
    }

    void _initialize(const char* appId) {
        [OneSignal setAppId:[NSString stringWithUTF8String:appId]];
        [OneSignal initWithLaunchOptions:nil];
    }

    void _promptForPushNotificationsWithUserResponse(BooleanResponseDelegate callback) {
        [OneSignal promptForPushNotificationsWithUserResponse:^(BOOL accepted) {
            callback(accepted);
        }];
    }

    void _postNotification(const char* optionsJson, StringResponseDelegate callback) {
        NSDictionary *options = objFromJsonString<NSDictionary*>(optionsJson);

        [OneSignal postNotification:options onSuccess:^(NSDictionary *result) {
            callback(jsonStringFromDictionary(result));
        } onFailure:^(NSError *error) {
            callback(NULL);
        }];
    }

    void _setTrigger(const char* key, const char* value) {
        [OneSignal addTrigger:[NSString stringWithUTF8String:key] withValue:[NSString stringWithUTF8String:value]];
    }

    void _setTriggers(const char* triggersJson) {
        NSDictionary *triggers = objFromJsonString<NSDictionary*>(triggersJson);
        [OneSignal addTriggers:triggers];
    }

    void _removeTrigger(const char* key) {
        [OneSignal removeTriggerForKey:[NSString stringWithUTF8String:key]];
    }

    void _removeTriggers(const char* triggersJson) {
        NSArray *triggers = objFromJsonString<NSArray*>(triggersJson);
        [OneSignal removeTriggersForKeys:triggers];
    }

    const char* _getTrigger(const char* key) {
        id value = [OneSignal getTriggerValueForKey:[NSString stringWithUTF8String:key]];
        return [[value string] UTF8String];
    }

    const char* _getTriggers() {
        NSDictionary *triggers = [OneSignal getTriggers];
        return jsonStringFromDictionary(triggers);
    }

    void _setInAppMessagesArePaused(bool paused) {
        [OneSignal pauseInAppMessages:paused];
    }

    bool _getInAppMessagesArePaused() {
        return [OneSignal isInAppMessagingPaused];
    }

    void _sendTag(const char* name, const char* value, BooleanResponseDelegate callback) {
        [OneSignal sendTag:[NSString stringWithUTF8String:name]
                     value:[NSString stringWithUTF8String:value]
                 onSuccess:^(NSDictionary *result) { callback(YES); }
                 onFailure:^(NSError *error) { callback(NO); }];
    }

    void _sendTags(const char* tagsJson, BooleanResponseDelegate callback) {
        NSDictionary *tags = objFromJsonString<NSDictionary*>(tagsJson);

        [OneSignal sendTags:tags
                  onSuccess:^(NSDictionary *result) { callback(YES); }
                  onFailure:^(NSError *error) { callback(NO); }];
    }

    void _getTags(StringResponseDelegate callback) {
        [OneSignal getTags:^(NSDictionary *result) {
            callback(jsonStringFromDictionary(result));
        } onFailure:^(NSError *error) {
            NSLog(@"[Onesignal] Could not get tags");
            callback(nil);
        }];
    }

    void _deleteTag(const char* name, BooleanResponseDelegate callback) {
        [OneSignal deleteTag:[NSString stringWithUTF8String:name]
                   onSuccess:^(NSDictionary *result) { callback(YES); }
                   onFailure:^(NSError *error) { callback(NO); }];
    }

    void _deleteTags(const char* tagsJson, BooleanResponseDelegate callback) {
        NSArray *tags = objFromJsonString<NSArray*>(tagsJson);

        if (tags == nil) {
            NSLog(@"[Onesignal] Could not parse tags to delete");
            callback(NO);
        }

        [OneSignal deleteTags:tags
                    onSuccess:^(NSDictionary *result) { callback(YES); }
                    onFailure:^(NSError *error) { callback(NO); }];
    }

    void _setExternalUserId(const char* externalId, const char* authHash, BooleanResponseDelegate callback) {
        [OneSignal setExternalUserId:[NSString stringWithUTF8String:externalId]
         withExternalIdAuthHashToken:[NSString stringWithUTF8String:authHash]
                         withSuccess:^(NSDictionary *results) { callback(YES); }
                         withFailure:^(NSError *error) { callback(NO); }];
    }

    void _setEmail(const char* email, const char* authHash, BooleanResponseDelegate callback) {
        [OneSignal setEmail:[NSString stringWithUTF8String:email]
     withEmailAuthHashToken:[NSString stringWithUTF8String:authHash]
                withSuccess:^{ callback(YES); }
                withFailure:^(NSError *error) { callback(NO); }];
    }

    void _setSMSNumber(const char* smsNumber, const char* authHash, BooleanResponseDelegate callback) {
        [OneSignal setSMSNumber:[NSString stringWithUTF8String:smsNumber]
           withSMSAuthHashToken:[NSString stringWithUTF8String:authHash]
                    withSuccess:^(NSDictionary *results) { callback(YES); }
                    withFailure:^(NSError *error) { callback(NO); }];
    }

    void _promptLocation() {
        [OneSignal promptLocation];
    }

    void _setShareLocation(bool share) {
        [OneSignal setLocationShared:share];
    }

    bool _getShareLocation() {
        return [OneSignal isLocationShared];
    }

    void _sendOutcome(const char* name, BooleanResponseDelegate callback) {
        [OneSignal sendOutcome:[NSString stringWithUTF8String:name]
                     onSuccess:^(OSOutcomeEvent *outcome) { handleOutcomeResult(outcome, callback); }];
    }

    void _sendUniqueOutcome(const char* name, BooleanResponseDelegate callback) {
        [OneSignal sendUniqueOutcome:[NSString stringWithUTF8String:name]
                           onSuccess:^(OSOutcomeEvent *outcome) { handleOutcomeResult(outcome, callback); }];
    }

    void _sendOutcomeWithValue(const char* name, float value, BooleanResponseDelegate callback) {
        [OneSignal sendOutcomeWithValue:[NSString stringWithUTF8String:name]
                                  value:@(value)
                              onSuccess:^(OSOutcomeEvent *outcome) { handleOutcomeResult(outcome, callback); }];
    }
}

 
