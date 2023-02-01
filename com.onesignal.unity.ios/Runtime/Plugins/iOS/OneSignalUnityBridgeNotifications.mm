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

typedef void (*BooleanResponseDelegate)(int hashCode, bool response);
typedef void (*StringListenerDelegate)(const char* response);
typedef void (*StateListenerDelegate)(const char* current, const char* previous);
typedef bool (*NotificationWillShowDelegate)(const char* notification);

/*
 * Helpers
 */

#define CALLBACK(value) callback(hashCode, value)

/*
 * Observer singleton for global callbacks
 */

@interface OneSignalNotificationsObserver : NSObject <OSPermissionObserver>

+ (instancetype) sharedNotificationsObserver;
@property StateListenerDelegate permissionDelegate;

@end

@implementation OneSignalNotificationsObserver

+ (instancetype) sharedNotificationsObserver {
    static dispatch_once_t pred = 0;
    static id _sharedObject = nil;
    dispatch_once(&pred, ^{
        _sharedObject = [[self alloc] init];
    });
    return _sharedObject;
}

- (instancetype) init {
    if (self = [super init]) {
        [OneSignal.Notifications addPermissionObserver:self];
    }

    return self;
}

- (void)onOSPermissionChanged:(OSPermissionStateChanges*)stateChanges {
    if (_permissionDelegate != nil) {
        auto curr = jsonStringFromDictionary([[stateChanges to] jsonRepresentation]);
        auto prev = jsonStringFromDictionary([[stateChanges from] jsonRepresentation]);
        _permissionDelegate(curr, prev);
    }
}

@end

/*
 * Bridge methods
 */

extern "C" {
    bool _notificationsGetPermission() {
        return [OneSignal.Notifications permission];
    }

    /*bool _notificationsGetCanRequestPermission() {
        return [OneSignal.Notifications canRequestPermission];
    }*/

    void _notificationsClearAll() {
        [OneSignal.Notifications clearAll];
    }

    void _notificationsRequestPermission(bool fallbackToSettings, int hashCode, BooleanResponseDelegate callback) {
        [OneSignal.Notifications requestPermission:^(BOOL accepted) {
            CALLBACK(accepted);
        } fallbackToSettings:fallbackToSettings];
    }

    void _notificationsAddPermissionStateChangedCallback(StateListenerDelegate callback) {
        [[OneSignalNotificationsObserver sharedNotificationsObserver] setPermissionDelegate:callback];
    }

    void _notificationsSetWillShowHandler(NotificationWillShowDelegate callback) {
        [OneSignal.Notifications setNotificationWillShowInForegroundHandler:^(OSNotification *notification, OSNotificationDisplayResponse completion) {
            NSString *stringResponse = [notification stringify];
            bool shouldDisplay = callback([stringResponse UTF8String]);
            completion(shouldDisplay ? notification : nil);
        }];
    }

    void _notificationsSetOpenedHandler(StringListenerDelegate callback) {
        [OneSignal.Notifications setNotificationOpenedHandler:^(OSNotificationOpenedResult * _Nonnull result) {
            NSString *stringResponse = [result stringify];
            callback([stringResponse UTF8String]);
        }];
    }
}