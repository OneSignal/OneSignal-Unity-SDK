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

#import "OneSignal.h"
#import <objc/runtime.h>

char* os_cStringCopy(const char* string) {
    if (string == NULL)
    return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}

NSString* CreateNSString(const char* string) {
    return [NSString stringWithUTF8String: string ? string : ""];
}

char* unityListener = nil;
char* appId;
OSNotificationOpenedResult* actionNotification;

static Class getClassWithProtocolInHierarchy(Class searchClass, Protocol* protocolToFind) {
    if (!class_conformsToProtocol(searchClass, protocolToFind)) {
        if ([searchClass superclass] == [NSObject class])
            return nil;
        
        Class foundClass = getClassWithProtocolInHierarchy([searchClass superclass], protocolToFind);
        if (foundClass)
            return foundClass;
        
        return searchClass;
    }
    
    return searchClass;
}

static void injectSelector(Class newClass, SEL newSel, Class addToClass, SEL makeLikeSel) {
    Method newMeth = class_getInstanceMethod(newClass, newSel);
    IMP imp = method_getImplementation(newMeth);
    const char* methodTypeEncoding = method_getTypeEncoding(newMeth);
    
    BOOL successful = class_addMethod(addToClass, makeLikeSel, imp, methodTypeEncoding);
    if (!successful) {
        class_addMethod(addToClass, newSel, imp, methodTypeEncoding);
        newMeth = class_getInstanceMethod(addToClass, newSel);
        
        Method orgMeth = class_getInstanceMethod(addToClass, makeLikeSel);
        
        method_exchangeImplementations(orgMeth, newMeth);
    }
}

const char* dictionaryToJsonChar(NSDictionary* dictionaryToConvert) {
    if (!dictionaryToConvert)
        return [NSString new].UTF8String;
    
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:dictionaryToConvert options:0 error:nil];
    NSString* jsonRequestData = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    return jsonRequestData.UTF8String;
}

char* dictionaryToJsonNonConstChar(NSDictionary* dictionaryToConvert) {
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:dictionaryToConvert options:0 error:nil];
    NSString* jsonRequestData = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    char* dyStr = malloc(sizeof(char) * jsonRequestData.length);
    strcpy(dyStr, jsonRequestData.UTF8String);
    
    return dyStr;
}

NSString* dictionaryToNSString(NSDictionary* dict) {
    return CreateNSString(dictionaryToJsonChar(dict));
}

@interface OSUnityPermissionAndSubscriptionObserver : NSObject<OSPermissionObserver, OSSubscriptionObserver, OSEmailSubscriptionObserver>
- (void)onOSPermissionChanged:(OSPermissionStateChanges*)stateChanges;
- (void)onOSSubscriptionChanged:(OSSubscriptionStateChanges*)stateChanges;
- (void)onOSEmailSubscriptionChanged:(OSEmailSubscriptionStateChanges *)stateChanges;
@end

@implementation OSUnityPermissionAndSubscriptionObserver
- (void)onOSPermissionChanged:(OSPermissionStateChanges*)stateChanges {
    UnitySendMessage(unityListener, "onOSPermissionChanged", dictionaryToJsonChar([stateChanges toDictionary]));
}

- (void)onOSSubscriptionChanged:(OSSubscriptionStateChanges*)stateChanges {
    UnitySendMessage(unityListener, "onOSSubscriptionChanged", dictionaryToJsonChar([stateChanges toDictionary]));
}

- (void)onOSEmailSubscriptionChanged:(OSEmailSubscriptionStateChanges *)stateChanges {
    UnitySendMessage(unityListener, "onOSEmailSubscriptionChanged", dictionaryToJsonChar([stateChanges toDictionary]));
}
@end

static OSUnityPermissionAndSubscriptionObserver* osUnityObserver;

@implementation UIApplication(OneSignalUnityPush)

+ (void)load {
    method_exchangeImplementations(class_getInstanceMethod(self, @selector(setDelegate:)), class_getInstanceMethod(self, @selector(setOneSignalUnityDelegate:)));
}

static Class delegateClass = nil;

- (void) setOneSignalUnityDelegate:(id<UIApplicationDelegate>)delegate {
    if (delegateClass) {
        [self setOneSignalUnityDelegate:delegate];
        return;
    }
    
    delegateClass = getClassWithProtocolInHierarchy([delegate class], @protocol(UIApplicationDelegate));
    
    injectSelector(self.class, @selector(oneSignalApplication:didFinishLaunchingWithOptions:), delegateClass, @selector(application:didFinishLaunchingWithOptions:));
    [self setOneSignalUnityDelegate:delegate];
}

