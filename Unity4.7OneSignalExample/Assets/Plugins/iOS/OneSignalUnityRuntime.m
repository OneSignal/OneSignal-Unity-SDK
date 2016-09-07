/**
 * Modified MIT License
 * 
 * Copyright 2016 OneSignal
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

@implementation UIApplication(OneSignalUnityPush)

NSString* CreateNSString(const char* string) {
    return [NSString stringWithUTF8String: string ? string : ""];
}

char* unityListener = nil;
char* appId;
NSString* stringifiedNotificationOpened;
NSString* stringifiedNotificationReceived;

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
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:dictionaryToConvert options:0 error:nil];
    NSString* jsonRequestData = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
    return [jsonRequestData UTF8String];
}


+ (void)load {
    method_exchangeImplementations(class_getInstanceMethod(self, @selector(setDelegate:)), class_getInstanceMethod(self, @selector(setOneSignalUnityDelegate:)));
}

static Class delegateClass = nil;

- (void) setOneSignalUnityDelegate:(id<UIApplicationDelegate>)delegate {
    if(delegateClass != nil)
        return;
    
    delegateClass = getClassWithProtocolInHierarchy([delegate class], @protocol(UIApplicationDelegate));
    
    injectSelector(self.class, @selector(oneSignalApplication:didFinishLaunchingWithOptions:),
                   delegateClass, @selector(application:didFinishLaunchingWithOptions:));
    [self setOneSignalUnityDelegate:delegate];
}

- (BOOL)oneSignalApplication:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions {
    if ([launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey] != nil)
        initOneSignalObject(launchOptions, nil, YES, YES, YES);
    
    if ([self respondsToSelector:@selector(oneSignalApplication:didFinishLaunchingWithOptions:)])
        return [self oneSignalApplication:application didFinishLaunchingWithOptions:launchOptions];
    
    return YES;
}

void processNotificationOpened(NSString* resultString) {
    UnitySendMessage(unityListener, "onPushNotificationOpened", [resultString UTF8String]);
}

void processNotificationReceived(NSString* resultString) {
    UnitySendMessage(unityListener, "onPushNotificationReceived", [resultString UTF8String]);
}

void initOneSignalObject(NSDictionary* launchOptions, const char* appId, BOOL autoPrompt, BOOL inAppAlert, BOOL inAppLaunchURL) {
    
    static BOOL initialized = NO;
    if(!initialized) {
        
        initialized = YES;
    
        NSString* appIdStr = (appId ? [NSString stringWithUTF8String: appId] : nil);
        
        [OneSignal setValue:@"unity" forKey:@"mSDKType"];
        
        [OneSignal initWithLaunchOptions:launchOptions appId:appIdStr
            handleNotificationReceived:^(OSNotification *notification) {
                stringifiedNotificationReceived = [notification stringify];
                
                if (unityListener)
                    processNotificationReceived(stringifiedNotificationReceived);
            }
         
            handleNotificationAction:^(OSNotificationOpenedResult * result) {
            stringifiedNotificationOpened = [result stringify];
            
            if (unityListener)
                processNotificationOpened(stringifiedNotificationOpened);
        } settings:@{ kOSSettingsKeyAutoPrompt : @(autoPrompt), kOSSettingsKeyInAppAlerts : @(inAppAlert), kOSSettingsKeyInAppLaunchURL : @(inAppLaunchURL) }];
        
    }
    
}

void _init(const char* listenerName, const char* appId, BOOL autoPrompt, BOOL inAppAlerts, BOOL inAppLaunchURL, int logLevel, int visualLogLevel) {
    
    [OneSignal setLogLevel:logLevel visualLevel: visualLogLevel];

    unsigned long len = strlen(listenerName);
    unityListener = malloc(len + 1);
    strcpy(unityListener, listenerName);
    
    initOneSignalObject(nil, appId, autoPrompt, inAppAlerts, inAppLaunchURL);
    
    if (stringifiedNotificationOpened)
        processNotificationOpened(stringifiedNotificationOpened);
    
    if (stringifiedNotificationReceived)
        processNotificationReceived(stringifiedNotificationReceived);
}


void _registerForPushNotifications() {
    [OneSignal registerForPushNotifications];
}

void _sendTag(const char* tagName, const char* tagValue) {
    [OneSignal sendTag:CreateNSString(tagName) value:CreateNSString(tagValue)];
}

void _sendTags(const char* tags) {
    [OneSignal sendTagsWithJsonString:CreateNSString(tags)];
}

void _deleteTag(const char* key) {
    [OneSignal deleteTag:CreateNSString(key)];
}

void _deleteTags(const char* keys) {
    [OneSignal deleteTagsWithJsonString:CreateNSString(keys)];
}

void _getTags() {
    [OneSignal getTags:^(NSDictionary* result) {
        UnitySendMessage(unityListener, "onTagsReceived", dictionaryToJsonChar(result));
    }];
}

void _idsAvailable() {
    [OneSignal IdsAvailable:^(NSString* userId, NSString* pushToken) {
        if(pushToken == nil)
            pushToken = @"";
        
        UnitySendMessage(unityListener, "onIdsAvailable",
                         dictionaryToJsonChar(@{@"userId" : userId, @"pushToken" : pushToken}));
    }];
}

void _setSubscription(BOOL enable) {
    [OneSignal setSubscription:enable];
}

void _postNotification(const char* jsonData) {
    [OneSignal postNotificationWithJsonString:CreateNSString(jsonData)
        onSuccess:^(NSDictionary* results) {
            UnitySendMessage(unityListener, "onPostNotificationSuccess", dictionaryToJsonChar(results));
        }
        onFailure:^(NSError* error) {
            if (error.userInfo && error.userInfo[@"returned"])
                UnitySendMessage(unityListener, "onPostNotificationFailed", dictionaryToJsonChar(error.userInfo[@"returned"]));
            else
                UnitySendMessage(unityListener, "onPostNotificationFailed", "{\"error\": \"HTTP no response error\"}");
        }];

}

void _syncHashedEmail(const char* email) {
  [OneSignal syncHashedEmail:[NSString stringWithUTF8String: email]];
}

void _promptLocation() {
  [OneSignal promptLocation];
}

void _setLogLevel(int logLevel, int visualLogLevel) {
    [OneSignal setLogLevel:logLevel visualLevel: visualLogLevel];
}

@end
