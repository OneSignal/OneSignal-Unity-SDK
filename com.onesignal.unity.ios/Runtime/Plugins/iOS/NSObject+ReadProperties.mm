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

#import "NSObject+ReadProperties.h"
#import <objc/runtime.h>

@implementation NSObject (ReadProperties)

- (NSDictionary *)propertiesAsDictionary {
    NSMutableDictionary *propertyDictionary = [NSMutableDictionary dictionary];
    unsigned int outCount, i;
    objc_property_t *propertyList = class_copyPropertyList([self class], &outCount);

    for (i = 0; i < outCount; i++) {
        objc_property_t property = propertyList[i];
        const char *propNameCString = property_getName(property);
        NSString *propertyName = [NSString stringWithUTF8String:propNameCString];
        id propertyValue = [self valueForKey:propertyName];

        if (propertyValue)
            propertyDictionary[propertyName] = propertyValue;
    }

    free(propertyList);
    return propertyDictionary;
}

- (NSString *)propertiesAsJsonString {
    NSDictionary *propertyDictionary = [self propertiesAsDictionary];

    NSError *error = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:propertyDictionary options:0 error:&error];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];

    return jsonString;
}

@end