- (BOOL)oneSignalApplication:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions {
    initOneSignalObject(launchOptions, nil, 1, true, false, true);
    
    if ([self respondsToSelector:@selector(oneSignalApplication:didFinishLaunchingWithOptions:)])
        return [self oneSignalApplication:application didFinishLaunchingWithOptions:launchOptions];
    
    return YES;
}

void processNotificationOpened(NSString* openString) {
    UnitySendMessage(unityListener, "onPushNotificationOpened", [openString UTF8String]);
}

void processNotificationReceived(NSString* notificationString) {
    UnitySendMessage(unityListener, "onPushNotificationReceived", [notificationString UTF8String]);
}

void processInAppMessageClicked(char* inAppMessageActionString) {
    UnitySendMessage(unityListener, "onInAppMessageClicked", inAppMessageActionString);
}

char* createInAppMessageJsonString(OSInAppMessageAction* action) {
    return os_cStringCopy(dictionaryToJsonChar(
    @{
        @"click_name" : action.clickName ? action.clickName : @"",
        @"click_url" : action.clickUrl ? action.clickUrl.absoluteString : @"",
        @"first_click" : @(action.firstClick),
        @"closes_message" : @(action.closesMessage)
    }));
}

void initOneSignalObject(NSDictionary* launchOptions, const char* appId, int displayOption, BOOL inAppLaunchURL, BOOL autoPrompt, BOOL fromColdStart) {
    
    NSString* appIdStr = (appId ? [NSString stringWithUTF8String: appId] : nil);
    
    [OneSignal setValue:@"unity" forKey:@"mSDKType"];
    
    [OneSignal initWithLaunchOptions:launchOptions appId:appIdStr handleNotificationReceived:^(OSNotification* notification) {
        if (unityListener)
            processNotificationReceived([notification stringify]);
    } handleNotificationAction:^(OSNotificationOpenedResult* openResult) {
        actionNotification = openResult;
        if (unityListener)
            processNotificationOpened([openResult stringify]);
    } settings:@{kOSSettingsKeyAutoPrompt: @(autoPrompt),
                 kOSSettingsKeyInFocusDisplayOption: @(displayOption),
                 kOSSettingsKeyInAppLaunchURL: @(inAppLaunchURL),
                 @"kOSSettingsKeyInOmitNoAppIdLogging": @(fromColdStart)}];
    
    [OneSignal setInAppMessageClickHandler:^(OSInAppMessageAction* action) {
        processInAppMessageClicked(createInAppMessageJsonString(action));
    }];
}

void _init(const char* listenerName, const char* appId, BOOL autoPrompt, BOOL inAppLaunchURL, int displayOption, int logLevel, int visualLogLevel, bool requiresUserPrivacyConsent) {
    [OneSignal setRequiresUserPrivacyConsent:requiresUserPrivacyConsent];
    
    [OneSignal setLogLevel:logLevel visualLevel: visualLogLevel];
    
    unsigned long len = strlen(listenerName);
    unityListener = malloc(len + 1);
    strcpy(unityListener, listenerName);
    
    initOneSignalObject(nil, appId, displayOption, inAppLaunchURL, autoPrompt, false);
    
    if (actionNotification)
        processNotificationOpened([actionNotification stringify]);
}


void _registerForPushNotifications() {
    [OneSignal registerForPushNotifications];
}

void _sendTag(const char* tagName, const char* tagValue) {
    [OneSignal sendTag:CreateNSString(tagName) value:CreateNSString(tagValue)];
}

void _sendTags(const char* tags) {
    NSString* jsonString = CreateNSString(tags);
    
    NSError* jsonError;
    
    NSData* data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary* keyValuePairs = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&jsonError];
    if (!jsonError)
        [OneSignal sendTags:keyValuePairs];
}

void _deleteTag(const char* key) {
    [OneSignal deleteTag:CreateNSString(key)];
}

void _deleteTags(const char* keys) {
    NSString* jsonString = CreateNSString(keys);
    
    NSError* jsonError;
    
    NSData* data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSArray* kk = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonError];
    if (!jsonError)
        [OneSignal deleteTags:kk];
}

