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

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires UI buttons to their corresponding methods at runtime.
/// This script is added by the UIBuilder and handles the button click event wiring.
/// </summary>
public class ButtonWiring : MonoBehaviour
{
    public UIController uiController;
    public DialogManager dialogManager;

    private void Start()
    {
        Debug.Log("ButtonWiring Start - beginning button wiring...");

        // Find DialogManager
        if (dialogManager == null)
        {
            dialogManager = GetComponent<DialogManager>();
            if (dialogManager == null)
                dialogManager = FindObjectOfType<DialogManager>();
        }
        
        // Find UIController
        if (uiController == null)
        {
            uiController = GetComponent<UIController>();
            if (uiController == null)
                uiController = FindObjectOfType<UIController>();
        }

        // Wire SDK-independent buttons first
        WireDialogButtons();
        WirePushGridButtons();
        WireMainButtons(); // Wire main dashboard buttons to show dialogs
        SetupDeleteCallbacks(); // Setup delete callbacks for item cards
        
#if ONE_SIGNAL_INSTALLED
        // Wire SDK-dependent buttons
        WireSdkButtons();
#else
        Debug.Log("OneSignal SDK not installed. SDK-specific button wiring skipped.");
#endif
    }
    
    private void SetupDeleteCallbacks()
    {
        if (uiController == null) return;
        
        // Set up delete callbacks to call SDK remove methods
        uiController.onAliasDelete = (key) => {
            Debug.Log($"Removing alias: {key}");
#if ONE_SIGNAL_INSTALLED
            OneSignalSDK.OneSignal.User.RemoveAlias(key);
#endif
        };
        
        uiController.onEmailDelete = (email) => {
            Debug.Log($"Removing email: {email}");
#if ONE_SIGNAL_INSTALLED
            OneSignalSDK.OneSignal.User.RemoveEmail(email);
#endif
        };
        
        uiController.onSmsDelete = (sms) => {
            Debug.Log($"Removing SMS: {sms}");
#if ONE_SIGNAL_INSTALLED
            OneSignalSDK.OneSignal.User.RemoveSms(sms);
#endif
        };
        
        uiController.onTagDelete = (key) => {
            Debug.Log($"Removing tag: {key}");
#if ONE_SIGNAL_INSTALLED
            OneSignalSDK.OneSignal.User.RemoveTag(key);
#endif
        };
        
        uiController.onTriggerDelete = (key) => {
            Debug.Log($"Removing trigger: {key}");
#if ONE_SIGNAL_INSTALLED
            OneSignalSDK.OneSignal.InAppMessages.RemoveTrigger(key);
#endif
        };
        
        Debug.Log("Delete callbacks set up");
    }

