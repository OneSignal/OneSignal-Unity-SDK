using OneSignalSDK;
using OneSignalSDK.Debug.Models;
using UnityEngine;

public sealed class NoLocationDemo : MonoBehaviour
{
    [SerializeField]
    private string _oneSignalAppId = "YOUR-ONESIGNAL-APP-ID";

    private const float ReferenceWidth = 393f;
    private static readonly Color _backgroundColor = new Color32(248, 249, 250, 255);
    private static readonly Color _primaryColor = new Color32(229, 75, 77, 255);
    private static readonly Color _valueColor = new Color32(47, 52, 55, 255);
    private static readonly Color _mutedTextColor = new Color32(97, 97, 97, 255);
    private static readonly Color _labelColor = new Color32(107, 114, 128, 255);
    private static readonly Color _dividerColor = new Color32(232, 234, 237, 255);
    private static readonly Color _warningColor = new Color32(201, 37, 45, 255);

    private GUIStyle _bodyStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _cardStyle;
    private GUIStyle _headerSubtitleStyle;
    private GUIStyle _headerTitleStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _outlineButtonStyle;
    private GUIStyle _sectionHeaderStyle;
    private GUIStyle _valueStyle;
    private GUIStyle _warningValueStyle;
    private Texture2D _cardTexture;
    private Texture2D _dividerTexture;
    private Texture2D _primaryTexture;
    private Texture2D _whiteTexture;
    private Texture2D _backgroundTexture;
    private Vector2 _scrollPosition;

    private string _locationStatus = "Location test not run";
    private bool _initialized;

    private void Start()
    {
        OneSignal.Debug.LogLevel = LogLevel.Verbose;

        if (!IsConfigured)
            return;

        OneSignal.Initialize(_oneSignalAppId);
        _initialized = true;
    }

    private void OnGUI()
    {
        EnsureStyles();

        var scale = Mathf.Max(1f, Screen.width / ReferenceWidth);
        var logicalWidth = Screen.width / scale;
        var logicalHeight = Screen.height / scale;
        var safeArea = ScaleRect(Screen.safeArea, 1f / scale);
        var topInset = Mathf.Max(0f, logicalHeight - safeArea.yMax);
        var bottomInset = Mathf.Max(0f, safeArea.y);

        var previousMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        GUI.DrawTexture(new Rect(0, 0, logicalWidth, logicalHeight), _backgroundTexture);

        var headerHeight = topInset + 72f;
        GUI.DrawTexture(new Rect(0, 0, logicalWidth, headerHeight), _primaryTexture);
        GUI.Label(new Rect(16f, topInset + 12f, logicalWidth - 32f, 28f), "OneSignal", _headerTitleStyle);
        GUI.Label(
            new Rect(16f, topInset + 42f, logicalWidth - 32f, 20f),
            "No-Location Demo",
            _headerSubtitleStyle
        );

        var scrollRect = new Rect(0, headerHeight, logicalWidth, logicalHeight - headerHeight);
        var contentWidth = logicalWidth - 32f;
        var contentHeight = 560f + bottomInset;

        _scrollPosition = GUI.BeginScrollView(
            scrollRect,
            _scrollPosition,
            new Rect(0, 0, logicalWidth, contentHeight),
            false,
            false
        );

        var y = 16f;
        y = DrawAppSection(16f, y, contentWidth);
        y = DrawPushSection(16f, y, contentWidth);
        DrawLocationSection(16f, y, contentWidth);

        GUI.EndScrollView();
        GUI.matrix = previousMatrix;
    }

    private float DrawAppSection(float x, float y, float width)
    {
        DrawSectionHeader(x, y, "APP");
        y += 24f;

        var cardHeight = 52f;
        GUI.Box(new Rect(x, y, width, cardHeight), GUIContent.none, _cardStyle);
        DrawKeyValueRow(
            x + 12f,
            y + 14f,
            width - 24f,
            "App ID",
            _oneSignalAppId,
            IsConfigured ? _valueStyle : _warningValueStyle
        );

        return y + cardHeight + 24f;
    }

    private float DrawPushSection(float x, float y, float width)
    {
        DrawSectionHeader(x, y, "PUSH");
        y += 24f;

        var cardHeight = 164f;
        GUI.Box(new Rect(x, y, width, cardHeight), GUIContent.none, _cardStyle);

        var contentX = x + 12f;
        var contentWidth = width - 24f;
        var permission = _initialized
            ? OneSignal.Notifications.Permission
                ? "Granted"
                : "Not granted"
            : "Not initialized";
        var pushId = _initialized ? FormatValue(OneSignal.User.PushSubscription.Id) : "-";

        DrawKeyValueRow(
            contentX,
            y + 12f,
            contentWidth,
            "Permission",
            permission,
            _valueStyle
        );
        DrawDivider(contentX, y + 45f, contentWidth);
        DrawKeyValueRow(contentX, y + 58f, contentWidth, "Push ID", pushId, _valueStyle);

        if (GUI.Button(new Rect(contentX, y + 104f, contentWidth, 48f), "REQUEST PERMISSION", _buttonStyle))
            RequestPushPermission();

        return y + cardHeight + 24f;
    }

