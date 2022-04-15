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

#import <OneSignal/OneSignal.h>

typedef void (*BooleanResponseDelegate)(int hashCode, bool response);
typedef void (*StringResponseDelegate)(int hashCode, const char* response);

typedef void (*BooleanListenerDelegate)(bool response);
typedef void (*StringListenerDelegate)(const char* response);
typedef void (*StateListenerDelegate)(const char* current, const char* previous);

typedef bool (*NotificationWillShowInForegroundDelegate)(const char* notification);

/*
 * Helpers
 */

#define CALLBACK(value) callback(hashCode, value)
#define TO_NSSTRING(cstr) cstr ? [NSString stringWithUTF8String:cstr] : nil

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

/*
 * Observer singleton for global callbacks
 */

@interface OneSignalObserver : NSObject <OSPermissionObserver,
        OSSubscriptionObserver,
        OSEmailSubscriptionObserver,
        OSSMSSubscriptionObserver,
        OSInAppMessageLifecycleHandler>

+ (instancetype) sharedObserver;
@property StateListenerDelegate permissionDelegate;
@property StateListenerDelegate subscriptionDelegate;
@property StateListenerDelegate emailDelegate;
@property StateListenerDelegate smsDelegate;
@property StringListenerDelegate iamWillDisplayDelegate;
@property StringListenerDelegate iamDidDisplayDelegate;
@property StringListenerDelegate iamWillDismissDelegate;
@property StringListenerDelegate iamDidDismissDelegate;

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
        auto prev = jsonStringFromDictionary([[stateChanges from] toDictionary]);
        _permissionDelegate(curr, prev);
    }
}

- (void)onOSSubscriptionChanged:(OSSubscriptionStateChanges * _Nonnull)stateChanges {
    if (_subscriptionDelegate != nil) {
        auto curr = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        auto prev = jsonStringFromDictionary([[stateChanges from] toDictionary]);
        _subscriptionDelegate(curr, prev);
    }
}

- (void)onOSEmailSubscriptionChanged:(OSEmailSubscriptionStateChanges * _Nonnull)stateChanges {
    if (_emailDelegate != nil) {
        auto curr = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        auto prev = jsonStringFromDictionary([[stateChanges from] toDictionary]);
        _emailDelegate(curr, prev);
    }
}

