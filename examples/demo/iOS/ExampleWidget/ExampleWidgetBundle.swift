//
//  ExampleWidgetBundle.swift
//  ExampleWidget
//
//  Created by Brian Smith on 5/29/24.
//

#if !targetEnvironment(macCatalyst)
import WidgetKit
import SwiftUI

@main
struct ExampleWidgetBundle: WidgetBundle {
    var body: some Widget {
        ExampleWidgetLiveActivity()
    }
}
#endif
