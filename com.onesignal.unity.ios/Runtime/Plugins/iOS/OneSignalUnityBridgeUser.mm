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

#import <OneSignalNotifications/OneSignalNotifications.h>
#import <OneSignalUser/OneSignalUser-Swift.h>
#import <OneSignalFramework/OneSignalFramework.h>
#import "OneSignalBridgeUtil.h"

typedef void (*StateListenerDelegate)(const char* current, const char* previous);

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
        auto curr = jsonStringFromDictionary([[state current] jsonRepresentation]);
        auto prev = jsonStringFromDictionary([[state previous] jsonRepresentation]);
        _pushSubscriptionDelegate(curr, prev);
    }
}

@end

/*
 * Bridge methods
 * We use strdup because Unity attempts to free the memory after we return the value
 */

extern "C" {
    void _userSetLanguage(const char* languageCode) {
        [OneSignal.User setLanguage:TO_NSSTRING(languageCode)];
    }

    void _userAddAlias(const char* aliasLabel, const char* aliasId) {
        [OneSignal.User addAliasWithLabel:TO_NSSTRING(aliasLabel)
                                       id:TO_NSSTRING(aliasId)];
    }

    void _userAddAliases(const char* aliasesJson) {
        NSDictionary *aliases = dictionaryFromJsonString(aliasesJson);

        [OneSignal.User addAliases:aliases];
    }

    void _userRemoveAlias(const char* aliasLabel) {
        [OneSignal.User removeAlias:TO_NSSTRING(aliasLabel)];
    }

    void _userRemoveAliases(const char* aliasesJson) {
        NSArray *aliases = arrayFromJsonString(aliasesJson);

        if (aliases == nil) {
            NSLog(@"[Onesignal] Could not parse aliases to delete");
            //CALLBACK(NO);
        }

        [OneSignal.User removeAliases:aliases];
    }

    void _userAddEmail(const char* email) {
        [OneSignal.User addEmail:TO_NSSTRING(email)];
    }

    void _userRemoveEmail(const char* email) {
        [OneSignal.User removeEmail:TO_NSSTRING(email)];
    }

    void _userAddSms(const char* smsNumber) {
        [OneSignal.User addSms:TO_NSSTRING(smsNumber)];
    }

    void _userRemoveSms(const char* smsNumber) {
        [OneSignal.User removeSms:TO_NSSTRING(smsNumber)];
    }

    void _userAddTag(const char* key, const char* value) {
        [OneSignal.User addTagWithKey:TO_NSSTRING(key)
                                value:TO_NSSTRING(value)];
    }

    void _userAddTags(const char* tagsJson) {
        NSDictionary *tags = dictionaryFromJsonString(tagsJson);

        [OneSignal.User addTags:tags];
    }

    void _userRemoveTag(const char* key) {
        [OneSignal.User removeTag:TO_NSSTRING(key)];
    }

    void _userRemoveTags(const char* tagsJson) {
        NSArray *tags = arrayFromJsonString(tagsJson);

        if (tags == nil) {
            NSLog(@"[Onesignal] Could not parse tags to delete");
            //CALLBACK(NO);
        }

        [OneSignal.User removeTags:tags];
    }

    const char* _pushSubscriptionGetId() {
        return strdup([OneSignal.User.pushSubscription.id UTF8String]);
    }

    const char* _pushSubscriptionGetToken() {
        return strdup([OneSignal.User.pushSubscription.token UTF8String]);
    }

    bool _pushSubscriptionGetOptedIn() {
        return OneSignal.User.pushSubscription.optedIn;
    }

    void _pushSubscriptionOptIn() {
        [OneSignal.User.pushSubscription optIn];
    }

    void _pushSubscriptionOptOut() {
        [OneSignal.User.pushSubscription optOut];
    }

    void _pushSubscriptionAddStateChangedCallback(StateListenerDelegate callback) {
        [[OneSignalUserObserver sharedUserObserver] setPushSubscriptionDelegate:callback];
    }
}