void _getTags(const char* delegate) {
    NSString* delegateId = CreateNSString(delegate);
    
    [OneSignal getTags:^(NSDictionary* result) {
        if (!result)
            result = @{};
        
        NSString* response = dictionaryToNSString(result);
        NSDictionary *data = @{ @"delegate_id" : delegateId, @"response" : response };
        
        UnitySendMessage(unityListener, "onTagsReceived", dictionaryToJsonChar(data));
    }];
}

void _idsAvailable(const char* delegate) {
    NSString* delegateId = CreateNSString(delegate);
    
    [OneSignal IdsAvailable:^(NSString* userId, NSString* pushToken) {
        if (!userId)
            userId = @"";
            
        if (!pushToken)
            pushToken = @"";
        
        NSString* response = dictionaryToNSString(@{ @"userId" : userId, @"pushToken" : pushToken });
        NSDictionary* data = @{ @"delegate_id" : delegateId, @"response" : response };
        
        UnitySendMessage(unityListener, "onIdsAvailable", dictionaryToJsonChar(data));
    }];
}

void _setSubscription(BOOL enable) {
    [OneSignal setSubscription:enable];
}

void _postNotification(const char* delegateSuccess, const char* delegateFailure, const char* jsonData) {
    NSString* delegateIdSuccess = CreateNSString(delegateSuccess);
    NSString* delegateIdFailure = CreateNSString(delegateFailure);
    
    NSString* delegate = dictionaryToNSString(@{ @"success" : delegateIdSuccess, @"failure" : delegateIdFailure });
    
    NSString* jsonString = CreateNSString(jsonData);
    NSError* jsonError;
    
    NSData* data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary* jsd = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&jsonError];
    if (!jsonError)
        [OneSignal postNotification:jsd onSuccess:^(NSDictionary* results) {
            if (!results)
                results = @{};
            
            NSString* response = dictionaryToNSString(results);
            NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
            UnitySendMessage(unityListener, "onPostNotificationSuccess", dictionaryToJsonChar(data));
        } onFailure:^(NSError* error) {
            
            NSString* response = CreateNSString([[OneSignal parseNSErrorAsJsonString:error] UTF8String]);
            NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
            UnitySendMessage(unityListener, "onPostNotificationFailed", dictionaryToJsonChar(data));
        }];
}

void _syncHashedEmail(const char* email) {
    [OneSignal syncHashedEmail:[NSString stringWithUTF8String: email]];
}

void _promptLocation() {
    [OneSignal promptLocation];
}

void _setInFocusDisplayType(int type) {
    OneSignal.inFocusDisplayType = type;
}

void _addPermissionObserver() {
    if (!osUnityObserver)
        osUnityObserver = [OSUnityPermissionAndSubscriptionObserver alloc];
    [OneSignal addPermissionObserver:osUnityObserver];
}

void _removePermissionObserver() {
    [OneSignal removePermissionObserver:osUnityObserver];
}

void _addSubscriptionObserver() {
    if (!osUnityObserver)
        osUnityObserver = [OSUnityPermissionAndSubscriptionObserver alloc];
    [OneSignal addSubscriptionObserver:osUnityObserver];
}

void _removeSubscriptionObserver() {
    [OneSignal removeSubscriptionObserver:osUnityObserver];
}

void _addEmailSubscriptionObserver() {
    if (!osUnityObserver)
        osUnityObserver = [OSUnityPermissionAndSubscriptionObserver alloc];
    [OneSignal addEmailSubscriptionObserver:osUnityObserver];
}

void _removeEmailSubscriptionObserver() {
    [OneSignal removeEmailSubscriptionObserver:osUnityObserver];
}

char* _getPermissionSubscriptionState() {
    return dictionaryToJsonNonConstChar([[OneSignal getPermissionSubscriptionState] toDictionary]);
}

void _promptForPushNotificationsWithUserResponse() {
    [OneSignal promptForPushNotificationsWithUserResponse:^(BOOL accepted) {
        UnitySendMessage(unityListener, "onPromptForPushNotificationsWithUserResponse", (accepted ? @"true" : @"false").UTF8String);
    }];
}

void _setOneSignalLogLevel(int logLevel, int visualLogLevel) {
    [OneSignal setLogLevel:logLevel visualLevel: visualLogLevel];
}

