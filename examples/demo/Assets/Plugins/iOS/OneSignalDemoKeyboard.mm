#import <UIKit/UIKit.h>

// Resign first responder so the iOS keyboard view tears down immediately.
// UIToolkit's TextField.Blur() and TouchScreenKeyboard APIs don't reliably
// dismiss the UIKit keyboard when a UIToolkit-owned modal closes.
extern "C" void OneSignalDemoEndEditing() {
    [UIApplication.sharedApplication.keyWindow endEditing:YES];
}
