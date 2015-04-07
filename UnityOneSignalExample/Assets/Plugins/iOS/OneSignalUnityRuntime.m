/**
 * Modified MIT License
 * 
 * Copyright 2015 OneSignal
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

OneSignal* oneSignal;
char* unityListener = nil;
char* appId;
NSMutableDictionary* launchDict;

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
        initOneSignalObject(launchOptions, nil, true);
    
    if ([self respondsToSelector:@selector(oneSignalApplication:didFinishLaunchingWithOptions:)])
        return [self oneSignalApplication:application didFinishLaunchingWithOptions:launchOptions];
    
    return YES;
}

void processNotificationOpened(NSDictionary* resultDictionary) {
    UnitySendMessage(unityListener, "onPushNotificationReceived", dictionaryToJsonChar(resultDictionary));
}

void initOneSignalObject(NSDictionary* launchOptions, const char* appId, BOOL autoRegister) {
    if (oneSignal == nil) {
        NSString* appIdStr = (appId ? [NSString stringWithUTF8String: appId] : nil);
        
        oneSignal = [[OneSignal alloc] initWithLaunchOptions:launchOptions appId:appIdStr handleNotification:^(NSString* message, NSDictionary* additionalData, BOOL isActive) {
            launchDict = [[NSMutableDictionary alloc] initWithDictionary:additionalData];
            launchDict[@"isActive"] = [NSNumber numberWithBool:isActive];
            launchDict[@"alertMessage"] = message;
            
            if (unityListener)
                processNotificationOpened(launchDict);
        } autoRegister:autoRegister];
    }
}

void _init(const char* listenerName, const char* appId, BOOL autoRegister, int logLevel, int visualLogLevel) {
    [OneSignal setLogLevel:logLevel visualLevel: visualLogLevel];

    unsigned long len = strlen(listenerName);
	unityListener = malloc(len + 1);
	strcpy(unityListener, listenerName);
    
    initOneSignalObject(nil, appId, autoRegister);
    
    if (launchDict)
        processNotificationOpened(launchDict);
}


void _registerForPushNotifications() {
    [oneSignal registerForPushNotifications];
}

void _sendTag(const char* tagName, const char* tagValue) {
	[oneSignal sendTag:CreateNSString(tagName) value:CreateNSString(tagValue)];
}

void _sendTags(const char* tags) {
    [oneSignal sendTagsWithJsonString:CreateNSString(tags)];
}

void _deleteTag(const char* key) {
    [oneSignal deleteTag:CreateNSString(key)];
}

void _deleteTags(const char* keys) {
    [oneSignal deleteTagsWithJsonString:CreateNSString(keys)];
}

void _getTags() {
    [oneSignal getTags:^(NSDictionary* result) {
        UnitySendMessage(unityListener, "onTagsReceived", dictionaryToJsonChar(result));
    }];
}

void _idsAvailable() {
    [oneSignal IdsAvailable:^(NSString* userId, NSString* pushToken) {
        if(pushToken == nil)
            pushToken = @"";
        
        UnitySendMessage(unityListener, "onIdsAvailable",
                         dictionaryToJsonChar(@{@"userId" : userId, @"pushToken" : pushToken}));
    }];
}

void _setLogLevel(int logLevel, int visualLogLevel) {
    [OneSignal setLogLevel:logLevel visualLevel: visualLogLevel];
}

@end