void _setUnauthenticatedEmail(const char* delegateSuccess, const char* delegateFailure, const char* email) {
    NSString* delegateIdSuccess = CreateNSString(delegateSuccess);
    NSString* delegateIdFailure = CreateNSString(delegateFailure);
    
    NSString* delegate = dictionaryToNSString(@{ @"success" : delegateIdSuccess, @"failure" : delegateIdFailure });
    
    [OneSignal setEmail:CreateNSString(email) withSuccess:^{
        NSString* response = dictionaryToNSString(@{ @"status" : @"success" });
        NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
        UnitySendMessage(unityListener, "onSetEmailSuccess", dictionaryToJsonChar(data));
    } withFailure:^(NSError *error) {
        NSString* response = CreateNSString([[OneSignal parseNSErrorAsJsonString:error] UTF8String]);
        NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
        UnitySendMessage(unityListener, "onSetEmailFailure", dictionaryToJsonChar(data));
    }];
}

void _setEmail(const char* delegateSuccess, const char* delegateFailure, const char *email, const char *emailAuthCode) {
    NSString* delegateIdSuccess = CreateNSString(delegateSuccess);
    NSString* delegateIdFailure = CreateNSString(delegateFailure);
    
    NSString* delegate = dictionaryToNSString(@{ @"success" : delegateIdSuccess, @"failure" : delegateIdFailure });
    
    [OneSignal setEmail:CreateNSString(email) withEmailAuthHashToken:CreateNSString(emailAuthCode) withSuccess:^{
        NSString* response = dictionaryToNSString(@{ @"status" : @"success" });
        NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
        UnitySendMessage(unityListener, "onSetEmailSuccess", dictionaryToJsonChar(data));
    } withFailure:^(NSError *error) {
        NSString* response = CreateNSString([[OneSignal parseNSErrorAsJsonString:error] UTF8String]);
        NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
        UnitySendMessage(unityListener, "onSetEmailFailure", dictionaryToJsonChar(data));
    }];
}

void _logoutEmail(const char* delegateSuccess, const char* delegateFailure) {
    NSString* delegateIdSuccess = CreateNSString(delegateSuccess);
    NSString* delegateIdFailure = CreateNSString(delegateFailure);
    
    NSString* delegate = dictionaryToNSString(@{ @"success" : delegateIdSuccess, @"failure" : delegateIdFailure });
    
    [OneSignal logoutEmailWithSuccess:^{
        NSString* response = dictionaryToNSString(@{ @"status" : @"success" });
        NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
        UnitySendMessage(unityListener, "onLogoutEmailSuccess", dictionaryToJsonChar(data));
    } withFailure:^(NSError *error) {
        NSString* response = CreateNSString([[OneSignal parseNSErrorAsJsonString:error] UTF8String]);
        NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
        UnitySendMessage(unityListener, "onLogoutEmailFailure", dictionaryToJsonChar(data));
    }];
}

void _userDidProvideConsent(bool consent) {
    [OneSignal consentGranted:consent];
}

bool _userProvidedConsent() {
    return ![OneSignal requiresUserPrivacyConsent];
}

void _setRequiresUserPrivacyConsent(bool required) {
    [OneSignal setRequiresUserPrivacyConsent:required];
}

void _setLocationShared(bool shared) {
    [OneSignal setLocationShared:shared];
}

void _setExternalUserId(const char* delegate, const char *externalId) {
    NSString* delegateId = CreateNSString(delegate);
    [OneSignal setExternalUserId:CreateNSString(externalId) withCompletion:^(NSDictionary *results) {
        NSString* response = dictionaryToNSString(results);
        NSDictionary* data = @{ @"delegate_id" : delegateId, @"response" : response };
        UnitySendMessage(unityListener, "onExternalUserIdUpdateCompletion", dictionaryToJsonChar(data));
    }];
}

