using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A 5-axis radar / spider chart, drawn at runtime via a single uGUI mesh.
/// Overlays two polygons — the mission requirement (amber) and the combined
/// agent skills (blue) — over a faint grid, so the player can see the skill
/// match at a glance. Axes follow the canonical order ENG, DIP, NAV, SSM, RES.
///
/// Values are on a fixed 0..10 scale (the skill range). Build one with
/// <see cref="Create"/>, then push data with <see cref="SetData"/>.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class SpiderChart : Graphic
{
    private const float MaxValue     = 10f;
    private const float RadiusFactor = 0.72f; // leaves room for axis labels
    private const float LabelPad     = 16f;

    // Colors (translucent fills, opaque-ish outlines)
    private static readonly Color GridColor   = new Color(0.45f, 0.45f, 0.60f, 0.35f);
    private static readonly Color MissionFill  = new Color(0.95f, 0.65f, 0.20f, 0.22f);
    private static readonly Color MissionLine  = new Color(1.00f, 0.72f, 0.25f, 0.95f);
    private static readonly Color TeamFill     = new Color(0.30f, 0.55f, 0.95f, 0.30f);
    private static readonly Color TeamLine     = new Color(0.48f, 0.70f, 1.00f, 0.98f);

    // Unit direction per axis: start at the top (90°), go clockwise every 72°.
    private static readonly Vector2[] Dirs = BuildDirs();

    private SkillSet _required, _combined;
    private bool     _hasRequired, _hasCombined;
    private Text[]   _axisLabels;

    // ── Factory ────────────────────────────────────────────────────────────────
    public static SpiderChart Create(Transform parent, Vector2 pos, float size)
    {
        var go = new GameObject("SpiderChart", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var chart = go.AddComponent<SpiderChart>();
        chart.raycastTarget = false;
        UIHelper.SetRect(go, pos, new Vector2(size, size));
        return chart;
    }

    // ── Public API ───────────────────────────────────────────────────────────────
    /// <summary>Sets the two overlaid polygons. Pass hasCombined=false to hide the team layer.</summary>
    public void SetData(SkillSet required, bool hasRequired, SkillSet combined, bool hasCombined)
    {
        _required    = required;
        _combined    = combined;
        _hasRequired = hasRequired;
        _hasCombined = hasCombined;
        EnsureLabels();
        SetVerticesDirty();
    }

    // ── Mesh ─────────────────────────────────────────────────────────────────────
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect  r      = GetPixelAdjustedRect();
        float radius = Mathf.Min(r.width, r.height) * 0.5f * RadiusFactor;
        if (radius <= 1f) return;

        // Grid rings + spokes
        foreach (float frac in new[] { 0.25f, 0.5f, 0.75f, 1f })
            AddLineLoop(vh, Ring(radius * frac), 1.5f, GridColor);
        for (int i = 0; i < 5; i++)
            AddSegment(vh, Vector2.zero, Dirs[i] * radius, 1.5f, GridColor);

        // Mission requirement (drawn first, under the team layer)
        if (_hasRequired)
        {
            var pts = Polygon(_required, radius);
            AddFan(vh, pts, MissionFill);
            AddLineLoop(vh, pts, 2.5f, MissionLine);
        }

        // Combined agent skills (on top)
        if (_hasCombined)
        {
            var pts = Polygon(_combined, radius);
            AddFan(vh, pts, TeamFill);
            AddLineLoop(vh, pts, 2.5f, TeamLine);
        }
    }

    // ── Geometry helpers ──────────────────────────────────────────────────────────
    private static Vector2[] BuildDirs()
    {
        var d = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            float deg = 90f - i * 72f;
            float rad = deg * Mathf.Deg2Rad;
            d[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
        return d;
    }

    private static Vector2[] Ring(float radius)
    {
        var pts = new Vector2[5];
        for (int i = 0; i < 5; i++) pts[i] = Dirs[i] * radius;
        return pts;
    }

    private static Vector2[] Polygon(SkillSet s, float radius)
    {
        float[] v = s.ToArray();
        var pts = new Vector2[5];
        for (int i = 0; i < 5; i++)
            pts[i] = Dirs[i] * radius * (Mathf.Clamp(v[i], 0f, MaxValue) / MaxValue);
        return pts;
    }

    private static void AddFan(VertexHelper vh, Vector2[] pts, Color col)
    {
        int start = vh.currentVertCount;
        vh.AddVert(Vector3.zero, col, Vector2.zero);
        for (int i = 0; i < pts.Length; i++)
            vh.AddVert(pts[i], col, Vector2.zero);
        for (int i = 0; i < pts.Length; i++)
            vh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % pts.Length);
    }

    private static void AddLineLoop(VertexHelper vh, Vector2[] pts, float thickness, Color col)
    {
        for (int i = 0; i < pts.Length; i++)
            AddSegment(vh, pts[i], pts[(i + 1) % pts.Length], thickness, col);
    }

    private static void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, float thickness, Color col)
    {
        Vector2 dir = b - a;
        float   len = dir.magnitude;
        if (len < 1e-4f) return;
        dir /= len;
        Vector2 n = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        int s = vh.currentVertCount;
        vh.AddVert(a - n, col, Vector2.zero);
        vh.AddVert(a + n, col, Vector2.zero);
        vh.AddVert(b + n, col, Vector2.zero);
        vh.AddVert(b - n, col, Vector2.zero);
        vh.AddTriangle(s, s + 1, s + 2);
        vh.AddTriangle(s, s + 2, s + 3);
    }

    // ── Axis labels ────────────────────────────────────────────────────────────────
    private void EnsureLabels()
    {
        if (_axisLabels != null) return;

        Rect  r      = rectTransform.rect;
        float radius = Mathf.Min(r.width, r.height) * 0.5f * RadiusFactor;

        _axisLabels = new Text[5];
        for (int i = 0; i < 5; i++)
        {
            Vector2 pos = Dirs[i] * (radius + LabelPad);
            _axisLabels[i] = UIHelper.Label(transform, SkillSet.SkillAbbreviations[i],
                                            12, UIHelper.ColSubtext,
                                            pos, new Vector2(48, 18));
        }
    }
}
