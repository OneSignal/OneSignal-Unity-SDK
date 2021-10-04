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
 
typedef void (*BooleanResponseDelegate)(bool response);
typedef void (*StringResponseDelegate)(const char* response);

@interface OneSignalObserver : NSObject <OSPermissionObserver, OSSubscriptionObserver, OSEmailSubscriptionObserver, OSSMSSubscriptionObserver>

+ (instancetype) sharedObserver;
@property StringResponseDelegate permissionDelegate;
@property StringResponseDelegate subscriptionDelegate;
@property StringResponseDelegate emailDelegate;
@property StringResponseDelegate smsDelegate;

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

- (void)onOSPermissionChanged:(OSPermissionStateChanges * _Nonnull)stateChanges {
    <#code#>
}

- (void)onOSSubscriptionChanged:(OSSubscriptionStateChanges * _Nonnull)stateChanges {
    <#code#>
}

- (void)onOSEmailSubscriptionChanged:(OSEmailSubscriptionStateChanges * _Nonnull)stateChanges {
    <#code#>
}

- (void)onOSSMSSubscriptionChanged:(OSSMSSubscriptionStateChanges * _Nonnull)stateChanges {
    <#code#>
}

@end
 
extern "C" {

    void _setNotificationReceivedCallback(StringResponseDelegate callback) {
        // todo - doesn't exist?
    }
                                               
    void _setNotificationOpenedCallback(StringResponseDelegate callback) {
        [OneSignal setNotificationOpenedHandler:^(OSNotificationOpenedResult * _Nonnull result) {
            // todo
        }];
    }
    
    void _setInAppMessageClickedCallback(StringResponseDelegate callback) {
        [OneSignal setInAppMessageClickHandler:^(OSInAppMessageAction * _Nonnull action) {
            // todo
        }]
    }
    
    void _setPermissionStateChangedCallback(StringResponseDelegate callback) {
        [[OneSignalObserver sharedObserver] setPermissionDelegate:callback];
        [OneSignal addPermissionObserver:[OneSignalObserver sharedObserver]];
    }
    
    void _setSubscriptionStateChangedCallback(StringResponseDelegate callback) {
        [[OneSignalObserver sharedObserver] setSubscriptionDelegate:callback];
        [OneSignal addSubscriptionObserver:[OneSignalObserver sharedObserver]];
    }
    
    void _setEmailSubscriptionStateChangedCallback(StringResponseDelegate callback) {
        [[OneSignalObserver sharedObserver] setEmailDelegate:callback];
        [OneSignal addEmailSubscriptionObserver:[OneSignalObserver sharedObserver]];
    }
    
    void _setSMSSubscriptionStateChangedCallback(StringResponseDelegate callback) {
        [[OneSignalObserver sharedObserver] setSmsDelegate:callback];
        [OneSignal addSMSSubscriptionObserver:[OneSignalObserver sharedObserver]];
    }

    void _setPrivacyConsent(bool consent) {
        [OneSignal consentGranted: consent];
    }
    
    bool _getPrivacyConsent() {
        return false; // todo - doesn't exist?
    }
 
    void _setRequiresPrivacyConsent(bool required) {
        [OneSignal setRequiresUserPrivacyConsent: required];
    }
    
    bool _getRequiresPrivacyConsent() {
        return [OneSignal requiresUserPrivacyConsent];
    }
 
    void _initialize(const char* appId) {
        // todo - init here?
        [OneSignal setAppId:[NSString stringWithUTF8String:appId]];
    }
    
    void _registerForPushNotifications() {
        
    }
    
    void _promptForPushNotificationsWithUserResponse(BooleanResponseDelegate callback) {
        [OneSignal promptForPushNotificationsWithUserResponse:^(BOOL accepted) {
            callback(accepted);
        }];
    }
    
    void _clearOneSignalNotifications() {
        // todo - doesn't exist?
    }
    
    void _postNotification(const char* options, StringResponseDelegate callback) {
        
        // todo
        [OneSignal postNotification:nil onSuccess:^(NSDictionary *result) {
            // todo
        } onFailure:^(NSError *error) {
            // todo
        }];
    }
    
    void _setTrigger(const char* key, const char* value) {
        [OneSignal addTrigger:[NSString stringWithUTF8String:key] withValue:[NSString stringWithUTF8String:value]];
    }
    
    void _setTriggers() {
        // todo - signature
    }
    
    void _removeTrigger(const char* key) {
        [OneSignal removeTriggerForKey:[NSString stringWithUTF8String:key]];
    }
    
    void _removeTriggers() {
        // todo - signature
    }
    
    void _getTrigger(const char* key) {
        // todo - signature
    }
    
    void _getTriggers() {
        // todo - signature
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
    
    void _sendTags(const char* tagJson, BooleanResponseDelegate callback) {
        // todo
        [OneSignal sendTags:nil
                  onSuccess:^(NSDictionary *result) { callback(YES); }
                  onFailure:^(NSError *error) { callback(NO); }];
    }
                                                                       
    void _getTags(StringResponseDelegate callback) {
        [OneSignal getTags:^(NSDictionary *result) {
            // todo
        } onFailure:^(NSError *error) {
            callback(nil);
        }];
    }
                                                      
    void _deleteTag(const char* name, BooleanResponseDelegate callback) {
        [OneSignal deleteTag:[NSString stringWithUTF8String:name]
                   onSuccess:^(NSDictionary *result) { callback(YES); }
                   onFailure:^(NSError *error) { callback(NO); }];
    }
                                                                     
    void _deleteTags(const char* tagsJson, BooleanResponseDelegate callback) {
        // todo
        [OneSignal deleteTags:nil
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
                     onSuccess:^(OSOutcomeEvent *outcome) {
            // todo
        }]
    }
        
    void _sendUniqueOutcome(const char* name, BooleanResponseDelegate callback) {
        
    }

    void _sendOutcomeWithValue(const char* name, float value, BooleanResponseDelegate callback) {
        
    }
}
 
 