void _setExternalUserIdWithAuthToken(const char* delegateSuccess, const char* delegateFailure, const char *externalId, const char *authHashToken) {
    NSString* delegateIdSuccess = CreateNSString(delegateSuccess);
    NSString* delegateIdFailure = CreateNSString(delegateFailure);
    
    NSString* delegate = dictionaryToNSString(@{ @"success" : delegateIdSuccess, @"failure" : delegateIdFailure });
    
    [OneSignal setExternalUserId:CreateNSString(externalId) withExternalIdAuthHashToken:CreateNSString(authHashToken) withSuccess:^(NSDictionary *results) {
        NSString* response = dictionaryToNSString(results);
        NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
        UnitySendMessage(unityListener, "onExternalUserIdUpdateCompletion", dictionaryToJsonChar(data));
    } withFailure: ^(NSError* error) {
        [OneSignal onesignal_Log:ONE_S_LL_VERBOSE message:[NSString stringWithFormat:@"Set external user id Failure with error: %@", error]];
        NSString* response = CreateNSString([[OneSignal parseNSErrorAsJsonString:error] UTF8String]);
        NSDictionary* data = @{ @"delegate_id" : delegate, @"response" : response };
        UnitySendMessage(unityListener, "onExternalUserIdUpdateCompletionFailure", dictionaryToJsonChar(data));
    }];
}

void _removeExternalUserId(const char* delegate) {
    NSString* delegateId = CreateNSString(delegate);
    [OneSignal removeExternalUserId:^(NSDictionary *results) {
        NSString* response = dictionaryToNSString(results);
        NSDictionary* data = @{ @"delegate_id" : delegateId, @"response" : response };
        UnitySendMessage(unityListener, "onExternalUserIdUpdateCompletion", dictionaryToJsonChar(data));
    }];
}

void _addTriggers(char *triggers) {
    NSString* jsonString = CreateNSString(triggers);
    
    NSError* jsonError;
    
    NSData* data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary* triggerKeyValuePairs = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&jsonError];
    if (!jsonError)
        [OneSignal addTriggers:triggerKeyValuePairs];
}

void _removeTriggerForKey(char *key) {
    NSString* triggerKey = CreateNSString(key);
    [OneSignal removeTriggerForKey:triggerKey];
}

void _removeTriggersForKeys(char *keys) {
    NSString* jsonString = CreateNSString(keys);
    
    NSError* jsonError;
    
    NSData* data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSArray* triggerKeys = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonError];
    if (!jsonError)
        [OneSignal removeTriggersForKeys:triggerKeys];
}

char* _getTriggerValueForKey(char *key) {
    NSString* triggerKey = CreateNSString(key);
    NSDictionary* triggerValue = @{ @"value" : [OneSignal getTriggerValueForKey:triggerKey] };
    return os_cStringCopy(dictionaryToJsonChar(triggerValue));
}

void _pauseInAppMessages(bool pause) {
    [OneSignal pauseInAppMessages:pause];
}

void _sendOutcome(const char* delegate, char* name) {
    NSString* delegateId = CreateNSString(delegate);
    NSString* outcomeName = CreateNSString(name);
    
    [OneSignal sendOutcome:outcomeName onSuccess:^(OSOutcomeEvent *outcomeEvent) {
        NSString* response = dictionaryToNSString(outcomeEvent.jsonRepresentation);
        NSDictionary* data = @{ @"delegate_id" : delegateId, @"response" : response };
        UnitySendMessage(unityListener, "onSendOutcomeSuccess", dictionaryToJsonChar(data));
    }];
}

void _sendUniqueOutcome(const char* delegate, char* name) {
    NSString* delegateId = CreateNSString(delegate);
    NSString* outcomeName = CreateNSString(name);

    [OneSignal sendUniqueOutcome:outcomeName onSuccess:^(OSOutcomeEvent *outcomeEvent) {
        NSString* response = dictionaryToNSString(outcomeEvent.jsonRepresentation);
        NSDictionary* data = @{ @"delegate_id" : delegateId, @"response" : response };
        UnitySendMessage(unityListener, "onSendOutcomeSuccess", dictionaryToJsonChar(data));
    }];
}

void _sendOutcomeWithValue(const char* delegate, char* name, float value) {
    NSString* delegateId = CreateNSString(delegate);
    NSString* outcomeName = CreateNSString(name);
    NSNumber* outcomeValue = [NSNumber numberWithFloat:value];
    
    [OneSignal sendOutcomeWithValue:outcomeName value:outcomeValue onSuccess:^(OSOutcomeEvent *outcomeEvent) {
        NSString* response = dictionaryToNSString(outcomeEvent.jsonRepresentation);
        NSDictionary* data = @{ @"delegate_id" : delegateId, @"response" : response };
        UnitySendMessage(unityListener, "onSendOutcomeSuccess", dictionaryToJsonChar(data));
    }];
}

@end
