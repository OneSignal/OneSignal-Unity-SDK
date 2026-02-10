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

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Editor utility to build the OneSignal demo app UI matching the Android native app design.
/// Run via menu: OneSignal > Build Demo UI
/// </summary>
public class UIBuilder : EditorWindow
{
    // Design tokens
    private static readonly Color PrimaryRed = new Color(0.937f, 0.325f, 0.314f, 1f);      // #EF5350
    private static readonly Color BackgroundGray = new Color(0.961f, 0.961f, 0.961f, 1f);  // #F5F5F5
    private static readonly Color CardWhite = Color.white;
    private static readonly Color TextDark = new Color(0.129f, 0.129f, 0.129f, 1f);        // #212121
    private static readonly Color TextMuted = new Color(0.459f, 0.459f, 0.459f, 1f);       // #757575
    private static readonly Color ToggleGreen = new Color(0.298f, 0.686f, 0.314f, 1f);     // #4CAF50
    private static readonly Color ToggleGray = new Color(0.7f, 0.7f, 0.7f, 1f);            // Gray for off state
    private static readonly Color ScrimBlack = new Color(0f, 0f, 0f, 0.5f);
    
    private static Sprite roundedRectSprite;
    private static Sprite circleSprite;
    private static Sprite toggleTrackSprite;
    
    [MenuItem("OneSignal/Build Demo UI")]
    public static void BuildUI()
    {
        // Load sprites
        LoadSprites();
        
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            CreateCanvas();
            canvas = FindObjectOfType<Canvas>();
        }
        
        // Clear existing UI (except EventSystem)
        foreach (Transform child in canvas.transform)
        {
            DestroyImmediate(child.gameObject);
        }
        
        // Configure Canvas Scaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        // Build UI structure
        BuildMainDashboard(canvas.transform);
        // PushGridScreen content is now inline at the bottom of MainDashboard
        BuildDialogOverlay(canvas.transform);
        // FAB removed - all content is now on one scrollable page
        
        // Add controllers
        AddControllers(canvas.gameObject);
        
        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        
        // Rebuild all layouts
        foreach (var layoutGroup in canvas.GetComponentsInChildren<LayoutGroup>(true))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
        
        Debug.Log("OneSignal Demo UI built successfully!");
        
        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    
    private static void LoadSprites()
    {
        string spritePath = "Assets/OneSignal/Example/Sprites/";
        
        // Ensure sprites are imported correctly
        EnsureSpriteImportSettings(spritePath + "RoundedRect.png", true);
        EnsureSpriteImportSettings(spritePath + "Circle.png", false);
        EnsureSpriteImportSettings(spritePath + "ToggleTrack.png", true);
        EnsureSpriteImportSettings(spritePath + "HamburgerIcon.png", false);
        
        // Import icons folder
        string[] iconFiles = AssetDatabase.FindAssets("t:Texture2D", new[] { spritePath + "Icons" });
        foreach (string guid in iconFiles)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnsureSpriteImportSettings(path, false);
        }
        
        // Refresh assets
        AssetDatabase.Refresh();
        
