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
 
#import "OneSignal.h"
 
typedef void (*BooleanResponseDelegate)(bool response);
typedef void (*StringResponseDelegate)(const char* response);
 
extern "C" {

    void _setNotificationReceivedCallback() {
   
    }
                                               
    void _setNotificationOpenedCallback() {
    
    }
    
    void _setInAppMessageClickedCallback() {
    
    }
    
    void _setPermissionStateChangedCallback() {
    
    }
    
    void _setSubscriptionStateChangedCallback() {
    
    }
    
    void _setEmailSubscriptionStateChangedCallback() {
    
    }
    
    void _setSMSSubscriptionStateChangedCallback() {
    
    }

    void _setPrivacyConsent(bool consent) {
        [OneSignal consentGranted: consent];
    }
    
    bool _getPrivacyConsent() {
        return false; // todo - doesn't exist
    }
 
    void _setRequiresPrivacyConsent(bool required) {
        [OneSignal setRequiresUserPrivacyConsent: required];
    }
    
    bool _getRequiresPrivacyConsent() {
        return [OneSignal requiresUserPrivacyConsent];
    }
 
    void _initialize(const char* appId) {
        
    }
    
    void _registerForPushNotifications() {
    
    }
    
    void _promptForPushNotificationsWithUserResponse(BooleanResponseDelegate callback) {
    
    }
    
    void _clearOneSignalNotifications() {
    
    }
    
    void _postNotification(string options, StringCallbackProxy callback) {
    
    }
    
    void _setTrigger(string key, string value) {
    
    }
    
    void _setTriggers() {
        // todo - signature
    }
    
    void _removeTrigger(string key) {
    
    }
    
    void _removeTriggers() {
        // todo - signature
    }
    
    void _getTrigger(string key) {
        // todo - signature
    }
    
    void _getTriggers() {
        // todo - signature
    }
    
    void _setInAppMessagesArePaused(bool paused) {
    
    }
    
    bool _getInAppMessagesArePaused() {
    
    }
    
    void _sendTag(string name, string value, StringResponseDelegate callback) {
    
    }
    
    void _sendTags(string tagJson, StringResponseDelegate callback) {
    
    }
                                                                       
    void _getTags(StringResponseDelegate callback) {
    
    }
                                                      
    void _deleteTag(string name, StringResponseDelegate callback) {
    
    }
                                                                     
    void _deleteTags(string tagsJson, StringResponseDelegate callback) {
    
    }
                                                                          
    void _setExternalUserId(string externalId, string authHash, StringResponseDelegate callback) {
    
    }
                                                                                                    
    void _setEmail(string email, string authHash, StringResponseDelegate callback) {
    
    }
                                                                                      
    void _setSMSNumber(string smsNumber, string authHash, StringResponseDelegate callback) {
    
    }
    
    void _promptLocation() {
    
    }
    
    void _setShareLocation(bool share) {
    
    }
    
    bool _getShareLocation() {
    
    }
 }
 
 