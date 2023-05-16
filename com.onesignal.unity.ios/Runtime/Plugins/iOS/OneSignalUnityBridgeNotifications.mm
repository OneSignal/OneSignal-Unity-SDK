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
typedef void (*PermissionListenerDelegate)(bool permission);
typedef void (*WillDisplayListenerDelegate)(const char* notification);
typedef void (*ClickListenerDelegate)(const char* notification, const char* resultActionId, const char* resultUrl);

/*
 * Helpers
 */

#define CALLBACK(value) callback(hashCode, value)

/*
 * Observer singleton for global callbacks
 */

@interface OneSignalNotificationsObserver : NSObject <OSNotificationPermissionObserver, OSNotificationLifecycleListener, OSNotificationClickListener>

+ (instancetype) sharedNotificationsObserver;
@property PermissionListenerDelegate permissionDelegate;
@property WillDisplayListenerDelegate willDisplayDelegate;
@property ClickListenerDelegate clickDelegate;
@property NSMutableDictionary<NSString *, id> *willDisplayEvents;

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
        [OneSignal.Notifications addForegroundLifecycleListener:self];
        [OneSignal.Notifications addClickListener:self];
    }

    return self;
}

- (void)onNotificationPermissionDidChange:(BOOL)permission {
    if (_permissionDelegate != nil) {
        _permissionDelegate(permission);
    }
}

- (void)onWillDisplayNotification:(OSNotificationWillDisplayEvent *)event {
    if (_willDisplayDelegate != nil) {
        NSString *stringNotification = [event.notification stringify];
        _willDisplayDelegate([stringNotification UTF8String]);

        _willDisplayEvents[event.notification.notificationId] = event;
    }
}

- (void)notificationsWillDisplayEventPreventDefault:(NSString *)notificationId {
    OSNotificationWillDisplayEvent *event = _willDisplayEvents[notificationId];
    [event preventDefault];
}

- (void)notificationsDisplay:(NSString *)notificationId {
    OSNotificationWillDisplayEvent *event = _willDisplayEvents[notificationId];
    [event.notification display];
}

- (void)onClickNotification:(OSNotificationClickEvent * _Nonnull)event {
    if (_clickDelegate != nil) {
        NSString *stringNotification = [event.notification stringify];
        NSString *stringResultActionId = event.result.actionId;
        NSString *stringResultUrl = event.result.url;
        _clickDelegate([stringNotification UTF8String], [stringResultActionId UTF8String], [stringResultUrl UTF8String]);
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

    int _notificationsGetPermissionNative() {
        return [OneSignal.Notifications permissionNative];
    }

    // bool _notificationsGetCanRequestPermission() {
    //     return [OneSignal.Notifications canRequestPermission];
    // }

    void _notificationsClearAll() {
        [OneSignal.Notifications clearAll];
    }

    void _notificationsRequestPermission(bool fallbackToSettings, int hashCode, BooleanResponseDelegate callback) {
        [OneSignal.Notifications requestPermission:^(BOOL accepted) {
            CALLBACK(accepted);
        } fallbackToSettings:fallbackToSettings];
    }

    void _notificationsAddPermissionObserver(PermissionListenerDelegate callback) {
        [[OneSignalNotificationsObserver sharedNotificationsObserver] setPermissionDelegate:callback];
    }

    void _notificationsSetForegroundWillDisplayCallback(WillDisplayListenerDelegate callback) {
        [[OneSignalNotificationsObserver sharedNotificationsObserver] setWillDisplayDelegate:callback];
    }

    void _notificationsWillDisplayEventPreventDefault(const char* notifcationId) {
        NSString *key = [NSString stringWithUTF8String:notifcationId];
        [[OneSignalNotificationsObserver sharedNotificationsObserver] notificationsWillDisplayEventPreventDefault:key];
    }

    void _notificationsDisplay(const char* notifcationId) {
        NSString *key = [NSString stringWithUTF8String:notifcationId];
        [[OneSignalNotificationsObserver sharedNotificationsObserver] notificationsDisplay:key];
    }

    void _notificationsSetClickCallback(ClickListenerDelegate callback) {
        [[OneSignalNotificationsObserver sharedNotificationsObserver] setClickDelegate:callback];
    }
}