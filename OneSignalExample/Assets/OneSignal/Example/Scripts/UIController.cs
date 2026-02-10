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

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Controls UI state for the OneSignal demo app.
/// Manages dynamic item cards for aliases, emails, SMS, tags, and triggers.
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("App Section")]
    public Text appIdText;
    
    [Header("Push Section")]
    public Text pushIdText;
    public Toggle pushEnabledToggle;
    
    [Header("List Containers")]
    public Transform aliasListContainer;
    public Transform emailListContainer;
    public Transform smsListContainer;
    public Transform tagListContainer;
    public Transform triggerListContainer;
    
    [Header("No Items Labels")]
    public GameObject noAliasesLabel;
    public GameObject noEmailsLabel;
    public GameObject noSmsLabel;
    public GameObject noTagsLabel;
    public GameObject noTriggersLabel;
    
    [Header("Toggles")]
    public Toggle pauseIamToggle;
    public Toggle locationSharedToggle;
    
    [Header("Card Styling")]
    public Color cardBackgroundColor = Color.white;
    public Color deleteButtonColor = new Color(0.937f, 0.325f, 0.314f, 1f);
    
    // Callbacks for delete actions
    public Action<string> onAliasDelete;
    public Action<string> onEmailDelete;
    public Action<string> onSmsDelete;
    public Action<string> onTagDelete;
    public Action<string> onTriggerDelete;
    
    // Track created cards
    private Dictionary<string, GameObject> aliasCards = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> emailCards = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> smsCards = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> tagCards = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> triggerCards = new Dictionary<string, GameObject>();
    
    private void Start()
    {
        // Try to find containers at runtime if not set in inspector
        FindContainersIfNeeded();
        
        // Initialize - hide no items labels if containers have children
        RefreshAllLists();
        
        // Debug: log container states
        Debug.Log($"[UIController] Start - aliasListContainer: {(aliasListContainer != null ? aliasListContainer.name : "NULL")}");
        Debug.Log($"[UIController] Start - emailListContainer: {(emailListContainer != null ? emailListContainer.name : "NULL")}");
        Debug.Log($"[UIController] Start - smsListContainer: {(smsListContainer != null ? smsListContainer.name : "NULL")}");
        Debug.Log($"[UIController] Start - tagListContainer: {(tagListContainer != null ? tagListContainer.name : "NULL")}");
        Debug.Log($"[UIController] Start - triggerListContainer: {(triggerListContainer != null ? triggerListContainer.name : "NULL")}");
    }
    
    private void FindContainersIfNeeded()
    {
        // Find the main content transform
        Transform content = transform.Find("MainDashboard/MainScrollView/Viewport/Content");
        if (content == null)
        {
            Debug.LogWarning("[UIController] Could not find main content for container lookup");
            return;
        }
        
        // Find containers if not already set
        if (aliasListContainer == null)
            aliasListContainer = FindChildRecursive(content, "AliasListContainer");
        if (emailListContainer == null)
            emailListContainer = FindChildRecursive(content, "EmailListContainer");
        if (smsListContainer == null)
            smsListContainer = FindChildRecursive(content, "SmsListContainer");
        if (tagListContainer == null)
            tagListContainer = FindChildRecursive(content, "TagListContainer");
        if (triggerListContainer == null)
            triggerListContainer = FindChildRecursive(content, "TriggerListContainer");
        
        // Find no-items labels if not set
        if (noAliasesLabel == null)
            noAliasesLabel = FindChildRecursive(content, "NoAliasesLabel")?.gameObject;
        if (noEmailsLabel == null)
            noEmailsLabel = FindChildRecursive(content, "NoEmailsLabel")?.gameObject;
        if (noSmsLabel == null)
            noSmsLabel = FindChildRecursive(content, "NoSmsLabel")?.gameObject;
        if (noTagsLabel == null)
            noTagsLabel = FindChildRecursive(content, "NoTagsLabel")?.gameObject;
        if (noTriggersLabel == null)
            noTriggersLabel = FindChildRecursive(content, "NoTriggersLabel")?.gameObject;
    }
    
    private Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent == null) return null;
        
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
    
    /// <summary>
    /// Forces layout rebuild for a container and its parents
    /// </summary>
    private void ForceLayoutRebuild(Transform container)
    {
        if (container == null) return;
        
        // Rebuild from the container up through the scroll view
        StartCoroutine(RebuildLayoutDelayed(container));
    }
    
    private IEnumerator RebuildLayoutDelayed(Transform container)
    {
        yield return null; // Wait one frame
        
        // Rebuild the container itself
        if (container != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(container as RectTransform);
        }
        
        // Walk up the hierarchy and rebuild parent layouts
        Transform current = container;
        while (current != null && current.parent != null)
        {
            current = current.parent;
            RectTransform rt = current as RectTransform;
            if (rt != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
            
            // Stop at the scroll view content
            if (current.name == "Content" || current.name == "Viewport")
                break;
        }
        
        Canvas.ForceUpdateCanvases();
    }
    
    public void UpdateAppId(string appId)
    {
        if (appIdText != null)
            appIdText.text = "App-Id: " + appId;
    }
    
    public void UpdatePushId(string pushId)
    {
        if (pushIdText != null)
            pushIdText.text = "Push-Id: " + (string.IsNullOrEmpty(pushId) ? "Not Available" : pushId);
    }
    
    public void UpdatePushEnabled(bool enabled)
    {
        if (pushEnabledToggle != null)
            pushEnabledToggle.isOn = enabled;
    }
    
    public void UpdatePauseIam(bool paused)
    {
        if (pauseIamToggle != null)
            pauseIamToggle.isOn = paused;
    }
    
    public void UpdateLocationShared(bool shared)
    {
        if (locationSharedToggle != null)
            locationSharedToggle.isOn = shared;
    }
    
    #region Alias Management
    public void AddAlias(string key, string value)
    {
        Debug.Log($"[UIController] AddAlias called: {key} = {value}");
        if (aliasListContainer == null)
        {
            Debug.LogWarning("[UIController] aliasListContainer is null!");
            return;
        }
        
        // Remove existing if updating
        if (aliasCards.ContainsKey(key))
        {
            Destroy(aliasCards[key]);
            aliasCards.Remove(key);
        }
        
        GameObject card = CreateItemCard(key, value, aliasListContainer, () => {
            RemoveAlias(key);
            onAliasDelete?.Invoke(key);
        });
        aliasCards[key] = card;
        Debug.Log($"[UIController] Alias card created: {card.name}");
        RefreshAliasLabel();
        ForceLayoutRebuild(aliasListContainer);
    }
    
    public void RemoveAlias(string key)
    {
        if (aliasCards.ContainsKey(key))
        {
            Destroy(aliasCards[key]);
            aliasCards.Remove(key);
            RefreshAliasLabel();
            ForceLayoutRebuild(aliasListContainer);
        }
    }
    
    private void RefreshAliasLabel()
    {
        if (noAliasesLabel != null)
            noAliasesLabel.SetActive(aliasCards.Count == 0);
    }
    #endregion
    
    #region Email Management
    public void AddEmail(string email)
    {
        if (emailListContainer == null) return;
        
        if (emailCards.ContainsKey(email)) return;
        
        GameObject card = CreateItemCard(email, null, emailListContainer, () => {
            RemoveEmail(email);
            onEmailDelete?.Invoke(email);
        });
        emailCards[email] = card;
        RefreshEmailLabel();
        ForceLayoutRebuild(emailListContainer);
    }
    
    public void RemoveEmail(string email)
    {
        if (emailCards.ContainsKey(email))
        {
            Destroy(emailCards[email]);
            emailCards.Remove(email);
            RefreshEmailLabel();
            ForceLayoutRebuild(emailListContainer);
        }
    }
    
    private void RefreshEmailLabel()
    {
        if (noEmailsLabel != null)
            noEmailsLabel.SetActive(emailCards.Count == 0);
    }
    #endregion
    
    #region SMS Management
    public void AddSms(string sms)
    {
        if (smsListContainer == null) return;
        
        if (smsCards.ContainsKey(sms)) return;
        
        GameObject card = CreateItemCard(sms, null, smsListContainer, () => {
            RemoveSms(sms);
            onSmsDelete?.Invoke(sms);
        });
        smsCards[sms] = card;
        RefreshSmsLabel();
        ForceLayoutRebuild(smsListContainer);
    }
    
    public void RemoveSms(string sms)
    {
        if (smsCards.ContainsKey(sms))
        {
            Destroy(smsCards[sms]);
            smsCards.Remove(sms);
            RefreshSmsLabel();
            ForceLayoutRebuild(smsListContainer);
        }
    }
    
    private void RefreshSmsLabel()
    {
        if (noSmsLabel != null)
            noSmsLabel.SetActive(smsCards.Count == 0);
    }
    #endregion
    
    #region Tag Management
    public void AddTag(string key, string value)
    {
        if (tagListContainer == null) return;
        
        // Remove existing if updating
        if (tagCards.ContainsKey(key))
        {
            Destroy(tagCards[key]);
            tagCards.Remove(key);
        }
        
        GameObject card = CreateItemCard(key, value, tagListContainer, () => {
            RemoveTag(key);
            onTagDelete?.Invoke(key);
        });
        tagCards[key] = card;
        RefreshTagLabel();
        ForceLayoutRebuild(tagListContainer);
    }
    
    public void RemoveTag(string key)
    {
        if (tagCards.ContainsKey(key))
        {
            Destroy(tagCards[key]);
            tagCards.Remove(key);
            RefreshTagLabel();
            ForceLayoutRebuild(tagListContainer);
        }
    }
    
    public void RemoveAllTags()
    {
        foreach (var card in tagCards.Values)
        {
            Destroy(card);
        }
        tagCards.Clear();
        RefreshTagLabel();
        ForceLayoutRebuild(tagListContainer);
    }
    
    private void RefreshTagLabel()
    {
        if (noTagsLabel != null)
            noTagsLabel.SetActive(tagCards.Count == 0);
    }
    #endregion
    
    #region Trigger Management
    public void AddTrigger(string key, string value)
    {
        if (triggerListContainer == null) return;
        
        // Remove existing if updating
        if (triggerCards.ContainsKey(key))
        {
            Destroy(triggerCards[key]);
            triggerCards.Remove(key);
        }
        
        GameObject card = CreateItemCard(key, value, triggerListContainer, () => {
            RemoveTrigger(key);
            onTriggerDelete?.Invoke(key);
        });
        triggerCards[key] = card;
        RefreshTriggerLabel();
        ForceLayoutRebuild(triggerListContainer);
    }
    
    public void RemoveTrigger(string key)
    {
        if (triggerCards.ContainsKey(key))
        {
            Destroy(triggerCards[key]);
            triggerCards.Remove(key);
            RefreshTriggerLabel();
            ForceLayoutRebuild(triggerListContainer);
        }
    }
    
    public void RemoveAllTriggers()
    {
        foreach (var card in triggerCards.Values)
        {
            Destroy(card);
        }
        triggerCards.Clear();
        RefreshTriggerLabel();
        ForceLayoutRebuild(triggerListContainer);
    }
    
    private void RefreshTriggerLabel()
    {
        if (noTriggersLabel != null)
            noTriggersLabel.SetActive(triggerCards.Count == 0);
    }
    #endregion
    
    private void RefreshAllLists()
    {
        RefreshAliasLabel();
        RefreshEmailLabel();
        RefreshSmsLabel();
        RefreshTagLabel();
        RefreshTriggerLabel();
    }
    
    /// <summary>
    /// Creates an item card with key/value display and delete button
    /// </summary>
    private GameObject CreateItemCard(string key, string value, Transform parent, Action onDelete)
    {
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        // Card container - use anchored positioning instead of layout groups for more control
        GameObject card = new GameObject("ItemCard_" + key);
        card.transform.SetParent(parent, false);
        
        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0, 1);
        cardRect.anchorMax = new Vector2(1, 1);
        cardRect.pivot = new Vector2(0.5f, 1);
        cardRect.sizeDelta = new Vector2(0, 80);
        
        Image cardBg = card.AddComponent<Image>();
        cardBg.color = cardBackgroundColor;
        
        LayoutElement cardLayoutElement = card.AddComponent<LayoutElement>();
        cardLayoutElement.preferredHeight = 80;
        cardLayoutElement.minHeight = 80;
        cardLayoutElement.flexibleWidth = 1;
        
        // Key text (bold) - anchored to left, stretches vertically
        GameObject keyTextObj = new GameObject("KeyText");
        keyTextObj.transform.SetParent(card.transform, false);
        
        RectTransform keyRect = keyTextObj.AddComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0, 0);
        keyRect.anchorMax = new Vector2(1, 0.6f);
        keyRect.offsetMin = new Vector2(20, 4);
        keyRect.offsetMax = new Vector2(-64, 0);
        
        Text keyTextComp = keyTextObj.AddComponent<Text>();
        keyTextComp.text = key;
        keyTextComp.font = font;
        keyTextComp.fontSize = 32;
        keyTextComp.fontStyle = FontStyle.Bold;
        keyTextComp.color = new Color(0.13f, 0.13f, 0.13f, 1f);
        keyTextComp.alignment = TextAnchor.LowerLeft;
        keyTextComp.horizontalOverflow = HorizontalWrapMode.Overflow;
        keyTextComp.verticalOverflow = VerticalWrapMode.Overflow;
        
        // Value text (if provided) - positioned below key
        if (!string.IsNullOrEmpty(value))
        {
            // Adjust key position when value exists
            keyRect.anchorMin = new Vector2(0, 0.5f);
            keyRect.anchorMax = new Vector2(1, 1);
            keyRect.offsetMin = new Vector2(20, 0);
            keyRect.offsetMax = new Vector2(-64, -4);
            keyTextComp.alignment = TextAnchor.LowerLeft;
            
            GameObject valueTextObj = new GameObject("ValueText");
            valueTextObj.transform.SetParent(card.transform, false);
            
            RectTransform valueRect = valueTextObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0, 0);
            valueRect.anchorMax = new Vector2(1, 0.5f);
            valueRect.offsetMin = new Vector2(20, 4);
            valueRect.offsetMax = new Vector2(-64, 0);
            
            Text valueTextComp = valueTextObj.AddComponent<Text>();
            valueTextComp.text = value;
            valueTextComp.font = font;
            valueTextComp.fontSize = 26;
            valueTextComp.color = new Color(0.46f, 0.46f, 0.46f, 1f);
            valueTextComp.alignment = TextAnchor.UpperLeft;
            valueTextComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            valueTextComp.verticalOverflow = VerticalWrapMode.Overflow;
        }
        else
        {
            // Center the key text when no value
            keyRect.anchorMin = new Vector2(0, 0);
            keyRect.anchorMax = new Vector2(1, 1);
            keyRect.offsetMin = new Vector2(20, 0);
            keyRect.offsetMax = new Vector2(-64, 0);
            keyTextComp.alignment = TextAnchor.MiddleLeft;
        }
        
        // Delete button - anchored to right
        GameObject deleteBtn = new GameObject("DeleteButton");
        deleteBtn.transform.SetParent(card.transform, false);
        
        RectTransform deleteBtnRect = deleteBtn.AddComponent<RectTransform>();
        deleteBtnRect.anchorMin = new Vector2(1, 0);
        deleteBtnRect.anchorMax = new Vector2(1, 1);
        deleteBtnRect.pivot = new Vector2(1, 0.5f);
        deleteBtnRect.offsetMin = new Vector2(-56, 10);
        deleteBtnRect.offsetMax = new Vector2(-12, -10);
        
        // X text for delete
        Text deleteText = deleteBtn.AddComponent<Text>();
        deleteText.text = "X";
        deleteText.font = font;
        deleteText.fontSize = 36;
        deleteText.fontStyle = FontStyle.Bold;
        deleteText.color = deleteButtonColor;
        deleteText.alignment = TextAnchor.MiddleCenter;
        
        Button deleteButton = deleteBtn.AddComponent<Button>();
        deleteButton.targetGraphic = deleteText;
        deleteButton.onClick.AddListener(() => onDelete?.Invoke());
        
        return card;
    }
}
