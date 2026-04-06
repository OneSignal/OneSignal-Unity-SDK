#if !targetEnvironment(macCatalyst)
import WidgetKit
import SwiftUI

@main
struct OneSignalWidgetBundle: WidgetBundle {
    var body: some Widget {
        OneSignalWidgetLiveActivity()
    }
}
#endif
