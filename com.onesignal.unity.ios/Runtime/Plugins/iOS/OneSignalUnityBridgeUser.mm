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
#import <OneSignalFramework/OneSignalFramework.h>
#import "OneSignalBridgeUtil.h"

typedef void (*StateListenerDelegate)(const char* current, const char* previous);
typedef void (*UserStateListenerDelegate)(const char* current);

/*
 * Helpers
 */

#define TO_NSSTRING(cstr) cstr ? [NSString stringWithUTF8String:cstr] : nil

/*
 * Observer singleton for global callbacks
 */

@interface OneSignalUserObserver : NSObject <OSPushSubscriptionObserver>

+ (instancetype) sharedUserObserver;
@property StateListenerDelegate pushSubscriptionDelegate;

@end

@implementation OneSignalUserObserver

+ (instancetype) sharedUserObserver {
    static dispatch_once_t pred = 0;
    static id _sharedObject = nil;
    dispatch_once(&pred, ^{
        _sharedObject = [[self alloc] init];
    });
    return _sharedObject;
}

- (instancetype) init {
    if (self = [super init]) {
        [OneSignal.User.pushSubscription addObserver:self];
    }

    return self;
}

- (void)onPushSubscriptionDidChangeWithState:(OSPushSubscriptionChangedState*)state {
    if (_pushSubscriptionDelegate != nil) {
        auto curr = oneSignalJsonStringFromDictionary([[state current] jsonRepresentation]);
        auto prev = oneSignalJsonStringFromDictionary([[state previous] jsonRepresentation]);
        _pushSubscriptionDelegate(curr, prev);
    }
}

@end

/*
 * User state observer singleton for global callbacks
 */

@interface OneSignalUserStateObserver : NSObject <OSUserStateObserver>

+ (instancetype) sharedUserObserver;
@property UserStateListenerDelegate userStateDelegate;

@end

@implementation OneSignalUserStateObserver

+ (instancetype) sharedUserObserver {
    static dispatch_once_t pred = 0;
    static id _sharedObject = nil;
    dispatch_once(&pred, ^{
        _sharedObject = [[self alloc] init];
    });
    return _sharedObject;
}

- (instancetype) init {
    if (self = [super init]) {
        [OneSignal.User addObserver:self];
    }

    return self;
}

- (void)onUserStateDidChangeWithState:(OSUserChangedState*)state {
    if (_userStateDelegate != nil) {
        auto curr = oneSignalJsonStringFromDictionary([[state current] jsonRepresentation]);
        _userStateDelegate(curr);
    }
}

@end

/*
 * Bridge methods
 * We use strdup because Unity attempts to free the memory after we return the value
 */

extern "C" {
    void _oneSignalUserSetLanguage(const char* languageCode) {
        [OneSignal.User setLanguage:TO_NSSTRING(languageCode)];
    }

    void _oneSignalUserAddAlias(const char* aliasLabel, const char* aliasId) {
        [OneSignal.User addAliasWithLabel:TO_NSSTRING(aliasLabel)
                                       id:TO_NSSTRING(aliasId)];
    }

    void _oneSignalUserAddAliases(const char* aliasesJson) {
        NSDictionary *aliases = oneSignalDictionaryFromJsonString(aliasesJson);

        [OneSignal.User addAliases:aliases];
    }

    void _oneSignalUserRemoveAlias(const char* aliasLabel) {
        [OneSignal.User removeAlias:TO_NSSTRING(aliasLabel)];
    }

    void _oneSignalUserRemoveAliases(const char* aliasesJson) {
        NSArray *aliases = oneSignalArrayFromJsonString(aliasesJson);

        if (aliases == nil) {
            NSLog(@"[Onesignal] Could not parse aliases to delete");
            //CALLBACK(NO);
        }

        [OneSignal.User removeAliases:aliases];
    }

    void _oneSignalUserAddEmail(const char* email) {
        [OneSignal.User addEmail:TO_NSSTRING(email)];
    }

    void _oneSignalUserRemoveEmail(const char* email) {
        [OneSignal.User removeEmail:TO_NSSTRING(email)];
    }

    void _oneSignalUserAddSms(const char* smsNumber) {
        [OneSignal.User addSms:TO_NSSTRING(smsNumber)];
    }

    void _oneSignalUserRemoveSms(const char* smsNumber) {
        [OneSignal.User removeSms:TO_NSSTRING(smsNumber)];
    }

    const char* _oneSignalUserGetTags() {
        return strdup(oneSignalJsonStringFromDictionary([OneSignal.User getTags]));
    }

    void _oneSignalUserAddTag(const char* key, const char* value) {
        [OneSignal.User addTagWithKey:TO_NSSTRING(key)
                                value:TO_NSSTRING(value)];
    }

    void _oneSignalUserAddTags(const char* tagsJson) {
        NSDictionary *tags = oneSignalDictionaryFromJsonString(tagsJson);

        [OneSignal.User addTags:tags];
    }

    void _oneSignalUserRemoveTag(const char* key) {
        [OneSignal.User removeTag:TO_NSSTRING(key)];
    }

    void _oneSignalUserRemoveTags(const char* tagsJson) {
        NSArray *tags = oneSignalArrayFromJsonString(tagsJson);

        if (tags == nil) {
            NSLog(@"[Onesignal] Could not parse tags to delete");
            //CALLBACK(NO);
        }

        [OneSignal.User removeTags:tags];
    }

    const char* _oneSignalUserGetOneSignalId(){
        if (OneSignal.User.onesignalId == NULL) {
            return NULL;
        }
        return strdup([OneSignal.User.onesignalId UTF8String]);
    }

    const char* _oneSignalUserGetExternalId(){
        if (OneSignal.User.externalId == NULL) {
            return NULL;
        }
        return strdup([OneSignal.User.externalId UTF8String]);
    }

    const char* _oneSignalPushSubscriptionGetId() {
        if (OneSignal.User.pushSubscription.id == NULL) {
            return NULL;
        }
        return strdup([OneSignal.User.pushSubscription.id UTF8String]);
    }

    const char* _oneSignalPushSubscriptionGetToken() {
        if (OneSignal.User.pushSubscription.token == NULL) {
            return NULL;
        }
        return strdup([OneSignal.User.pushSubscription.token UTF8String]);
    }

    bool _oneSignalPushSubscriptionGetOptedIn() {
        return OneSignal.User.pushSubscription.optedIn;
    }

    void _oneSignalPushSubscriptionOptIn() {
        [OneSignal.User.pushSubscription optIn];
    }

    void _oneSignalPushSubscriptionOptOut() {
        [OneSignal.User.pushSubscription optOut];
    }

    void _oneSignalPushSubscriptionAddStateChangedCallback(StateListenerDelegate callback) {
        [[OneSignalUserObserver sharedUserObserver] setPushSubscriptionDelegate:callback];
    }

    void _oneSignalUserAddStateChangedCallback(UserStateListenerDelegate callback) {
        [[OneSignalUserStateObserver sharedUserObserver] setUserStateDelegate:callback];
    }
}
