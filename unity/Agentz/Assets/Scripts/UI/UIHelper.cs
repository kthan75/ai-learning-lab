using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Static helpers for building uGUI elements at runtime.
/// All positions are in canvas space relative to the parent's pivot.
/// </summary>
public static class UIHelper
{
    // ── Palette ──────────────────────────────────────────────────────────────
    public static readonly Color BgDark      = new Color(0.05f, 0.05f, 0.12f);
    public static readonly Color BgCard      = new Color(0.10f, 0.10f, 0.22f);
    public static readonly Color BgPopup     = new Color(0.08f, 0.08f, 0.18f, 0.97f);
    public static readonly Color AccentBlue  = new Color(0.25f, 0.48f, 0.87f);
    public static readonly Color AccentOrange = new Color(0.85f, 0.45f, 0.13f); // frame accent
    public static readonly Color ColSuccess  = new Color(0.25f, 0.78f, 0.45f);
    public static readonly Color ColFail     = new Color(0.87f, 0.27f, 0.27f);
    public static readonly Color ColWarn     = new Color(0.90f, 0.60f, 0.15f);
    public static readonly Color ColText     = new Color(0.88f, 0.88f, 1.00f);
    public static readonly Color ColSubtext  = new Color(0.60f, 0.60f, 0.80f);
    public static readonly Color ColDisabled = new Color(0.25f, 0.25f, 0.35f);

    static Font _font;
    static Font BuiltinFont
    {
        get
        {
            if (_font == null)
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return _font;
        }
    }

    // ── Factory methods ──────────────────────────────────────────────────────

    /// <summary>Creates a RectTransform GO with an Image, anchored to center.</summary>
    public static GameObject Panel(Transform parent, string name,
                                   Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        SetRect(go, pos, size);
        go.GetComponent<Image>().color = color;
        return go;
    }

    /// <summary>Creates a Panel that stretches to fill its parent.</summary>
    public static GameObject PanelStretch(Transform parent, string name, Color color,
                                           float padL = 0, float padR = 0,
                                           float padT = 0, float padB = 0)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padL, padB);
        rt.offsetMax = new Vector2(-padR, -padT);
        go.GetComponent<Image>().color = color;
        return go;
    }

    /// <summary>Creates a UI Text label, anchored to center.</summary>
    public static Text Label(Transform parent, string text,
                              int fontSize, Color color,
                              Vector2 pos, Vector2 size,
                              TextAnchor align = TextAnchor.MiddleCenter,
                              FontStyle style  = FontStyle.Normal)
    {
        var go = new GameObject("Lbl_" + text.Substring(0, Mathf.Min(text.Length, 12)),
                                typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        SetRect(go, pos, size);
        var t = go.GetComponent<Text>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.color     = color;
        t.alignment = align;
        t.fontStyle = style;
        t.font      = BuiltinFont;
        t.resizeTextForBestFit = false;
        return t;
    }

    /// <summary>Creates a Text that stretches to fill its parent.</summary>
    public static Text LabelStretch(Transform parent, string text,
                                     int fontSize, Color color,
                                     TextAnchor align = TextAnchor.MiddleCenter,
                                     FontStyle style  = FontStyle.Normal)
    {
        var go = new GameObject("Lbl", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.GetComponent<Text>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.color     = color;
        t.alignment = align;
        t.fontStyle = style;
        t.font      = BuiltinFont;
        return t;
    }

    /// <summary>Creates a clickable Button with a text label.</summary>
    public static Button Btn(Transform parent, string label,
                              Vector2 pos, Vector2 size,
                              Color bgColor, int fontSize = 20)
    {
        var go = Panel(parent, "Btn_" + label, bgColor, pos, size);
        LabelStretch(go.transform, label, fontSize, Color.white);
        return go.AddComponent<Button>();
    }

    /// <summary>A thin horizontal bar (used for timer / skill bars).</summary>
    public static (GameObject bg, Image fill) ProgressBar(Transform parent,
                                                           Vector2 pos, Vector2 size,
                                                           Color bgColor, Color fillColor)
    {
        var bg = Panel(parent, "Bar_bg", bgColor, pos, size);
        var fillGo = PanelStretch(bg.transform, "Bar_fill", fillColor);
        // Fill anchors: left=0, bottom=0, top=1, right starts at 1 (full)
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
        var fillImg = fillGo.GetComponent<Image>();
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 1f;
        return (bg, fillImg);
    }

    /// <summary>An Image showing an agent portrait sprite (or a placeholder box if null).</summary>
    public static Image Portrait(Transform parent, Sprite sprite, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        SetRect(go, pos, size);
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        SetPortrait(img, sprite, tinted: false);
        return img;
    }

    /// <summary>Updates a portrait Image's sprite; grey-tints it when 'tinted' (e.g. deployed).</summary>
    public static void SetPortrait(Image img, Sprite sprite, bool tinted)
    {
        img.sprite        = sprite;
        img.preserveAspect = sprite != null;
        img.color = sprite != null
            ? (tinted ? new Color(0.55f, 0.55f, 0.55f) : Color.white)
            : new Color(0.14f, 0.14f, 0.20f); // placeholder box when no portrait
    }

    /// <summary>Adds a thin border (4 edge strips) around a panel. Non-interactive.</summary>
    public static void AddBorder(Transform panel, Color color, float thickness = 2f)
    {
        Edge(panel, color, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -thickness), Vector2.zero);      // top
        Edge(panel, color, new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, thickness));       // bottom
        Edge(panel, color, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(thickness, 0));       // left
        Edge(panel, color, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-thickness, 0), Vector2.zero);      // right
    }

    private static void Edge(Transform parent, Color color, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
    {
        var go = new GameObject("Border", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = oMin; rt.offsetMax = oMax;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    // ── RectTransform helpers ────────────────────────────────────────────────
    public static void SetRect(GameObject go, Vector2 anchoredPos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin       = new Vector2(0.5f, 0.5f);
        rt.anchorMax       = new Vector2(0.5f, 0.5f);
        rt.pivot           = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta       = size;
    }

    public static void AnchorTopStretch(RectTransform rt, float height, float offsetY = 0)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(0, offsetY - height);
        rt.offsetMax = new Vector2(0, offsetY);
    }

    public static void AnchorBottomStretch(RectTransform rt, float height, float offsetY = 0)
    {
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot     = new Vector2(0.5f, 0);
        rt.offsetMin = new Vector2(0, offsetY);
        rt.offsetMax = new Vector2(0, offsetY + height);
    }
}
