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

package com.onesignal;

public final class UnityIAMLifecycleHandler extends OSInAppMessageLifecycleHandler {
    private final WrapperLifecycleHandler wrapperLifeCycleHandler;

    public UnityIAMLifecycleHandler(WrapperLifecycleHandler handler) {
        wrapperLifeCycleHandler = handler;
    }

    public interface WrapperLifecycleHandler {
        void onWillDisplayInAppMessage(OSInAppMessage message);
        void onDidDisplayInAppMessage(OSInAppMessage message);
        void onWillDismissInAppMessage(OSInAppMessage message);
        void onDidDismissInAppMessage(OSInAppMessage message);
    }

    @Override
    public void onWillDisplayInAppMessage(OSInAppMessage message) {
        if (wrapperLifeCycleHandler != null)
            wrapperLifeCycleHandler.onWillDisplayInAppMessage(message);
    }

    @Override
    public void onDidDisplayInAppMessage(OSInAppMessage message) {
        if (wrapperLifeCycleHandler != null)
            wrapperLifeCycleHandler.onDidDisplayInAppMessage(message);
    }

    @Override
    public void onWillDismissInAppMessage(OSInAppMessage message) {
        if (wrapperLifeCycleHandler != null)
            wrapperLifeCycleHandler.onWillDismissInAppMessage(message);
    }

    @Override
    public void onDidDismissInAppMessage(OSInAppMessage message) {
        if (wrapperLifeCycleHandler != null)
            wrapperLifeCycleHandler.onDidDismissInAppMessage(message);
    }
}