        // Load sprites
        roundedRectSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "RoundedRect.png");
        circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "Circle.png");
        toggleTrackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "ToggleTrack.png");
        
        // Debug sprite loading
        if (roundedRectSprite == null)
            Debug.LogWarning("Failed to load RoundedRect.png - cards may not display correctly");
        if (circleSprite == null)
            Debug.LogWarning("Failed to load Circle.png - toggles and FAB may not display correctly");
        if (toggleTrackSprite == null)
            Debug.LogWarning("Failed to load ToggleTrack.png - toggles may not display correctly");
    }
    
    private static void EnsureSpriteImportSettings(string path, bool enableSlicing)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            
            if (enableSlicing)
            {
                // Set up 9-slice borders for rounded rect and toggle track
                importer.spriteBorder = new Vector4(20, 20, 20, 20);
            }
            
            importer.SaveAndReimport();
            Debug.Log($"Imported {path} as Sprite");
        }
    }
    
    private static void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem if needed
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
    
    private static void BuildMainDashboard(Transform parent)
    {
        // Main Dashboard container
        GameObject dashboard = CreatePanel("MainDashboard", parent, BackgroundGray);
        RectTransform dashboardRect = dashboard.GetComponent<RectTransform>();
        dashboardRect.anchorMin = Vector2.zero;
        dashboardRect.anchorMax = Vector2.one;
        dashboardRect.offsetMin = Vector2.zero;
        dashboardRect.offsetMax = Vector2.zero;
        
        // Header Bar
        GameObject header = CreatePanel("HeaderBar", dashboard.transform, PrimaryRed);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 120);
        headerRect.anchoredPosition = Vector2.zero;
        
        // Header content - Logo and Title
        GameObject headerContent = new GameObject("HeaderContent");
        headerContent.transform.SetParent(header.transform, false);
        RectTransform headerContentRect = headerContent.AddComponent<RectTransform>();
        headerContentRect.anchorMin = Vector2.zero;
        headerContentRect.anchorMax = Vector2.one;
        headerContentRect.offsetMin = new Vector2(32, 0);
        headerContentRect.offsetMax = new Vector2(-32, 0);
        HorizontalLayoutGroup headerLayout = headerContent.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childAlignment = TextAnchor.MiddleLeft;
        headerLayout.spacing = 16;
        headerLayout.childControlWidth = false;
        headerLayout.childControlHeight = false;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = false;
        
        // Logo image with tint (matching Android app style - white/light appearance on red header)
        GameObject logoImg = new GameObject("Logo");
        logoImg.transform.SetParent(headerContent.transform, false);
        Image logoImage = logoImg.AddComponent<Image>();
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OneSignal/Example/Sprites/Icons/onesignal_rectangle.png");
        if (logoSprite != null) logoImage.sprite = logoSprite;
        logoImage.preserveAspect = true;
        // Apply a light salmon/pink tint to blend with the red header (similar to Android)
        logoImage.color = new Color(1f, 0.85f, 0.85f, 1f);  // Light pink tint
        LayoutElement logoLayout = logoImg.AddComponent<LayoutElement>();
        logoLayout.preferredWidth = 200;
        logoLayout.preferredHeight = 50;
        
        // Title text
        GameObject titleGO = CreateText("TitleText", headerContent.transform, "OneSignal", 40, Color.white, FontStyle.Bold);
        LayoutElement titleLayout = titleGO.AddComponent<LayoutElement>();
        titleLayout.preferredWidth = 200;
        titleLayout.preferredHeight = 50;
        
        // Main ScrollView
        GameObject scrollView = CreateScrollView("MainScrollView", dashboard.transform);
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(0, 0);
        scrollRect.offsetMax = new Vector2(0, -120); // Below header
        
        // Content container
        Transform content = scrollView.transform.Find("Viewport/Content");
        if (content != null)
        {
            VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(32, 32, 32, 32);
            contentLayout.spacing = 24;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Build sections
            BuildAppSection(content);
            BuildUserSection(content);
            BuildAliasesSection(content);
            BuildPushSection(content);
            BuildEmailsSection(content);
            BuildSmsSection(content);
            BuildTagsSection(content);
            BuildOutcomeEventsSection(content);
            BuildInAppMessagingSection(content);
            BuildTriggersSection(content);
            BuildLocationSection(content);
            
            // Add Push/IAM grids at the bottom (inline, not separate screen)
            BuildPushNotificationGrid(content);
            BuildInAppMessageGrid(content);
        }
    }
    
    private static void BuildAppSection(Transform parent)
    {
        GameObject section = CreateSection("AppSection", parent);
        CreateSectionLabel("App", section.transform);
        
        // Info card with App-Id
        GameObject card = CreateInfoCard("AppIdCard", section.transform);
        Text appIdText = CreateText("AppIdText", card.transform, "App-Id: 77e32082-ea27-42e3-a898-c72e141824ef", 28, TextDark, FontStyle.Normal).GetComponent<Text>();
        appIdText.name = "appIdText";
        
        CreateActionButton("REVOKE CONSENT", section.transform, "RevokeConsentButton");
    }
    
    private static void BuildUserSection(Transform parent)
    {
        GameObject section = CreateSection("UserSection", parent);
        CreateActionButton("LOGIN USER", section.transform, "LoginUserButton");
        CreateActionButton("LOGOUT USER", section.transform, "LogoutUserButton");
    }
    
    private static void BuildAliasesSection(Transform parent)
    {
        GameObject section = CreateSection("AliasesSection", parent);
        CreateSectionLabel("Aliases", section.transform);
        
        GameObject card = CreateInfoCard("AliasesCard", section.transform);
        
        // List container for dynamic items
        GameObject listContainer = new GameObject("AliasListContainer");
        listContainer.transform.SetParent(card.transform, false);
        RectTransform listRect = listContainer.AddComponent<RectTransform>();
        listRect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup listLayout = listContainer.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 4;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = false;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        ContentSizeFitter listFitter = listContainer.AddComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GameObject noAliasesLabel = CreateText("NoAliasesLabel", card.transform, "No Aliases Added", 28, TextDark, FontStyle.Normal);
        noAliasesLabel.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        
        CreateActionButton("ADD ALIAS", section.transform, "AddAliasButton");
    }
    
    private static void BuildPushSection(Transform parent)
    {
        GameObject section = CreateSection("PushSection", parent);
        CreateSectionLabel("Push", section.transform);
        
        GameObject card = CreateInfoCard("PushCard", section.transform);
        VerticalLayoutGroup cardLayout = card.GetComponent<VerticalLayoutGroup>();
        if (cardLayout != null)
        {
            cardLayout.spacing = 16;
        }
        
        // Push-Id text
        CreateText("PushIdText", card.transform, "Push-Id: Not Available", 28, TextDark, FontStyle.Normal);
        
        // Enabled toggle row - use same style as other toggles
        CreateToggleRowWithSubtitle("Push Enabled:", "Enable or disable push notifications", card.transform, "PushEnabledToggle");
    }
    
    private static void BuildEmailsSection(Transform parent)
    {
        GameObject section = CreateSection("EmailsSection", parent);
        CreateSectionLabel("Emails", section.transform);
        
        GameObject card = CreateInfoCard("EmailsCard", section.transform);
        
        // List container for dynamic items
        GameObject listContainer = new GameObject("EmailListContainer");
        listContainer.transform.SetParent(card.transform, false);
        RectTransform listRect = listContainer.AddComponent<RectTransform>();
        listRect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup listLayout = listContainer.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 4;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = false;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        ContentSizeFitter listFitter = listContainer.AddComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GameObject noEmailsLabel = CreateText("NoEmailsLabel", card.transform, "No Emails Added", 28, TextDark, FontStyle.Normal);
        noEmailsLabel.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        
        CreateActionButton("ADD EMAIL", section.transform, "AddEmailButton");
    }
    
    private static void BuildSmsSection(Transform parent)
    {
        GameObject section = CreateSection("SmsSection", parent);
        CreateSectionLabel("SMSs", section.transform);
        
        GameObject card = CreateInfoCard("SmsCard", section.transform);
        
        // List container for dynamic items
        GameObject listContainer = new GameObject("SmsListContainer");
        listContainer.transform.SetParent(card.transform, false);
        RectTransform listRect = listContainer.AddComponent<RectTransform>();
        listRect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup listLayout = listContainer.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 4;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = false;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        ContentSizeFitter listFitter = listContainer.AddComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GameObject noSmsLabel = CreateText("NoSmsLabel", card.transform, "No SMSs Added", 28, TextDark, FontStyle.Normal);
        noSmsLabel.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        
        CreateActionButton("ADD SMS", section.transform, "AddSmsButton");
    }
    
    private static void BuildTagsSection(Transform parent)
    {
        GameObject section = CreateSection("TagsSection", parent);
        CreateSectionLabel("Tags", section.transform);
        
        GameObject card = CreateInfoCard("TagsCard", section.transform);
        
        // List container for dynamic items
        GameObject listContainer = new GameObject("TagListContainer");
        listContainer.transform.SetParent(card.transform, false);
        RectTransform listRect = listContainer.AddComponent<RectTransform>();
        listRect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup listLayout = listContainer.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 4;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = false;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        ContentSizeFitter listFitter = listContainer.AddComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GameObject noTagsLabel = CreateText("NoTagsLabel", card.transform, "No Tags Added", 28, TextDark, FontStyle.Normal);
        noTagsLabel.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        
        CreateActionButton("ADD TAG", section.transform, "AddTagButton");
        CreateActionButton("REMOVE ALL TAGS", section.transform, "RemoveAllTagsButton");
    }
    
    private static void BuildOutcomeEventsSection(Transform parent)
    {
        GameObject section = CreateSection("OutcomeEventsSection", parent);
        CreateSectionLabel("Outcome Events", section.transform);
        CreateActionButton("SEND OUTCOME", section.transform, "SendOutcomeButton");
    }
    
    private static void BuildInAppMessagingSection(Transform parent)
    {
        GameObject section = CreateSection("InAppMessagingSection", parent);
        CreateSectionLabel("In-App Messaging", section.transform);
        
        GameObject card = CreateInfoCard("IamCard", section.transform);
        CreateToggleRowWithSubtitle("Pause In-App Messages:", "Toggle in-app messages", card.transform, "PauseIamToggle");
    }
    
    private static void BuildTriggersSection(Transform parent)
    {
        GameObject section = CreateSection("TriggersSection", parent);
        CreateSectionLabel("Triggers", section.transform);
        
        GameObject card = CreateInfoCard("TriggersCard", section.transform);
        
        // List container for dynamic items
        GameObject listContainer = new GameObject("TriggerListContainer");
        listContainer.transform.SetParent(card.transform, false);
        RectTransform listRect = listContainer.AddComponent<RectTransform>();
        listRect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup listLayout = listContainer.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 4;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = false;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;
        ContentSizeFitter listFitter = listContainer.AddComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GameObject noTriggersLabel = CreateText("NoTriggersLabel", card.transform, "No Triggers Added", 28, TextDark, FontStyle.Normal);
        noTriggersLabel.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        
        CreateActionButton("ADD TRIGGER", section.transform, "AddTriggerButton");
        CreateActionButton("REMOVE ALL TRIGGERS", section.transform, "RemoveAllTriggersButton");
    }
    
    private static void BuildLocationSection(Transform parent)
    {
        GameObject section = CreateSection("LocationSection", parent);
        CreateSectionLabel("Location", section.transform);
        
        GameObject card = CreateInfoCard("LocationCard", section.transform);
        CreateToggleRowWithSubtitle("Location Shared:", "Location will be shared from device", card.transform, "LocationSharedToggle");
        
        CreateActionButton("PROMPT LOCATION", section.transform, "PromptLocationButton");
    }
    
    private static void BuildPushGridScreen(Transform parent)
    {
        // Push Grid Screen container
        GameObject screen = CreatePanel("PushGridScreen", parent, BackgroundGray);
        RectTransform screenRect = screen.GetComponent<RectTransform>();
        screenRect.anchorMin = Vector2.zero;
        screenRect.anchorMax = Vector2.one;
        screenRect.offsetMin = Vector2.zero;
        screenRect.offsetMax = Vector2.zero;
        screen.SetActive(false); // Initially hidden
        
        // Header Bar with back functionality
        GameObject header = CreatePanel("HeaderBar", screen.transform, PrimaryRed);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 120);
        headerRect.anchoredPosition = Vector2.zero;
        
        // Back button
        GameObject backBtn = CreateText("BackButton", header.transform, "< Back", 32, Color.white, FontStyle.Bold);
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 0);
        backRect.anchorMax = new Vector2(0.3f, 1);
        backRect.offsetMin = new Vector2(32, 0);
        backRect.offsetMax = Vector2.zero;
        Button backButton = backBtn.AddComponent<Button>();
        backButton.targetGraphic = null;
        
        // Main ScrollView
        GameObject scrollView = CreateScrollView("GridScrollView", screen.transform);
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(0, 0);
        scrollRect.offsetMax = new Vector2(0, -120);
        
        // Content container
        Transform content = scrollView.transform.Find("Viewport/Content");
        if (content != null)
        {
            VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(32, 32, 32, 32);
            contentLayout.spacing = 24;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Send Push Notification section
            BuildPushNotificationGrid(content);
            
            // Send In-App Message section
            BuildInAppMessageGrid(content);
            
            // Next Activity button
            CreateActionButton("NEXT ACTIVITY", content, "NextActivityButton");
        }
    }
    
    private static void BuildPushNotificationGrid(Transform parent)
    {
        GameObject section = CreateSection("PushNotificationSection", parent);
        CreateText("PushNotificationLabel", section.transform, "Send Push Notification", 32, TextDark, FontStyle.Bold);
        
        // Grid container using VerticalLayoutGroup with rows for responsive layout
        GameObject grid = new GameObject("PushGrid");
        grid.transform.SetParent(section.transform, false);
        RectTransform gridRect = grid.AddComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup gridVLayout = grid.AddComponent<VerticalLayoutGroup>();
        gridVLayout.spacing = 16;
        gridVLayout.childControlWidth = true;
        gridVLayout.childControlHeight = false;
        gridVLayout.childForceExpandWidth = true;
        gridVLayout.childForceExpandHeight = false;
        
        ContentSizeFitter gridFitter = grid.AddComponent<ContentSizeFitter>();
        gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Grid cards - 8 cards in 4 rows of 2
        string[] names = { "General", "Greetings", "Promotions", "Breaking News", "Abandoned Cart", "New Post", "Re-Engagement", "Rating" };
        string[] icons = { "ic_bell_white_24dp", "ic_human_greeting_white_24dp", "ic_brightness_percent_white_24dp", 
                          "ic_newspaper_white_24dp", "ic_cart_white_24dp", "ic_image_white_24dp", 
                          "ic_gesture_tap_white_24dp", "ic_star_white_24dp" };
        
        for (int i = 0; i < names.Length; i += 2)
        {
            // Create a row for each pair of cards
            GameObject row = CreateGridRow(grid.transform);
            CreateFlexibleGridCard(names[i], icons[i], row.transform);
            if (i + 1 < names.Length)
            {
                CreateFlexibleGridCard(names[i + 1], icons[i + 1], row.transform);
            }
        }
    }
    
    private static void BuildInAppMessageGrid(Transform parent)
    {
        GameObject section = CreateSection("InAppMessageSection", parent);
        CreateText("IamLabel", section.transform, "Send In-App Message", 32, TextDark, FontStyle.Bold);
        
        // Grid container using VerticalLayoutGroup with rows for responsive layout
        GameObject grid = new GameObject("IamGrid");
        grid.transform.SetParent(section.transform, false);
        RectTransform gridRect = grid.AddComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup gridVLayout = grid.AddComponent<VerticalLayoutGroup>();
        gridVLayout.spacing = 16;
        gridVLayout.childControlWidth = true;
        gridVLayout.childControlHeight = false;
        gridVLayout.childForceExpandWidth = true;
        gridVLayout.childForceExpandHeight = false;
        
        ContentSizeFitter gridFitter = grid.AddComponent<ContentSizeFitter>();
        gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Grid cards - 4 cards in 2 rows of 2
        string[] names = { "Top Banner", "Bottom Banner", "Center Modal", "Full Screen" };
        string[] icons = { "ic_phone_top_banner_white", "ic_phone_bottom_banner_white", 
                          "ic_phone_center_modal_white", "ic_phone_full_screen_white" };
        
        for (int i = 0; i < names.Length; i += 2)
        {
            // Create a row for each pair of cards
            GameObject row = CreateGridRow(grid.transform);
            CreateFlexibleGridCard(names[i], icons[i], row.transform);
            if (i + 1 < names.Length)
            {
                CreateFlexibleGridCard(names[i + 1], icons[i + 1], row.transform);
            }
        }
    }
    
    private static GameObject CreateGridRow(Transform parent)
    {
        GameObject row = new GameObject("GridRow");
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 180); // Fixed height for cards
        
        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 16;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = true;
        
        LayoutElement rowLayoutElement = row.AddComponent<LayoutElement>();
        rowLayoutElement.preferredHeight = 180;
        
        return row;
    }
    
    private static void CreateFlexibleGridCard(string cardName, string iconName, Transform parent)
    {
        // Card name without spaces for GameObject naming
        string safeName = cardName.Replace(" ", "") + "Card";
        
        GameObject card = CreatePanel(safeName, parent, PrimaryRed);
        if (roundedRectSprite != null)
        {
            card.GetComponent<Image>().sprite = roundedRectSprite;
            card.GetComponent<Image>().type = Image.Type.Sliced;
        }
        
        // Flexible width - will share space equally with sibling cards
        LayoutElement cardLayout = card.AddComponent<LayoutElement>();
        cardLayout.flexibleWidth = 1;
        
        // Button component
        Button cardButton = card.AddComponent<Button>();
        cardButton.targetGraphic = card.GetComponent<Image>();
        ColorBlock colors = cardButton.colors;
        colors.highlightedColor = new Color(0.85f, 0.25f, 0.24f, 1f);
        colors.pressedColor = new Color(0.75f, 0.2f, 0.19f, 1f);
        cardButton.colors = colors;
        
        // Vertical layout for icon + label - don't control width to prevent stretching
        VerticalLayoutGroup cardVLayout = card.AddComponent<VerticalLayoutGroup>();
        cardVLayout.padding = new RectOffset(16, 16, 24, 16);
        cardVLayout.spacing = 8;
        cardVLayout.childAlignment = TextAnchor.MiddleCenter;
        cardVLayout.childControlWidth = false;  // Don't stretch children horizontally
        cardVLayout.childControlHeight = false;
        cardVLayout.childForceExpandWidth = false;
        cardVLayout.childForceExpandHeight = false;
        
        // Icon - fixed size, centered
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(card.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(64, 64);
        
        LayoutElement iconLayout = iconGO.AddComponent<LayoutElement>();
        iconLayout.minWidth = 64;
        iconLayout.minHeight = 64;
        iconLayout.preferredWidth = 64;
        iconLayout.preferredHeight = 64;
        
        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.preserveAspect = true;  // Preserve aspect ratio
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/OneSignal/Example/Sprites/Icons/{iconName}.png");
        if (iconSprite != null)
        {
            iconImg.sprite = iconSprite;
        }
        iconImg.color = Color.white;
        
        // Label - allow text to use available width
        GameObject labelGO = CreateText("Label", card.transform, cardName, 24, Color.white, FontStyle.Bold);
        labelGO.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(200, 30); // Wide enough for text
        LayoutElement labelLayout = labelGO.AddComponent<LayoutElement>();
        labelLayout.preferredHeight = 30;
    }
    
    private static void BuildDialogOverlay(Transform parent)
    {
        // Dialog Overlay (full screen, initially hidden)
        GameObject overlay = new GameObject("DialogOverlay");
        overlay.transform.SetParent(parent, false);
        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        // Scrim
        GameObject scrim = new GameObject("Scrim");
        scrim.transform.SetParent(overlay.transform, false);
        RectTransform scrimRect = scrim.AddComponent<RectTransform>();
        scrimRect.anchorMin = Vector2.zero;
        scrimRect.anchorMax = Vector2.one;
        scrimRect.offsetMin = Vector2.zero;
        scrimRect.offsetMax = Vector2.zero;
        Image scrimImage = scrim.AddComponent<Image>();
        scrimImage.color = ScrimBlack;
        Button scrimButton = scrim.AddComponent<Button>();
        scrimButton.targetGraphic = scrimImage;
        
        // Dialog Container
        GameObject container = CreatePanel("DialogContainer", overlay.transform, CardWhite);
        if (roundedRectSprite != null)
        {
            container.GetComponent<Image>().sprite = roundedRectSprite;
            container.GetComponent<Image>().type = Image.Type.Sliced;
        }
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.3f);
        containerRect.anchorMax = new Vector2(0.9f, 0.7f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        VerticalLayoutGroup containerLayout = container.AddComponent<VerticalLayoutGroup>();
        containerLayout.padding = new RectOffset(48, 48, 48, 48);
        containerLayout.spacing = 24;
        containerLayout.childControlWidth = true;
        containerLayout.childControlHeight = false;
        containerLayout.childForceExpandWidth = true;
        containerLayout.childForceExpandHeight = false;
        
        // Title
        GameObject titleGO = CreateText("TitleText", container.transform, "Add Alias", 40, TextDark, FontStyle.Bold);
        
        // Key field container
        GameObject keyContainer = new GameObject("KeyFieldContainer");
        keyContainer.transform.SetParent(container.transform, false);
        RectTransform keyContainerRect = keyContainer.AddComponent<RectTransform>();
        keyContainerRect.sizeDelta = new Vector2(0, 100);
        LayoutElement keyContainerLayout = keyContainer.AddComponent<LayoutElement>();
        keyContainerLayout.preferredHeight = 100;
        VerticalLayoutGroup keyContainerLayoutGroup = keyContainer.AddComponent<VerticalLayoutGroup>();
        keyContainerLayoutGroup.childControlWidth = true;
        keyContainerLayoutGroup.childControlHeight = false;
        keyContainerLayoutGroup.childForceExpandWidth = true;
        keyContainerLayoutGroup.childForceExpandHeight = false;
        
        CreateText("KeyLabel", keyContainer.transform, "Key", 24, TextMuted, FontStyle.Normal);
        CreateInputField("KeyField", keyContainer.transform);
        
        // Value field container
        GameObject valueContainer = new GameObject("ValueFieldContainer");
        valueContainer.transform.SetParent(container.transform, false);
        RectTransform valueContainerRect = valueContainer.AddComponent<RectTransform>();
        valueContainerRect.sizeDelta = new Vector2(0, 100);
        LayoutElement valueContainerLayout = valueContainer.AddComponent<LayoutElement>();
        valueContainerLayout.preferredHeight = 100;
        VerticalLayoutGroup valueContainerLayoutGroup = valueContainer.AddComponent<VerticalLayoutGroup>();
        valueContainerLayoutGroup.childControlWidth = true;
        valueContainerLayoutGroup.childControlHeight = false;
        valueContainerLayoutGroup.childForceExpandWidth = true;
        valueContainerLayoutGroup.childForceExpandHeight = false;
        
        CreateText("ValueLabel", valueContainer.transform, "Value", 24, TextMuted, FontStyle.Normal);
        CreateInputField("ValueField", valueContainer.transform);
        
        // Dropdown container
        GameObject dropdownContainer = new GameObject("DropdownContainer");
        dropdownContainer.transform.SetParent(container.transform, false);
        RectTransform dropdownContainerRect = dropdownContainer.AddComponent<RectTransform>();
        dropdownContainerRect.sizeDelta = new Vector2(0, 80);
        LayoutElement dropdownContainerLayout = dropdownContainer.AddComponent<LayoutElement>();
        dropdownContainerLayout.preferredHeight = 80;
        
        CreateDropdown("OutcomeTypeDropdown", dropdownContainer.transform);
        
        // Button row
        GameObject buttonRow = new GameObject("ButtonRow");
        buttonRow.transform.SetParent(container.transform, false);
        RectTransform buttonRowRect = buttonRow.AddComponent<RectTransform>();
        buttonRowRect.sizeDelta = new Vector2(0, 60);
        LayoutElement buttonRowLayout = buttonRow.AddComponent<LayoutElement>();
        buttonRowLayout.preferredHeight = 60;
        HorizontalLayoutGroup buttonRowLayoutGroup = buttonRow.AddComponent<HorizontalLayoutGroup>();
        buttonRowLayoutGroup.childAlignment = TextAnchor.MiddleRight;
        buttonRowLayoutGroup.spacing = 32;
        buttonRowLayoutGroup.childControlWidth = true;  // Enable width control to use preferredWidth
        buttonRowLayoutGroup.childControlHeight = true;
        buttonRowLayoutGroup.childForceExpandWidth = false;
        buttonRowLayoutGroup.childForceExpandHeight = false;
        
        // Cancel button
        GameObject cancelBtn = CreateText("CancelButton", buttonRow.transform, "CANCEL", 28, PrimaryRed, FontStyle.Bold);
        // Update LayoutElement (CreateText already adds one)
        LayoutElement cancelLayout = cancelBtn.GetComponent<LayoutElement>();
        cancelLayout.preferredWidth = 120;
        cancelLayout.minWidth = 100;
        Button cancelButton = cancelBtn.AddComponent<Button>();
        cancelButton.targetGraphic = null;
        
        // Confirm button
        GameObject confirmBtn = CreateText("ConfirmButton", buttonRow.transform, "ADD", 28, PrimaryRed, FontStyle.Bold);
        // Update LayoutElement (CreateText already adds one)
        LayoutElement confirmLayout = confirmBtn.GetComponent<LayoutElement>();
        confirmLayout.preferredWidth = 80;
        confirmLayout.minWidth = 60;
        Button confirmButton = confirmBtn.AddComponent<Button>();
        confirmButton.targetGraphic = null;
        
        overlay.SetActive(false); // Initially hidden
    }
    
    private static void BuildFAB(Transform parent)
    {
        GameObject fab = new GameObject("FABButton");
        fab.transform.SetParent(parent, false);
        RectTransform fabRect = fab.AddComponent<RectTransform>();
        fabRect.anchorMin = new Vector2(0, 0);
        fabRect.anchorMax = new Vector2(0, 0);
        fabRect.pivot = new Vector2(0, 0);
        fabRect.sizeDelta = new Vector2(112, 112);
        fabRect.anchoredPosition = new Vector2(48, 48);
        
        Image fabImage = fab.AddComponent<Image>();
        fabImage.color = CardWhite;
        if (circleSprite != null)
        {
            fabImage.sprite = circleSprite;
        }
        
        Button fabButton = fab.AddComponent<Button>();
        fabButton.targetGraphic = fabImage;
        
        // Hamburger icon
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(fab.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.2f, 0.2f);
        iconRect.anchorMax = new Vector2(0.8f, 0.8f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        
        Image iconImage = iconGO.AddComponent<Image>();
        Sprite hamburgerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OneSignal/Example/Sprites/HamburgerIcon.png");
        if (hamburgerSprite != null)
        {
            iconImage.sprite = hamburgerSprite;
        }
        iconImage.color = TextDark;
    }
    
    private static void AddControllers(GameObject canvas)
    {
        // Find or add UIController
        UIController uiController = canvas.GetComponent<UIController>();
        if (uiController == null)
        {
            uiController = canvas.AddComponent<UIController>();
        }
        
        // Find or add DialogManager
        DialogManager dialogManager = canvas.GetComponent<DialogManager>();
        if (dialogManager == null)
        {
            dialogManager = canvas.AddComponent<DialogManager>();
        }
        
        // Find UI elements in main dashboard
        Transform mainDashboard = canvas.transform.Find("MainDashboard");
        if (mainDashboard != null)
        {
            Transform content = mainDashboard.Find("MainScrollView/Viewport/Content");
            if (content != null)
            {
                // App section
                uiController.appIdText = content.Find("AppSection/AppIdCard/appIdText")?.GetComponent<Text>();
                
                // Aliases section
                uiController.aliasListContainer = content.Find("AliasesSection/AliasesCard/AliasListContainer");
                uiController.noAliasesLabel = content.Find("AliasesSection/AliasesCard/NoAliasesLabel")?.gameObject;
                
                // Push section
                uiController.pushIdText = content.Find("PushSection/PushCard/PushIdText")?.GetComponent<Text>();
                uiController.pushEnabledToggle = content.Find("PushSection/PushCard/PushEnabledToggleRow/PushEnabledToggle")?.GetComponent<Toggle>();
                
                // Emails section
                uiController.emailListContainer = content.Find("EmailsSection/EmailsCard/EmailListContainer");
                uiController.noEmailsLabel = content.Find("EmailsSection/EmailsCard/NoEmailsLabel")?.gameObject;
                
                // SMS section
                uiController.smsListContainer = content.Find("SmsSection/SmsCard/SmsListContainer");
                uiController.noSmsLabel = content.Find("SmsSection/SmsCard/NoSmsLabel")?.gameObject;
                
                // Tags section
                uiController.tagListContainer = content.Find("TagsSection/TagsCard/TagListContainer");
                uiController.noTagsLabel = content.Find("TagsSection/TagsCard/NoTagsLabel")?.gameObject;
                
                // Triggers section
                uiController.triggerListContainer = content.Find("TriggersSection/TriggersCard/TriggerListContainer");
                uiController.noTriggersLabel = content.Find("TriggersSection/TriggersCard/NoTriggersLabel")?.gameObject;
                
                // Toggles
                uiController.pauseIamToggle = content.Find("InAppMessagingSection/IamCard/ToggleContainer/PauseIamToggle")?.GetComponent<Toggle>();
                uiController.locationSharedToggle = content.Find("LocationSection/LocationCard/ToggleContainer/LocationSharedToggle")?.GetComponent<Toggle>();
            }
        }
        
        // Find dialog references
        Transform dialogOverlay = canvas.transform.Find("DialogOverlay");
        if (dialogOverlay != null)
        {
            dialogManager.dialogOverlay = dialogOverlay.gameObject;
            dialogManager.dialogContainer = dialogOverlay.Find("DialogContainer")?.gameObject;
            dialogManager.titleText = dialogOverlay.Find("DialogContainer/TitleText")?.GetComponent<Text>();
            dialogManager.keyFieldContainer = dialogOverlay.Find("DialogContainer/KeyFieldContainer")?.gameObject;
            dialogManager.valueFieldContainer = dialogOverlay.Find("DialogContainer/ValueFieldContainer")?.gameObject;
            dialogManager.dropdownContainer = dialogOverlay.Find("DialogContainer/DropdownContainer")?.gameObject;
            dialogManager.keyField = dialogOverlay.Find("DialogContainer/KeyFieldContainer/KeyField")?.GetComponent<InputField>();
            dialogManager.valueField = dialogOverlay.Find("DialogContainer/ValueFieldContainer/ValueField")?.GetComponent<InputField>();
            dialogManager.keyLabel = dialogOverlay.Find("DialogContainer/KeyFieldContainer/KeyLabel")?.GetComponent<Text>();
            dialogManager.valueLabel = dialogOverlay.Find("DialogContainer/ValueFieldContainer/ValueLabel")?.GetComponent<Text>();
            dialogManager.outcomeTypeDropdown = dialogOverlay.Find("DialogContainer/DropdownContainer/OutcomeTypeDropdown")?.GetComponent<Dropdown>();
            dialogManager.cancelButton = dialogOverlay.Find("DialogContainer/ButtonRow/CancelButton")?.GetComponent<Button>();
            dialogManager.confirmButton = dialogOverlay.Find("DialogContainer/ButtonRow/ConfirmButton")?.GetComponent<Button>();
            dialogManager.confirmButtonText = dialogOverlay.Find("DialogContainer/ButtonRow/ConfirmButton")?.GetComponent<Text>();
        }
        
        // Add ButtonWiring component to canvas for runtime wiring
        ButtonWiring wiring = canvas.GetComponent<ButtonWiring>();
        if (wiring == null)
        {
            wiring = canvas.AddComponent<ButtonWiring>();
        }
        wiring.uiController = uiController;
        wiring.dialogManager = dialogManager;
    }
    
    // Helper methods
    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }
    
    private static GameObject CreateSection(string name, Transform parent)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(parent, false);
        RectTransform rect = section.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 16;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        ContentSizeFitter fitter = section.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        return section;
    }
    
    private static void CreateSectionLabel(string text, Transform parent)
    {
        GameObject label = CreateText("SectionLabel", parent, text, 28, PrimaryRed, FontStyle.Bold);
        LayoutElement layout = label.AddComponent<LayoutElement>();
        layout.preferredHeight = 40;
    }
    
    private static GameObject CreateInfoCard(string name, Transform parent)
    {
        GameObject card = CreatePanel(name, parent, CardWhite);
        if (roundedRectSprite != null)
        {
            card.GetComponent<Image>().sprite = roundedRectSprite;
            card.GetComponent<Image>().type = Image.Type.Sliced;
        }
        
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(32, 32, 32, 32);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        ContentSizeFitter fitter = card.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        LayoutElement cardLayout = card.AddComponent<LayoutElement>();
        cardLayout.minHeight = 80;
        
        return card;
    }
    
    private static void CreateActionButton(string text, Transform parent, string name = null)
    {
        GameObject button = CreatePanel(name ?? text.Replace(" ", "") + "Button", parent, PrimaryRed);
        if (roundedRectSprite != null)
        {
            button.GetComponent<Image>().sprite = roundedRectSprite;
            button.GetComponent<Image>().type = Image.Type.Sliced;
        }
        
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 96);
        
        LayoutElement layout = button.AddComponent<LayoutElement>();
        layout.preferredHeight = 96;
        layout.flexibleWidth = 1;
        
        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = button.GetComponent<Image>();
        
        // Button text
        GameObject textGO = CreateText("Text", button.transform, text, 28, Color.white, FontStyle.Bold);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textGO.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
    }
    
    private static GameObject CreateToggleRow(string labelText, Transform parent, string name)
    {
        GameObject row = new GameObject(name + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 50);
        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 50;
        
        // Label on the left (0-70%)
        GameObject label = CreateText("Label", row.transform, labelText, 28, TextDark, FontStyle.Bold);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.7f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        
        // Toggle on the right - fixed size, anchored right
        GameObject toggleContainer = new GameObject(name);
        toggleContainer.transform.SetParent(row.transform, false);
        RectTransform containerRect = toggleContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 0.5f);
        containerRect.anchorMax = new Vector2(1, 0.5f);
        containerRect.pivot = new Vector2(1, 0.5f);
        containerRect.sizeDelta = new Vector2(80, 40); // Fixed size toggle
        containerRect.anchoredPosition = new Vector2(-8, 0); // 8px from right edge
        
        // Toggle background (track) - gray when off
        GameObject trackGO = new GameObject("Background");
        trackGO.transform.SetParent(toggleContainer.transform, false);
        RectTransform trackRect = trackGO.AddComponent<RectTransform>();
        trackRect.anchorMin = Vector2.zero;
        trackRect.anchorMax = Vector2.one;
        trackRect.offsetMin = Vector2.zero;
        trackRect.offsetMax = Vector2.zero;
        Image trackImg = trackGO.AddComponent<Image>();
        trackImg.color = ToggleGray; // Start gray (off state)
        if (toggleTrackSprite != null)
        {
            trackImg.sprite = toggleTrackSprite;
            trackImg.type = Image.Type.Sliced;
        }
        
        // Toggle thumb (circle) - fixed size, centered vertically
        GameObject thumbGO = new GameObject("Checkmark");
        thumbGO.transform.SetParent(toggleContainer.transform, false);
        RectTransform thumbRect = thumbGO.AddComponent<RectTransform>();
        thumbRect.anchorMin = new Vector2(0, 0.5f);
        thumbRect.anchorMax = new Vector2(0, 0.5f);
        thumbRect.pivot = new Vector2(0, 0.5f);
        thumbRect.sizeDelta = new Vector2(32, 32); // Fixed circle size
        thumbRect.anchoredPosition = new Vector2(4, 0); // Left position (off state)
        Image thumbImg = thumbGO.AddComponent<Image>();
        thumbImg.color = Color.white;
        if (circleSprite != null) thumbImg.sprite = circleSprite;
        
        // Add Toggle component (no graphic set - ToggleColorHandler manages visuals)
        Toggle toggle = toggleContainer.AddComponent<Toggle>();
        toggle.targetGraphic = trackImg;
        toggle.isOn = false; // Start off
        
        // Set up color block for track color transition (gray->green)
        ColorBlock colors = toggle.colors;
        colors.normalColor = ToggleGray;
        colors.highlightedColor = ToggleGray;
        colors.pressedColor = ToggleGray;
        colors.selectedColor = ToggleGreen;
        toggle.colors = colors;
        
        // Add ToggleColorHandler script reference for runtime color change
        toggleContainer.AddComponent<ToggleColorHandler>();
        
        return row;
    }
    
    private static GameObject CreateToggleRowWithSubtitle(string labelText, string subtitleText, Transform parent, string name)
    {
        GameObject row = new GameObject(name + "Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 80);
        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 80;
        
        // Label container on the left (leaving room for toggle on right)
        GameObject labelContainer = new GameObject("LabelContainer");
        labelContainer.transform.SetParent(row.transform, false);
        RectTransform labelContainerRect = labelContainer.AddComponent<RectTransform>();
        labelContainerRect.anchorMin = new Vector2(0, 0);
        labelContainerRect.anchorMax = new Vector2(1, 1);
        labelContainerRect.offsetMin = Vector2.zero;
        labelContainerRect.offsetMax = new Vector2(-100, 0); // Leave 100px for toggle
        
        VerticalLayoutGroup labelLayout = labelContainer.AddComponent<VerticalLayoutGroup>();
        labelLayout.childAlignment = TextAnchor.MiddleLeft;
        labelLayout.childControlWidth = true;
        labelLayout.childControlHeight = false;
        labelLayout.childForceExpandWidth = true;
        labelLayout.childForceExpandHeight = false;
        labelLayout.spacing = 2;
        labelLayout.padding = new RectOffset(0, 0, 10, 10);
        
        GameObject titleGO = CreateText("Label", labelContainer.transform, labelText, 26, TextDark, FontStyle.Bold);
        titleGO.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        
        GameObject subtitleGO = CreateText("Subtitle", labelContainer.transform, subtitleText, 20, TextMuted, FontStyle.Normal);
        subtitleGO.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        
        // Toggle on the right - fixed size, anchored right
        GameObject toggleContainer = new GameObject(name);
        toggleContainer.transform.SetParent(row.transform, false);
        RectTransform containerRect = toggleContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 0.5f);
        containerRect.anchorMax = new Vector2(1, 0.5f);
        containerRect.pivot = new Vector2(1, 0.5f);
        containerRect.sizeDelta = new Vector2(80, 40); // Fixed size toggle
        containerRect.anchoredPosition = new Vector2(-8, 0); // 8px from right edge
        
        // Toggle background (track) - gray when off
        GameObject trackGO = new GameObject("Background");
        trackGO.transform.SetParent(toggleContainer.transform, false);
        RectTransform trackRect = trackGO.AddComponent<RectTransform>();
        trackRect.anchorMin = Vector2.zero;
        trackRect.anchorMax = Vector2.one;
        trackRect.offsetMin = Vector2.zero;
        trackRect.offsetMax = Vector2.zero;
        Image trackImg = trackGO.AddComponent<Image>();
        trackImg.color = ToggleGray; // Start gray (off state)
        if (toggleTrackSprite != null)
        {
            trackImg.sprite = toggleTrackSprite;
            trackImg.type = Image.Type.Sliced;
        }
        
        // Toggle thumb (circle) - fixed size, centered vertically
        GameObject thumbGO = new GameObject("Checkmark");
        thumbGO.transform.SetParent(toggleContainer.transform, false);
        RectTransform thumbRect = thumbGO.AddComponent<RectTransform>();
        thumbRect.anchorMin = new Vector2(0, 0.5f);
        thumbRect.anchorMax = new Vector2(0, 0.5f);
        thumbRect.pivot = new Vector2(0, 0.5f);
        thumbRect.sizeDelta = new Vector2(32, 32); // Fixed circle size
        thumbRect.anchoredPosition = new Vector2(4, 0); // Left position (off state)
        Image thumbImg = thumbGO.AddComponent<Image>();
        thumbImg.color = Color.white;
        if (circleSprite != null) thumbImg.sprite = circleSprite;
        
        // Add Toggle component (no graphic set - ToggleColorHandler manages visuals)
        Toggle toggle = toggleContainer.AddComponent<Toggle>();
        toggle.targetGraphic = trackImg;
        toggle.isOn = false; // Start off
        
        // Add ToggleColorHandler for runtime color/position changes
        toggleContainer.AddComponent<ToggleColorHandler>();
        
        return row;
    }
    
    private static GameObject CreateText(string name, Transform parent, string text, int fontSize, Color color, FontStyle style)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 10);
        
        Text textComponent = textGO.AddComponent<Text>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.fontStyle = style;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        
        LayoutElement layout = textGO.AddComponent<LayoutElement>();
        layout.preferredHeight = fontSize + 10;
        
        return textGO;
    }
    
    private static GameObject CreateScrollView(string name, Transform parent)
    {
        GameObject scrollView = new GameObject(name);
        scrollView.transform.SetParent(parent, false);
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;
        scroll.scrollSensitivity = 1f;
        
        Image scrollImage = scrollView.AddComponent<Image>();
        scrollImage.color = new Color(0, 0, 0, 0); // Transparent
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportRect.pivot = new Vector2(0, 1);
        
        // Use RectMask2D for better performance
        viewport.AddComponent<RectMask2D>();
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 300); // Initial height, will be resized by ContentSizeFitter
        
        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        
        return scrollView;
    }
    
    private static void CreateGridCard(string labelText, string iconName, Transform parent)
    {
        GameObject card = CreatePanel(labelText.Replace(" ", "") + "Card", parent, PrimaryRed);
        if (roundedRectSprite != null)
        {
            card.GetComponent<Image>().sprite = roundedRectSprite;
            card.GetComponent<Image>().type = Image.Type.Sliced;
        }
        
        Button btn = card.AddComponent<Button>();
        btn.targetGraphic = card.GetComponent<Image>();
        
        VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 32, 16);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        
        // Icon
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(card.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(96, 96);
        LayoutElement iconLayout = iconGO.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = 96;
        iconLayout.preferredHeight = 96;
        
        Image iconImage = iconGO.AddComponent<Image>();
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OneSignal/Example/Sprites/Icons/" + iconName + ".png");
        if (iconSprite != null)
        {
            iconImage.sprite = iconSprite;
        }
        iconImage.color = Color.white;
        
        // Label
        GameObject labelGO = CreateText("Label", card.transform, labelText, 28, Color.white, FontStyle.Bold);
        labelGO.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        LayoutElement labelLayout = labelGO.GetComponent<LayoutElement>();
        labelLayout.preferredWidth = 200;
    }
    
    private static void CreateInputField(string name, Transform parent)
    {
        GameObject inputFieldGO = new GameObject(name);
        inputFieldGO.transform.SetParent(parent, false);
        RectTransform inputRect = inputFieldGO.AddComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(0, 60);
        
        LayoutElement inputLayout = inputFieldGO.AddComponent<LayoutElement>();
        inputLayout.preferredHeight = 60;
        inputLayout.flexibleWidth = 1;
        
        Image inputImage = inputFieldGO.AddComponent<Image>();
        inputImage.color = new Color(0.95f, 0.95f, 0.95f);
        
        InputField inputField = inputFieldGO.AddComponent<InputField>();
        
        // Text component
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputFieldGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(16, 0);
        textRect.offsetMax = new Vector2(-16, 0);
        
        Text textComponent = textGO.AddComponent<Text>();
        textComponent.fontSize = 32;
        textComponent.color = TextDark;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.alignment = TextAnchor.MiddleLeft;
        textComponent.supportRichText = false;
        
        // Placeholder
        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(inputFieldGO.transform, false);
        RectTransform placeholderRect = placeholderGO.AddComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0, 0);
        placeholderRect.anchorMax = new Vector2(1, 1);
        placeholderRect.offsetMin = new Vector2(16, 0);
        placeholderRect.offsetMax = new Vector2(-16, 0);
        
        Text placeholderComponent = placeholderGO.AddComponent<Text>();
        placeholderComponent.text = "Enter text...";
        placeholderComponent.fontSize = 32;
        placeholderComponent.color = TextMuted;
        placeholderComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        placeholderComponent.alignment = TextAnchor.MiddleLeft;
        placeholderComponent.fontStyle = FontStyle.Italic;
        
        inputField.textComponent = textComponent;
        inputField.placeholder = placeholderComponent;
        inputField.targetGraphic = inputImage;
    }
    
    private static void CreateDropdown(string name, Transform parent)
    {
        GameObject dropdownGO = new GameObject(name);
        dropdownGO.transform.SetParent(parent, false);
        RectTransform dropdownRect = dropdownGO.AddComponent<RectTransform>();
        dropdownRect.anchorMin = Vector2.zero;
        dropdownRect.anchorMax = Vector2.one;
        dropdownRect.offsetMin = Vector2.zero;
        dropdownRect.offsetMax = Vector2.zero;
        
        Image dropdownImage = dropdownGO.AddComponent<Image>();
        dropdownImage.color = new Color(0.95f, 0.95f, 0.95f);
        
        Dropdown dropdown = dropdownGO.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownImage;
        
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(16, 0);
        labelRect.offsetMax = new Vector2(-48, 0);
        
        Text labelText = labelGO.AddComponent<Text>();
        labelText.text = "Select an Outcome Type...";
        labelText.fontSize = 28;
        labelText.color = TextDark;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.alignment = TextAnchor.MiddleLeft;
        
        dropdown.captionText = labelText;
        
        // Arrow
        GameObject arrowGO = new GameObject("Arrow");
        arrowGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform arrowRect = arrowGO.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.pivot = new Vector2(1, 0.5f);
        arrowRect.sizeDelta = new Vector2(40, 40);
        arrowRect.anchoredPosition = new Vector2(-8, 0);
        
        Image arrowImage = arrowGO.AddComponent<Image>();
        Sprite chevronSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OneSignal/Example/Sprites/Icons/ic_chevron_down_white_48dp.png");
        if (chevronSprite != null)
        {
            arrowImage.sprite = chevronSprite;
        }
        arrowImage.color = TextDark;
        
        // Template (simplified)
        GameObject templateGO = new GameObject("Template");
        templateGO.transform.SetParent(dropdownGO.transform, false);
        RectTransform templateRect = templateGO.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.sizeDelta = new Vector2(0, 300);
        
        Image templateImage = templateGO.AddComponent<Image>();
        templateImage.color = CardWhite;
        
        ScrollRect templateScroll = templateGO.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(templateGO.transform, false);
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewportGO.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0);
        Mask viewportMask = viewportGO.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // Content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        
        // Item
        GameObject itemGO = new GameObject("Item");
        itemGO.transform.SetParent(contentGO.transform, false);
        RectTransform itemRect = itemGO.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 0.5f);
        itemRect.anchorMax = new Vector2(1, 0.5f);
        itemRect.sizeDelta = new Vector2(0, 60);
        
        Toggle itemToggle = itemGO.AddComponent<Toggle>();
        
        // Item background
        GameObject itemBgGO = new GameObject("Item Background");
        itemBgGO.transform.SetParent(itemGO.transform, false);
        RectTransform itemBgRect = itemBgGO.AddComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.offsetMin = Vector2.zero;
        itemBgRect.offsetMax = Vector2.zero;
        
        Image itemBgImage = itemBgGO.AddComponent<Image>();
        itemBgImage.color = new Color(0.9f, 0.9f, 0.9f);
        
        itemToggle.targetGraphic = itemBgImage;
        
        // Item label
        GameObject itemLabelGO = new GameObject("Item Label");
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        RectTransform itemLabelRect = itemLabelGO.AddComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(16, 0);
        itemLabelRect.offsetMax = new Vector2(-16, 0);
        
        Text itemLabelText = itemLabelGO.AddComponent<Text>();
        itemLabelText.text = "Option";
        itemLabelText.fontSize = 28;
        itemLabelText.color = TextDark;
        itemLabelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        itemLabelText.alignment = TextAnchor.MiddleLeft;
        
        templateScroll.viewport = viewportRect;
        templateScroll.content = contentRect;
        
        dropdown.template = templateRect;
        dropdown.itemText = itemLabelText;
        
        templateGO.SetActive(false);
    }
}
#endif