- (void)onOSSMSSubscriptionChanged:(OSSMSSubscriptionStateChanges * _Nonnull)stateChanges {
    if (_smsDelegate != nil) {
        auto curr = jsonStringFromDictionary([[stateChanges to] toDictionary]);
        auto prev = jsonStringFromDictionary([[stateChanges from] toDictionary]);
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

void _setNotificationReceivedCallback(NotificationWillShowInForegroundDelegate callback) {
    [OneSignal setNotificationWillShowInForegroundHandler:^(OSNotification *notification, OSNotificationDisplayResponse completion) {
        NSString *stringResponse = [notification stringify];
        bool shouldDisplay = callback([stringResponse UTF8String]);
        completion(shouldDisplay ? notification : nil);
    }];
}

void _setNotificationOpenedCallback(StringListenerDelegate callback) {
    [OneSignal setNotificationOpenedHandler:^(OSNotificationOpenedResult * _Nonnull result) {
        NSString *stringResponse = [result stringify];
        callback([stringResponse UTF8String]);
    }];
}

void _setInAppMessageWillDisplayCallback(StringListenerDelegate callback) {
    [[OneSignalObserver sharedObserver] setIamWillDisplayDelegate:callback];
}

void _setInAppMessageDidDisplayCallback(StringListenerDelegate callback) {
    [[OneSignalObserver sharedObserver] setIamDidDisplayDelegate:callback];
}

void _setInAppMessageWillDismissCallback(StringListenerDelegate callback) {
    [[OneSignalObserver sharedObserver] setIamWillDismissDelegate:callback];
}

void _setInAppMessageDidDismissCallback(StringListenerDelegate callback) {
    [[OneSignalObserver sharedObserver] setIamDidDismissDelegate:callback];
}

void _setInAppMessageClickedCallback(StringListenerDelegate callback) {
    [OneSignal setInAppMessageClickHandler:^(OSInAppMessageAction * _Nonnull action) {
        callback(jsonStringFromDictionary([action jsonRepresentation]));
    }];
}

void _setPermissionStateChangedCallback(StateListenerDelegate callback) {
    [[OneSignalObserver sharedObserver] setPermissionDelegate:callback];
}

void _setSubscriptionStateChangedCallback(StateListenerDelegate callback) {
    [[OneSignalObserver sharedObserver] setSubscriptionDelegate:callback];
}

void _setEmailSubscriptionStateChangedCallback(StateListenerDelegate callback) {
    [[OneSignalObserver sharedObserver] setEmailDelegate:callback];
}

void _setSMSSubscriptionStateChangedCallback(StateListenerDelegate callback) {
    [[OneSignalObserver sharedObserver] setSmsDelegate:callback];
}

const char* _getDeviceState() {
    auto deviceState = [OneSignal getDeviceState];
    auto stateStr = jsonStringFromDictionary([deviceState jsonRepresentation]);
    return strdup(stateStr);
}

void _setLogLevel(int logLevel, int alertLevel) {
    [OneSignal setLogLevel:(ONE_S_LOG_LEVEL) logLevel
               visualLevel:(ONE_S_LOG_LEVEL) alertLevel];
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

void _setLaunchURLsInApp(bool launchInApp) {
    [OneSignal setLaunchURLsInApp: launchInApp];
}

void _initialize(const char* appId) {
    [OneSignal setAppId:TO_NSSTRING(appId)];
    [OneSignal initWithLaunchOptions:nil];
}

void _promptForPushNotificationsWithUserResponse(int hashCode, BooleanResponseDelegate callback) {
    [OneSignal promptForPushNotificationsWithUserResponse:^(BOOL accepted) {
        CALLBACK(accepted);
    }];
}

void _disablePush(bool disable) {
    [OneSignal disablePush:disable];
}

void _postNotification(const char* optionsJson, int hashCode, StringResponseDelegate callback) {
    NSDictionary *options = objFromJsonString<NSDictionary*>(optionsJson);

    [OneSignal postNotification:options onSuccess:^(NSDictionary *result) {
        CALLBACK(jsonStringFromDictionary(result));
    } onFailure:^(NSError *error) {
        CALLBACK(NULL);
    }];
}

void _setTrigger(const char* key, const char* value) {
    [OneSignal addTrigger:TO_NSSTRING(key) withValue:TO_NSSTRING(value)];
}

void _setTriggers(const char* triggersJson) {
    NSDictionary *triggers = objFromJsonString<NSDictionary*>(triggersJson);
    [OneSignal addTriggers:triggers];
}

void _removeTrigger(const char* key) {
    [OneSignal removeTriggerForKey:TO_NSSTRING(key)];
}

void _removeTriggers(const char* triggersJson) {
    NSArray *triggers = objFromJsonString<NSArray*>(triggersJson);
    [OneSignal removeTriggersForKeys:triggers];
}

const char* _getTrigger(const char* key) {
    id value = [OneSignal getTriggerValueForKey:TO_NSSTRING(key)];
    NSString *asString = [NSString stringWithFormat:@"%@", value];
    return strdup([asString UTF8String]);
}

const char* _getTriggers() {
    NSDictionary *triggers = [OneSignal getTriggers];
    return strdup(jsonStringFromDictionary(triggers));
}

void _setInAppMessagesArePaused(bool paused) {
    [OneSignal pauseInAppMessages:paused];
}

bool _getInAppMessagesArePaused() {
    return [OneSignal isInAppMessagingPaused];
}

void _sendTag(const char* name, const char* value, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal sendTag:TO_NSSTRING(name)
                 value:TO_NSSTRING(value)
             onSuccess:^(NSDictionary *result) { CALLBACK(YES); }
             onFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _sendTags(const char* tagsJson, int hashCode, BooleanResponseDelegate callback) {
    NSDictionary *tags = objFromJsonString<NSDictionary*>(tagsJson);

    [OneSignal sendTags:tags
              onSuccess:^(NSDictionary *result) { CALLBACK(YES); }
              onFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _getTags(int hashCode, StringResponseDelegate callback) {
    [OneSignal getTags:^(NSDictionary *result) {
        CALLBACK(jsonStringFromDictionary(result));
    } onFailure:^(NSError *error) {
        NSLog(@"[Onesignal] Could not get tags");
        CALLBACK(nil);
    }];
}

void _deleteTag(const char* name, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal deleteTag:TO_NSSTRING(name)
               onSuccess:^(NSDictionary *result) { CALLBACK(YES); }
               onFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _deleteTags(const char* tagsJson, int hashCode, BooleanResponseDelegate callback) {
    NSArray *tags = objFromJsonString<NSArray*>(tagsJson);

    if (tags == nil) {
        NSLog(@"[Onesignal] Could not parse tags to delete");
        CALLBACK(NO);
    }

    [OneSignal deleteTags:tags
                onSuccess:^(NSDictionary *result) { CALLBACK(YES); }
                onFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _setExternalUserId(const char* externalId, const char* authHash, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal setExternalUserId:TO_NSSTRING(externalId)
     withExternalIdAuthHashToken:TO_NSSTRING(authHash)
                     withSuccess:^(NSDictionary *results) { CALLBACK(YES); }
                     withFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _setEmail(const char* email, const char* authHash, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal setEmail:TO_NSSTRING(email)
 withEmailAuthHashToken:TO_NSSTRING(authHash)
            withSuccess:^{ CALLBACK(YES); }
            withFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _setSMSNumber(const char* smsNumber, const char* authHash, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal setSMSNumber:TO_NSSTRING(smsNumber)
       withSMSAuthHashToken:TO_NSSTRING(authHash)
                withSuccess:^(NSDictionary *results) { CALLBACK(YES); }
                withFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _removeExternalUserId(int hashCode, BooleanResponseDelegate callback) {
    [OneSignal removeExternalUserId:^(NSDictionary *results) { CALLBACK(YES); }
                        withFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _logoutEmail(int hashCode, BooleanResponseDelegate callback) {
    [OneSignal logoutEmailWithSuccess:^{ CALLBACK(YES); }
                          withFailure:^(NSError *error) { CALLBACK(NO); }];
}

void _logoutSMSNumber(int hashCode, BooleanResponseDelegate callback) {
    [OneSignal logoutSMSNumberWithSuccess:^(NSDictionary *results) { CALLBACK(YES); }
                              withFailure:^(NSError *error) { CALLBACK(NO); }];
}
    
void _setLanguage(const char* languageCode, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal setLanguage:TO_NSSTRING(languageCode)
               withSuccess:^{ CALLBACK(YES); }
               withFailure:^(NSError *error) { CALLBACK(NO); }];
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

void _sendOutcome(const char* name, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal sendOutcome:TO_NSSTRING(name)
                 onSuccess:^(OSOutcomeEvent *outcome) { CALLBACK(outcome != nil); }];
}

void _sendUniqueOutcome(const char* name, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal sendUniqueOutcome:TO_NSSTRING(name)
                       onSuccess:^(OSOutcomeEvent *outcome) { CALLBACK(outcome != nil); }];
}

void _sendOutcomeWithValue(const char* name, float value, int hashCode, BooleanResponseDelegate callback) {
    [OneSignal sendOutcomeWithValue:TO_NSSTRING(name)
                              value:@(value)
                          onSuccess:^(OSOutcomeEvent *outcome) { CALLBACK(outcome != nil); }];
}
}