    private void DrawLocationSection(float x, float y, float width)
    {
        DrawSectionHeader(x, y, "LOCATION MODULE");
        y += 24f;

        var cardHeight = 188f;
        GUI.Box(new Rect(x, y, width, cardHeight), GUIContent.none, _cardStyle);

        var contentX = x + 12f;
        var contentWidth = width - 24f;
        GUI.Label(
            new Rect(contentX, y + 12f, contentWidth, 74f),
            "This demo initializes OneSignal and requests notification permission only when you tap the button above. Native builds exclude the location module.",
            _bodyStyle
        );

        GUI.Label(new Rect(contentX, y + 94f, contentWidth, 28f), _locationStatus, _bodyStyle);

        if (
            GUI.Button(
                new Rect(contentX, y + 128f, contentWidth, 48f),
                "TEST LOCATION REQUEST",
                _outlineButtonStyle
            )
        )
            TestLocationRequest();
    }

    private void DrawSectionHeader(float x, float y, string text) =>
        GUI.Label(new Rect(x + 4f, y, 240f, 18f), text, _sectionHeaderStyle);

    private void DrawKeyValueRow(
        float x,
        float y,
        float width,
        string label,
        string value,
        GUIStyle valueStyle
    )
    {
        GUI.Label(new Rect(x, y, 92f, 22f), label, _labelStyle);
        GUI.Label(new Rect(x + 100f, y, width - 100f, 44f), value, valueStyle);
    }

    private void DrawDivider(float x, float y, float width) =>
        GUI.DrawTexture(new Rect(x, y, width, 1f), _dividerTexture);

    private void EnsureStyles()
    {
        if (_bodyStyle != null)
            return;

        _cardTexture = MakeTexture(new Color32(255, 255, 255, 255));
        _dividerTexture = MakeTexture(_dividerColor);
        _primaryTexture = MakeTexture(_primaryColor);
        _whiteTexture = MakeTexture(new Color32(255, 255, 255, 255));
        _backgroundTexture = MakeTexture(_backgroundColor);

        _headerTitleStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = Color.white },
            fontSize = 22,
            fontStyle = FontStyle.Bold,
        };
        _headerSubtitleStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = Color.white },
            fontSize = 14,
        };
        _sectionHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = _mutedTextColor },
            fontSize = 12,
            fontStyle = FontStyle.Bold,
        };
        _cardStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = _cardTexture },
            border = new RectOffset(12, 12, 12, 12),
            padding = new RectOffset(0, 0, 0, 0),
        };
        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = _labelColor },
            fontSize = 14,
            fontStyle = FontStyle.Bold,
        };
        _valueStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = _valueColor },
            fontSize = 13,
            alignment = TextAnchor.UpperRight,
            wordWrap = true,
        };
        _warningValueStyle = new GUIStyle(_valueStyle)
        {
            normal = { textColor = _warningColor },
        };
        _bodyStyle = new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = _mutedTextColor },
            fontSize = 14,
            wordWrap = true,
        };
        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = _primaryTexture, textColor = Color.white },
            active = { background = _primaryTexture, textColor = Color.white },
            hover = { background = _primaryTexture, textColor = Color.white },
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
        _outlineButtonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = _whiteTexture, textColor = _primaryColor },
            active = { background = _whiteTexture, textColor = _primaryColor },
            hover = { background = _whiteTexture, textColor = _primaryColor },
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
    }

    private async void RequestPushPermission()
    {
        if (!_initialized)
        {
            return;
        }

        try
        {
            await OneSignal.Notifications.RequestPermissionAsync(false);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[NoLocationDemo] Push permission request failed: {exception.Message}");
        }
    }

    private void TestLocationRequest()
    {
        if (!_initialized)
        {
            _locationStatus = "Initialize OneSignal before testing location.";
            return;
        }

        try
        {
            OneSignal.Location.RequestPermission();
            _locationStatus = "Location request completed without linking the location module.";
        }
        catch (System.Exception exception)
        {
            _locationStatus = $"Location request failed: {exception.Message}";
        }
    }

    private bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_oneSignalAppId)
        && !_oneSignalAppId.StartsWith("YOUR-");

    private static string FormatValue(string value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static Rect ScaleRect(Rect rect, float scale) =>
        new Rect(rect.x * scale, rect.y * scale, rect.width * scale, rect.height * scale);

    private static Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