    private void WireMainButtons()
    {
        if (dialogManager == null)
        {
            Debug.LogWarning("DialogManager not found - main buttons not wired");
            return;
        }

        Transform mainContent = transform.Find("MainDashboard/MainScrollView/Viewport/Content");
        if (mainContent == null)
        {
            Debug.LogWarning("Main content not found for button wiring");
            return;
        }

        // Wire buttons to show dialogs directly
        WireButton(
            mainContent,
            "UserSection/LoginUserButton",
            () =>
            {
                dialogManager.ShowLoginDialog(
                    (externalId) =>
                    {
                        Debug.Log($"Login with External ID: {externalId}");
#if ONE_SIGNAL_INSTALLED
                        OneSignalSDK.OneSignal.Login(externalId);
#endif
                    }
                );
            }
        );

        WireButton(
            mainContent,
            "AliasesSection/AddAliasButton",
            () =>
            {
                dialogManager.ShowAliasDialog(
                    (key, value) =>
                    {
                        Debug.Log($"Add Alias: {key} = {value}");
                        if (uiController != null) uiController.AddAlias(key, value);
#if ONE_SIGNAL_INSTALLED
                        OneSignalSDK.OneSignal.User.AddAlias(key, value);
#endif
                    }
                );
            }
        );

        WireButton(
            mainContent,
            "EmailsSection/AddEmailButton",
            () =>
            {
                dialogManager.ShowEmailDialog(
                    (email) =>
                    {
                        Debug.Log($"Add Email: {email}");
                        if (uiController != null) uiController.AddEmail(email);
#if ONE_SIGNAL_INSTALLED
                        OneSignalSDK.OneSignal.User.AddEmail(email);
#endif
                    }
                );
            }
        );

        WireButton(
            mainContent,
            "SmsSection/AddSmsButton",
            () =>
            {
                dialogManager.ShowSmsDialog(
                    (sms) =>
                    {
                        Debug.Log($"Add SMS: {sms}");
                        if (uiController != null) uiController.AddSms(sms);
#if ONE_SIGNAL_INSTALLED
                        OneSignalSDK.OneSignal.User.AddSms(sms);
#endif
                    }
                );
            }
        );

        WireButton(
            mainContent,
            "TagsSection/AddTagButton",
            () =>
            {
                dialogManager.ShowTagDialog(
                    (key, value) =>
                    {
                        Debug.Log($"Add Tag: {key} = {value}");
                        if (uiController != null) uiController.AddTag(key, value);
#if ONE_SIGNAL_INSTALLED
                        OneSignalSDK.OneSignal.User.AddTag(key, value);
#endif
                    }
                );
            }
        );

        WireButton(
            mainContent,
            "TriggersSection/AddTriggerButton",
            () =>
            {
                dialogManager.ShowTriggerDialog(
                    (key, value) =>
                    {
                        Debug.Log($"Add Trigger: {key} = {value}");
                        if (uiController != null) uiController.AddTrigger(key, value);
#if ONE_SIGNAL_INSTALLED
                        OneSignalSDK.OneSignal.InAppMessages.AddTrigger(key, value);
#endif
                    }
                );
            }
        );

        // Remove All Tags
        WireButton(
            mainContent,
            "TagsSection/RemoveAllTagsButton",
            () =>
            {
                Debug.Log("Remove All Tags");
                if (uiController != null) uiController.RemoveAllTags();
#if ONE_SIGNAL_INSTALLED
                OneSignalSDK.OneSignal.User.RemoveTags();
#endif
            }
        );
        
        // Remove All Triggers
        WireButton(
            mainContent,
            "TriggersSection/RemoveAllTriggersButton",
            () =>
            {
                Debug.Log("Remove All Triggers");
                if (uiController != null) uiController.RemoveAllTriggers();
#if ONE_SIGNAL_INSTALLED
                OneSignalSDK.OneSignal.InAppMessages.ClearTriggers();
#endif
            }
        );
        
        WireButton(
            mainContent,
            "OutcomeEventsSection/SendOutcomeButton",
            () =>
            {
                dialogManager.ShowOutcomeDialog(
                    (name, value, type) =>
                    {
                        Debug.Log($"Send Outcome: {name}, value={value}, type={type}");
#if ONE_SIGNAL_INSTALLED
                        switch (type)
                        {
                            case DialogManager.OutcomeType.Normal:
                                OneSignalSDK.OneSignal.Session.AddOutcome(name);
                                break;
                            case DialogManager.OutcomeType.Unique:
                                OneSignalSDK.OneSignal.Session.AddUniqueOutcome(name);
                                break;
                            case DialogManager.OutcomeType.WithValue:
                                OneSignalSDK.OneSignal.Session.AddOutcomeWithValue(name, value);
                                break;
                        }
#endif
                    }
                );
            }
        );

        Debug.Log("Main dashboard buttons wired to dialogs");
    }

    private void WireDialogButtons()
    {
        if (dialogManager == null)
        {
            dialogManager = GetComponent<DialogManager>();
            if (dialogManager == null)
                dialogManager = FindObjectOfType<DialogManager>();
        }

        if (dialogManager == null)
        {
            Debug.LogWarning("DialogManager not found - dialog buttons not wired");
            return;
        }

        Transform dialogOverlay = transform.Find("DialogOverlay");
        if (dialogOverlay == null)
        {
            Debug.LogWarning("DialogOverlay not found");
            return;
        }

        // Cancel button
        WireButton(
            dialogOverlay,
            "DialogContainer/ButtonRow/CancelButton",
            dialogManager.HideDialog
        );

        // Scrim click to close
        Button scrimButton = dialogOverlay.Find("Scrim")?.GetComponent<Button>();
        if (scrimButton != null)
        {
            scrimButton.onClick.AddListener(dialogManager.HideDialog);
            Debug.Log("Dialog scrim wired");
        }

        Debug.Log("Dialog buttons wired successfully");
    }

