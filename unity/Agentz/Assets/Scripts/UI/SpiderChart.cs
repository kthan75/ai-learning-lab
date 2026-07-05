using UnityEngine;
using UnityEngine.UI;

/// <summary>How axis labels are rendered on a <see cref="SpiderChart"/>.</summary>
public enum ChartLabelMode { None, Names, NamesAndValues }

/// <summary>
/// A 5-axis radar / spider chart, drawn at runtime via a single uGUI mesh.
/// Two uses:
///   • Dual series (<see cref="SetData"/>) — mission requirement (amber) overlaid
///     with combined agent skills (blue); used in the assign / result popups.
///   • Single series (<see cref="SetSingle"/>) — one agent's own skills; used as a
///     mini chart on each agent card, dimmed when the agent is deployed.
/// Axes follow the canonical order ENG, DIP, NAV, SSM, RES on a 0..10 scale.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class SpiderChart : Graphic
{
    private const float MaxValue     = 10f;
    private const float RadiusFactor = 0.70f; // leaves room for axis labels
    private const float LabelPad     = 14f;
    private const float LabelXStretch = 1.10f; // push near-horizontal labels (DIP/RES) further out

    // Grid + dual-series colors
    private static readonly Color GridColor   = new Color(0.45f, 0.45f, 0.60f, 0.35f);
    private static readonly Color MissionFill = new Color(0.95f, 0.65f, 0.20f, 0.22f);
    private static readonly Color MissionLine = new Color(1.00f, 0.72f, 0.25f, 0.95f);
    private static readonly Color TeamFill    = new Color(0.30f, 0.55f, 0.95f, 0.30f);
    private static readonly Color TeamLine    = new Color(0.48f, 0.70f, 1.00f, 0.98f);

    // Single-series (agent mini chart) colors — cyan, distinct from amber/blue above
    private static readonly Color AgentFill    = new Color(0.28f, 0.72f, 0.78f, 0.24f);
    private static readonly Color AgentLine    = new Color(0.42f, 0.86f, 0.92f, 0.96f);
    private static readonly Color AgentFillDim = new Color(0.40f, 0.40f, 0.46f, 0.14f);
    private static readonly Color AgentLineDim = new Color(0.48f, 0.48f, 0.55f, 0.55f);

    // Result highlight colors (green = covered overlap, red = uncovered requirement)
    private static readonly Color MissionFillFaint = new Color(0.95f, 0.65f, 0.20f, 0.10f);
    private static readonly Color TeamFillFaint    = new Color(0.30f, 0.55f, 0.95f, 0.12f);
    private static readonly Color SuccessFill       = new Color(0.30f, 0.85f, 0.45f, 0.45f);
    private static readonly Color FailFill          = new Color(0.90f, 0.25f, 0.25f, 0.45f);

    // Value-label token colors (rich text)
    private const string HexValue    = "E8C74A"; // amber
    private const string HexName     = "9999CC"; // grey-blue
    private const string HexValueDim = "666680";
    private const string HexNameDim  = "555566";

    private static readonly Vector2[] Dirs = BuildDirs();

    // Config
    private ChartLabelMode _labelMode     = ChartLabelMode.Names;
    private int            _labelFontSize = 12;

    // Data
    private SkillSet _required, _combined;
    private bool     _hasRequired, _hasCombined;
    private bool     _singleColors;
    private Color    _singleFill, _singleLine;
    private bool     _resultMode;
    private bool     _resultSuccess;

    // Labels
    private Text[]   _axisLabels;
    private bool     _labelsBuilt;
    private SkillSet _labelValues;
    private bool     _dim;

    // ── Factory ────────────────────────────────────────────────────────────────
    public static SpiderChart Create(Transform parent, Vector2 pos, float size,
                                     ChartLabelMode labelMode = ChartLabelMode.Names,
                                     int labelFontSize = 12)
    {
        var go = new GameObject("SpiderChart", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var chart = go.AddComponent<SpiderChart>();
        chart.raycastTarget  = false;
        chart._labelMode     = labelMode;
        chart._labelFontSize = labelFontSize;
        UIHelper.SetRect(go, pos, new Vector2(size, size));
        return chart;
    }

    // ── Public API ───────────────────────────────────────────────────────────────
    /// <summary>Dual series: mission requirement + combined team. Pass hasCombined=false to hide the team layer.</summary>
    public void SetData(SkillSet required, bool hasRequired, SkillSet combined, bool hasCombined)
    {
        _required     = required;
        _combined     = combined;
        _hasRequired  = hasRequired;
        _hasCombined  = hasCombined;
        _singleColors = false;
        _resultMode   = false;
        _labelValues  = combined;
        _dim          = false;
        EnsureLabels();
        SetVerticesDirty();
    }

    /// <summary>Single series: one agent's own skills (dimmed when the agent is unavailable).</summary>
    public void SetSingle(SkillSet skills, bool dimmed)
    {
        _hasRequired  = false;
        _hasCombined  = true;
        _combined     = skills;
        _singleColors = true;
        _resultMode   = false;
        _singleFill   = dimmed ? AgentFillDim : AgentFill;
        _singleLine   = dimmed ? AgentLineDim : AgentLine;
        _labelValues  = skills;
        _dim          = dimmed;
        EnsureLabels();
        SetVerticesDirty();
    }

    /// <summary>
    /// Result view: requirement (amber) vs team (blue), plus a highlight showing the
    /// outcome — the covered overlap filled green on success, or the uncovered part of
    /// the requirement filled red on failure.
    /// </summary>
    public void SetResult(SkillSet required, SkillSet team, bool success)
    {
        _required      = required;
        _combined      = team;
        _hasRequired   = true;
        _hasCombined   = true;
        _singleColors  = false;
        _resultMode    = true;
        _resultSuccess = success;
        _labelValues   = team;
        _dim           = false;
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

        foreach (float frac in new[] { 0.25f, 0.5f, 0.75f, 1f })
            AddLineLoop(vh, Ring(radius * frac), 1.2f, GridColor);
        for (int i = 0; i < 5; i++)
            AddSegment(vh, Vector2.zero, Dirs[i] * radius, 1.2f, GridColor);

        if (_resultMode)
        {
            var reqPts     = Polygon(_required, radius);
            var teamPts    = Polygon(_combined, radius);
            var overlapPts = OverlapPolygon(_required, _combined, radius);

            // Faint context fills so both shapes stay readable under the highlight
            AddFan(vh, reqPts,  MissionFillFaint);
            AddFan(vh, teamPts, TeamFillFaint);

            // Outcome highlight — where the roll "landed"
            if (_resultSuccess)
                AddFan(vh, overlapPts, SuccessFill);              // covered overlap, green
            else
                AddBand(vh, overlapPts, reqPts, FailFill);        // uncovered requirement, red

            AddLineLoop(vh, reqPts,  2.5f, MissionLine);
            AddLineLoop(vh, teamPts, 2.5f, TeamLine);
            return;
        }

        if (_hasRequired)
        {
            var pts = Polygon(_required, radius);
            AddFan(vh, pts, MissionFill);
            AddLineLoop(vh, pts, 2.5f, MissionLine);
        }

        if (_hasCombined)
        {
            var pts  = Polygon(_combined, radius);
            Color fill = _singleColors ? _singleFill : TeamFill;
            Color line = _singleColors ? _singleLine : TeamLine;
            AddFan(vh, pts, fill);
            AddLineLoop(vh, pts, 2.5f, line);
        }
    }

    // ── Geometry helpers ──────────────────────────────────────────────────────────
    private static Vector2[] BuildDirs()
    {
        var d = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            float rad = (90f - i * 72f) * Mathf.Deg2Rad;
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

    /// <summary>Per-axis min of two skill sets — the covered "overlap" polygon.</summary>
    private static Vector2[] OverlapPolygon(SkillSet a, SkillSet b, float radius)
    {
        float[] av = a.ToArray(), bv = b.ToArray();
        var pts = new Vector2[5];
        for (int i = 0; i < 5; i++)
        {
            float m = Mathf.Min(av[i], bv[i]);
            pts[i] = Dirs[i] * radius * (Mathf.Clamp(m, 0f, MaxValue) / MaxValue);
        }
        return pts;
    }

    /// <summary>Fills the ring between an inner and outer polygon (same vertex count).</summary>
    private static void AddBand(VertexHelper vh, Vector2[] inner, Vector2[] outer, Color col)
    {
        for (int i = 0; i < inner.Length; i++)
        {
            int j = (i + 1) % inner.Length;
            int s = vh.currentVertCount;
            vh.AddVert(inner[i], col, Vector2.zero);
            vh.AddVert(outer[i], col, Vector2.zero);
            vh.AddVert(outer[j], col, Vector2.zero);
            vh.AddVert(inner[j], col, Vector2.zero);
            vh.AddTriangle(s, s + 1, s + 2);
            vh.AddTriangle(s, s + 2, s + 3);
        }
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
        if (_labelMode == ChartLabelMode.None) return;

        if (!_labelsBuilt)
        {
            Rect  r      = rectTransform.rect;
            float radius = Mathf.Min(r.width, r.height) * 0.5f * RadiusFactor;

            _axisLabels = new Text[5];
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = Dirs[i] * (radius + LabelPad);
                pos.x *= LabelXStretch; // give the near-horizontal axes (DIP/RES) more breathing room
                _axisLabels[i] = UIHelper.Label(transform, LabelText(i), _labelFontSize,
                                                UIHelper.ColSubtext, pos, new Vector2(60, 18));
            }
            _labelsBuilt = true;
        }
        else
        {
            for (int i = 0; i < 5; i++)
                _axisLabels[i].text = LabelText(i);
        }
    }

    private string LabelText(int i)
    {
        string abbr = SkillSet.SkillAbbreviations[i];
        if (_labelMode != ChartLabelMode.NamesAndValues)
            return abbr;

        float  v       = _labelValues.ToArray()[i];
        string nameHex = _dim ? HexNameDim  : HexName;
        string valHex  = _dim ? HexValueDim : HexValue;
        return $"<color=#{nameHex}>{abbr}</color> <color=#{valHex}>{v:0}</color>";
    }
}
