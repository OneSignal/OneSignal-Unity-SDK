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
#import "OneSignalBridgeUtil.h"

typedef void (*StringListenerDelegate)(const char* response);

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

/*
 * Observer singleton for global callbacks
 */

@interface OneSignalInAppMessagesObserver : NSObject <OSInAppMessageLifecycleHandler>

+ (instancetype) sharedInAppMessagesObserver;
@property StringListenerDelegate willDisplayDelegate;
@property StringListenerDelegate didDisplayDelegate;
@property StringListenerDelegate willDismissDelegate;
@property StringListenerDelegate didDismissDelegate;

@end

@implementation OneSignalInAppMessagesObserver

+ (instancetype) sharedInAppMessagesObserver {
    static dispatch_once_t pred = 0;
    static id _sharedObject = nil;
    dispatch_once(&pred, ^{
        _sharedObject = [[self alloc] init];
    });
    return _sharedObject;
}

- (instancetype) init {
    if (self = [super init]) {
        [OneSignal.InAppMessages setLifecycleHandler:self];
    }

    return self;
}

- (void)onWillDisplayInAppMessage:(OSInAppMessage *)message {
    if (_willDisplayDelegate != nil)
        _willDisplayDelegate([message.messageId UTF8String]);
}

- (void)onDidDisplayInAppMessage:(OSInAppMessage *)message {
    if (_didDisplayDelegate != nil)
        _didDisplayDelegate([message.messageId UTF8String]);
}

- (void)onWillDismissInAppMessage:(OSInAppMessage *)message {
    if (_willDismissDelegate != nil)
        _willDismissDelegate([message.messageId UTF8String]);
}

- (void)onDidDismissInAppMessage:(OSInAppMessage *)message {
    if (_didDismissDelegate != nil)
        _didDismissDelegate([message.messageId UTF8String]);
}

@end

/*
 * Bridge methods
 */

extern "C" {
    void _inAppMessagesSetWillDisplayCallback(StringListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setWillDisplayDelegate:callback];
    }

    void _inAppMessagesSetDidDisplayCallback(StringListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setDidDisplayDelegate:callback];
    }

    void _inAppMessagesSetWillDismissCallback(StringListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setWillDismissDelegate:callback];
    }

    void _inAppMessagesSetDidDismissCallback(StringListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setDidDismissDelegate:callback];
    }

    void _inAppMessagesSetClickedCallback(StringListenerDelegate callback) {
        [OneSignal.InAppMessages setClickHandler:^(OSInAppMessageAction * _Nonnull action) {
            callback(jsonStringFromDictionary([action jsonRepresentation]));
        }];
    }

    void _inAppMessagesSetPaused(bool paused) {
        [OneSignal.InAppMessages paused:paused];
    }

    bool _inAppMessagesGetPaused() {
        return [OneSignal.InAppMessages paused];
    }

    void _inAppMessagesAddTrigger(const char* key, const char* value) {
        [OneSignal.InAppMessages addTrigger:TO_NSSTRING(key) withValue:TO_NSSTRING(value)];
    }

    void _inAppMessagesAddTriggers(const char* triggersJson) {
        NSDictionary *triggers = objFromJsonString<NSDictionary*>(triggersJson);

        [OneSignal.InAppMessages addTriggers:triggers];
    }

    void _inAppMessagesRemoveTrigger(const char* key) {
        [OneSignal.InAppMessages removeTrigger:TO_NSSTRING(key)];
    }

    void _inAppMessagesRemoveTriggers(const char* triggersJson) {
        NSArray *triggers = objFromJsonString<NSArray*>(triggersJson);

        [OneSignal.InAppMessages removeTriggers:triggers];
    }

    void _inAppMessagesClearTriggers() {
        [OneSignal.InAppMessages clearTriggers];
    }
}