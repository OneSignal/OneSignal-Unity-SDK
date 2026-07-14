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

typedef void (*StringListenerDelegate)(const char* response);
typedef void (*ClickListenerDelegate)(const char* message, const char* result, int urlType);

/*
 * Helpers
 */

#define CALLBACK(value) callback(hashCode, value)
#define TO_NSSTRING(cstr) cstr ? [NSString stringWithUTF8String:cstr] : nil

/*
 * Observer singleton for global callbacks
 */

@interface OneSignalInAppMessagesObserver : NSObject <OSInAppMessageLifecycleListener, OSInAppMessageClickListener>

+ (instancetype) sharedInAppMessagesObserver;
@property StringListenerDelegate willDisplayDelegate;
@property StringListenerDelegate didDisplayDelegate;
@property StringListenerDelegate willDismissDelegate;
@property StringListenerDelegate didDismissDelegate;
@property ClickListenerDelegate clickDelegate;

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
        [OneSignal.InAppMessages addLifecycleListener:self];
        [OneSignal.InAppMessages addClickListener:self];
    }

    return self;
}

- (void)onWillDisplayInAppMessage:(OSInAppMessageWillDisplayEvent *)event {
    if (_willDisplayDelegate != nil)
        _willDisplayDelegate([event.message.messageId UTF8String]);
}

- (void)onDidDisplayInAppMessage:(OSInAppMessageDidDisplayEvent *)event {
    if (_didDisplayDelegate != nil)
        _didDisplayDelegate([event.message.messageId UTF8String]);
}

- (void)onWillDismissInAppMessage:(OSInAppMessageWillDismissEvent *)event {
    if (_willDismissDelegate != nil)
        _willDismissDelegate([event.message.messageId UTF8String]);
}

- (void)onDidDismissInAppMessage:(OSInAppMessageDidDismissEvent *)event {
    if (_didDismissDelegate != nil)
        _didDismissDelegate([event.message.messageId UTF8String]);
}

- (void)onClickInAppMessage:(OSInAppMessageClickEvent * _Nonnull)event {
    if (_clickDelegate != nil) {
        auto message = oneSignalJsonStringFromDictionary([[event message] jsonRepresentation]);
        auto result = oneSignalJsonStringFromDictionary([[event result] jsonRepresentation]);
        _clickDelegate(message, result, event.result.urlTarget);
    }
}

@end

/*
 * Bridge methods
 */

extern "C" {
    void _oneSignalInAppMessagesSetWillDisplayCallback(StringListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setWillDisplayDelegate:callback];
    }

    void _oneSignalInAppMessagesSetDidDisplayCallback(StringListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setDidDisplayDelegate:callback];
    }

    void _oneSignalInAppMessagesSetWillDismissCallback(StringListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setWillDismissDelegate:callback];
    }

    void _oneSignalInAppMessagesSetDidDismissCallback(StringListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setDidDismissDelegate:callback];
    }

    void _oneSignalInAppMessagesSetClickCallback(ClickListenerDelegate callback) {
        [[OneSignalInAppMessagesObserver sharedInAppMessagesObserver] setClickDelegate:callback];
    }

    void _oneSignalInAppMessagesSetPaused(bool paused) {
        [OneSignal.InAppMessages paused:paused];
    }

    bool _oneSignalInAppMessagesGetPaused() {
        return [OneSignal.InAppMessages paused];
    }

    void _oneSignalInAppMessagesAddTrigger(const char* key, const char* value) {
        [OneSignal.InAppMessages addTrigger:TO_NSSTRING(key) withValue:TO_NSSTRING(value)];
    }

    void _oneSignalInAppMessagesAddTriggers(const char* triggersJson) {
        NSDictionary *triggers = oneSignalDictionaryFromJsonString(triggersJson);

        [OneSignal.InAppMessages addTriggers:triggers];
    }

    void _oneSignalInAppMessagesRemoveTrigger(const char* key) {
        [OneSignal.InAppMessages removeTrigger:TO_NSSTRING(key)];
    }

    void _oneSignalInAppMessagesRemoveTriggers(const char* triggersJson) {
        NSArray *triggers = oneSignalArrayFromJsonString(triggersJson);

        [OneSignal.InAppMessages removeTriggers:triggers];
    }

    void _oneSignalInAppMessagesClearTriggers() {
        [OneSignal.InAppMessages clearTriggers];
    }
}