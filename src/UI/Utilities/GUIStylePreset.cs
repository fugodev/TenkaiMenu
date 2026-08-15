using UnityEngine;

namespace TenkaiMenu;

public static class GUIStylePreset
{
    private static GUIStyle _separator;
    private static GUIStyle _darkSeparator;
    private static GUIStyle _normalButton;
    private static GUIStyle _normalToggle;
    private static GUIStyle _tabButton;
    private static GUIStyle _tabButtonSelected;
    private static GUIStyle _tabTitle;
    private static GUIStyle _tabSubtitle;

    public static float CurrentFontScale => Mathf.Clamp(TenkaiMenu.menuFontSize.Value, 8f, 24f);

    public static int ScaleFontSize(float multiplier)
    {
        return Mathf.Max(8, Mathf.RoundToInt(CurrentFontScale * multiplier));
    }

    public static void RefreshStyles()
    {
        _separator = null;
        _darkSeparator = null;
        _normalButton = null;
        _normalToggle = null;
        _tabButton = null;
        _tabButtonSelected = null;
        _tabTitle = null;
        _tabSubtitle = null;
    }

    public static GUIStyle Separator
    {
        get
        {
            if (_separator == null)
            {
                _separator = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = Texture2D.whiteTexture },
                    margin = new RectOffset { top = 4, bottom = 4 },
                    padding = new RectOffset(),
                    border = new RectOffset()
                };
            }

            return _separator;
        }
    }

    public static GUIStyle DarkSeparator
    {
        get
        {
            if (_darkSeparator == null)
            {
                _darkSeparator = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = Texture2D.grayTexture },
                    margin = new RectOffset { top = 4, bottom = 4 },
                    padding = new RectOffset(),
                    border = new RectOffset()
                };
            }

            return _darkSeparator;
        }
    }

    public static GUIStyle NormalButton
    {
        get
        {
            if (_normalButton == null)
            {
                _normalButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = ScaleFontSize(1f)
                };
            }

            return _normalButton;
        }
    }

    public static GUIStyle NormalToggle
    {
        get
        {
            if (_normalToggle == null)
            {
                _normalToggle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = ScaleFontSize(1f)
                };
            }

            return _normalToggle;
        }
    }

    public static GUIStyle TabButton
    {
        get
        {
            if (_tabButton == null)
            {
                _tabButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = ScaleFontSize(1f),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { background = Texture2D.blackTexture, textColor = Color.white },
                    hover = { background = Texture2D.blackTexture, textColor = Color.white },
                    active = { background = Texture2D.blackTexture, textColor = Color.white },
                    focused = { background = Texture2D.blackTexture, textColor = Color.white }
                };
                _tabButton.border = new RectOffset();
                _tabButton.margin = new RectOffset();
                _tabButton.margin.left = 0;
                _tabButton.margin.right = 0;
                _tabButton.margin.top = 0;
                _tabButton.margin.bottom = 0;
                _tabButton.padding = new RectOffset();
                _tabButton.padding.left = 6;
                _tabButton.padding.right = 6;
                _tabButton.padding.top = 0;
                _tabButton.padding.bottom = 0;
            }
            return _tabButton;
        }
    }

    public static GUIStyle TabButtonSelected
    {
        get
        {
            if (_tabButtonSelected == null)
            {
                _tabButtonSelected = new GUIStyle(GUI.skin.button)
                {
                    fontSize = ScaleFontSize(1f),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { background = Texture2D.whiteTexture, textColor = Color.blue },
                    hover = { background = Texture2D.whiteTexture, textColor = Color.blue },
                    active = { background = Texture2D.whiteTexture, textColor = Color.blue },
                    focused = { background = Texture2D.whiteTexture, textColor = Color.blue }
                };
                _tabButtonSelected.border = new RectOffset();
                _tabButtonSelected.border.left = 2;
                _tabButtonSelected.border.right = 2;
                _tabButtonSelected.border.top = 2;
                _tabButtonSelected.border.bottom = 2;
                _tabButtonSelected.margin = new RectOffset();
                _tabButtonSelected.margin.left = 0;
                _tabButtonSelected.margin.right = 0;
                _tabButtonSelected.margin.top = 0;
                _tabButtonSelected.margin.bottom = 0;
                _tabButtonSelected.padding = new RectOffset();
                _tabButtonSelected.padding.left = 6;
                _tabButtonSelected.padding.right = 6;
                _tabButtonSelected.padding.top = 0;
                _tabButtonSelected.padding.bottom = 0;
            }
            return _tabButtonSelected;
        }
    }

    public static GUIStyle TabTitle
    {
        get
        {
            if (_tabTitle == null)
            {
                _tabTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = ScaleFontSize(1.35f),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                };
            }

            return _tabTitle;
        }
    }

    public static GUIStyle TabSubtitle
    {
        get
        {
            if (_tabSubtitle == null)
            {
                _tabSubtitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = ScaleFontSize(1.15f),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                };
            }

            return _tabSubtitle;
        }
    }
}
