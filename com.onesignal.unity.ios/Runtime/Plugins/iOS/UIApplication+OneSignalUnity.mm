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
#import "UIApplication+OneSignalUnity.h"
#import <objc/runtime.h>

static Class oneSignalGetClassWithProtocolInHierarchy(Class searchClass, Protocol *protocolToFind) {
    if (!class_conformsToProtocol(searchClass, protocolToFind)) {
        if ([searchClass superclass] == [NSObject class])
            return nil;
        Class foundClass = oneSignalGetClassWithProtocolInHierarchy([searchClass superclass], protocolToFind);
        if (foundClass)
            return foundClass;
        return searchClass;
    }
    return searchClass;
}

// from OneSignalSelectorHelpers.m
BOOL oneSignalInjectSelector(Class newClass, SEL newSel, Class addToClass, SEL makeLikeSel) {
    Method newMeth = class_getInstanceMethod(newClass, newSel);
    IMP imp = method_getImplementation(newMeth);

    const char* methodTypeEncoding = method_getTypeEncoding(newMeth);
    BOOL existing = class_getInstanceMethod(addToClass, makeLikeSel) != NULL;

    if (existing) {
        class_addMethod(addToClass, newSel, imp, methodTypeEncoding);
        newMeth = class_getInstanceMethod(addToClass, newSel);
        Method orgMeth = class_getInstanceMethod(addToClass, makeLikeSel);
        method_exchangeImplementations(orgMeth, newMeth);
    }
    else {
        class_addMethod(addToClass, makeLikeSel, imp, methodTypeEncoding);
    }

    return existing;
}

static bool swizzled = false;

@implementation UIApplication (OneSignalUnity)

+ (void)load {
    method_exchangeImplementations(
        class_getInstanceMethod(self, @selector(setDelegate:)),
        class_getInstanceMethod(self, @selector(setOneSignalUnityDelegate:))
    );
}

- (void)setOneSignalUnityDelegate:(id <UIApplicationDelegate>)delegate {
    if (swizzled) {
        [self setOneSignalUnityDelegate:delegate];
        return;
    }

    Class delegateClass = oneSignalGetClassWithProtocolInHierarchy([delegate class], @protocol(UIApplicationDelegate));

    oneSignalInjectSelector(
        self.class, @selector(oneSignalApplication:didFinishLaunchingWithOptions:),
        delegateClass, @selector(application:didFinishLaunchingWithOptions:)
    );

    swizzled = true;

    [self setOneSignalUnityDelegate:delegate];
}

- (BOOL)oneSignalApplication:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [OneSignalWrapper setSdkType:@"unity"];
    [OneSignalWrapper setSdkVersion:@"050114"];
    [OneSignal initialize:nil withLaunchOptions:launchOptions];

    if ([self respondsToSelector:@selector(oneSignalApplication:didFinishLaunchingWithOptions:)])
        return [self oneSignalApplication:application didFinishLaunchingWithOptions:launchOptions];

    return YES;
}

@end