    private void WirePushGridButtons()
    {
        Transform mainContent = transform.Find("MainDashboard/MainScrollView/Viewport/Content");
        if (mainContent == null)
        {
            Debug.LogWarning("Main content not found for push grid wiring");
            return;
        }

        // Push notification cards
        string[] pushTypes =
        {
            "General",
            "Greetings",
            "Promotions",
            "BreakingNews",
            "AbandonedCart",
            "NewPost",
            "Re-Engagement",
            "Rating",
        };

        Transform pushGrid = mainContent.Find("PushNotificationSection/PushGrid");
        if (pushGrid != null)
        {
            foreach (string type in pushTypes)
            {
                string cardName = type + "Card";
                Transform card = FindChildRecursive(pushGrid, cardName);
                if (card != null)
                {
                    Button btn = card.GetComponent<Button>();
                    if (btn != null)
                    {
                        string capturedType = type;
                        btn.onClick.AddListener(() => LogPushCardClicked(capturedType));
                    }
                }
            }
            Debug.Log("Push notification cards wired");
        }

        // IAM cards
        string[] iamTypes = { "TopBanner", "BottomBanner", "CenterModal", "FullScreen" };

        Transform iamGrid = mainContent.Find("InAppMessageSection/IamGrid");
        if (iamGrid != null)
        {
            foreach (string type in iamTypes)
            {
                string cardName = type + "Card";
                Transform card = FindChildRecursive(iamGrid, cardName);
                if (card != null)
                {
                    Button btn = card.GetComponent<Button>();
                    if (btn != null)
                    {
                        string capturedType = type;
                        btn.onClick.AddListener(() => LogIamCardClicked(capturedType));
                    }
                }
            }
            Debug.Log("IAM cards wired");
        }
    }

#if ONE_SIGNAL_INSTALLED
    private OneSignalExampleBehaviour behaviour;

    private void WireSdkButtons()
    {
        behaviour = FindObjectOfType<OneSignalExampleBehaviour>();

        if (behaviour == null)
        {
            Debug.LogWarning("OneSignalExampleBehaviour not found. SDK button wiring skipped.");
            return;
        }

        Transform mainContent = transform.Find("MainDashboard/MainScrollView/Viewport/Content");
        if (mainContent == null)
        {
            Debug.LogWarning("MainDashboard content not found");
            return;
        }

        // SDK-only buttons (not handled by WireMainButtons)
        WireButton(mainContent, "AppSection/RevokeConsentButton", behaviour.RevokeConsent);
        WireButton(mainContent, "UserSection/LogoutUserButton", behaviour.LogoutOneSignalUser);
        WireButton(mainContent, "LocationSection/PromptLocationButton", behaviour.PromptLocation);

        // Wire toggles
        WireToggle(
            mainContent,
            "InAppMessagingSection/IamCard/PauseIamToggleRow/PauseIamToggle",
            (value) => behaviour.TogglePauseInAppMessages()
        );

        WireToggle(
            mainContent,
            "LocationSection/LocationCard/LocationSharedToggleRow/LocationSharedToggle",
            (value) => behaviour.ToggleShareLocation()
        );

        // Wire push enabled toggle
        WireToggle(
            mainContent,
            "PushSection/PushCard/PushEnabledToggleRow/PushEnabledToggle",
            (value) =>
            {
                if (value)
                    behaviour.PushSubscriptionOptIn();
                else
                    behaviour.PushSubscriptionOptOut();
            }
        );

        Debug.Log("SDK buttons wired successfully");
    }
#endif

    private void WireButton(Transform root, string path, UnityEngine.Events.UnityAction action)
    {
        Transform target = root.Find(path);
        if (target == null)
        {
            Debug.LogWarning($"Button not found at path: {path}");
            return;
        }

        Button button = target.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(action);
            Debug.Log($"Wired button: {path}");
        }
        else
        {
            Debug.LogWarning($"No Button component on: {path}");
        }
    }

    private void WireToggle(
        Transform root,
        string path,
        UnityEngine.Events.UnityAction<bool> action
    )
    {
        Transform target = root.Find(path);
        if (target == null)
        {
            Debug.LogWarning($"Toggle not found at path: {path}");
            return;
        }

        Toggle toggle = target.GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(action);
            Debug.Log($"Wired toggle: {path}");
        }
    }

    private void LogPushCardClicked(string type)
    {
        Debug.Log($"Push Notification Card Clicked: {type}");
    }

    private void LogIamCardClicked(string type)
    {
        Debug.Log($"In-App Message Card Clicked: {type}");
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
