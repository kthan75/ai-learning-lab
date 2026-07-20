using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Modal popup for assigning agents to a mission.
/// Shows mission info, 6 agent toggle-buttons, live success %, and Assign/Cancel.
/// </summary>
public class AssignmentPopup : MonoBehaviour
{
    public event Action<Mission, List<AgentData>> OnAssigned;
    public event Action                            OnCancelled;

    private Mission          _mission;
    private AgentData[]      _agents;
    private bool[]           _selected;
    private Button[]         _agentBtns;
    private Image[]          _agentBtnBgs;
    private Text[]           _agentBtnLabels;
    private Text             _lblTitle, _lblDesc, _lblChance;
    private Text             _lblSuccessHeader, _legendReq, _legendTeam, _lblOiiNotice;
    private SpiderChart[]    _agentCharts;
    private Image[]          _agentPortraits;
    private Button           _btnAssign;
    private GameObject       _root;
    private GameObject       _keySkillsRow;
    private SpiderChart      _spider;

    // ── Build (called once by GameUI) ─────────────────────────────────────────
    public void Build(Transform canvasRoot)
    {
        _root = UIHelper.Panel(canvasRoot, "AssignmentPopup",
                               UIHelper.BgPopup, Vector2.zero, new Vector2(1300, 980));
        _root.SetActive(false);

        _root.transform.SetAsLastSibling();
        _root.AddComponent<CanvasGroup>();

        var t = _root.transform;

        // ── Header ────────────────────────────────────────────────────────────
        _lblTitle = UIHelper.Label(t, "Mission Title", 26, UIHelper.ColText,
                                   new Vector2(0, 442), new Vector2(1240, 42),
                                   TextAnchor.UpperCenter, FontStyle.Bold);

        _lblDesc = UIHelper.Label(t, "Description", 16, UIHelper.ColSubtext,
                                  new Vector2(0, 390), new Vector2(1200, 52),
                                  TextAnchor.UpperCenter);
        _lblDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
        _lblDesc.verticalOverflow   = VerticalWrapMode.Overflow; // never truncate the description

        // ── Key skills required (caption + icon/abbr per skill, populated per mission) ─
        _keySkillsRow = new GameObject("KeySkills", typeof(RectTransform));
        _keySkillsRow.transform.SetParent(t, false);
        UIHelper.SetRect(_keySkillsRow, new Vector2(0, 360), new Vector2(1000, 30));
        var hlg = _keySkillsRow.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment        = TextAnchor.MiddleCenter;
        hlg.spacing               = 8f;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        // ── Divider ───────────────────────────────────────────────────────────
        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 338), new Vector2(1240, 2));

        // ── Agent cards (2 rows × 3): portrait (left) + mini spider chart (right) ─
        // Left-aligned to the left edge of the left-column cards (x = -416 - 400/2 = -616).
        UIHelper.Label(t, "Select agents (max 3):", 17, UIHelper.ColSubtext,
                       new Vector2(-446, 318), new Vector2(340, 24), TextAnchor.MiddleLeft);

        _agentBtns      = new Button[6];
        _agentBtnBgs    = new Image[6];
        _agentBtnLabels = new Text[6];
        _agentCharts    = new SpiderChart[6];
        _agentPortraits = new Image[6];

        float cardW = 400f, cardH = 200f, gapY = 16f;
        float rowY0 = 200f, rowY1 = rowY0 - cardH - gapY;
        float[] xs = { -416f, 0f, 416f };

        for (int i = 0; i < 6; i++)
        {
            int row = i / 3, col = i % 3;
            float x = xs[col], y = (row == 0) ? rowY0 : rowY1;

            var btn = UIHelper.Btn(t, "", new Vector2(x, y), new Vector2(cardW, cardH),
                                   UIHelper.BgCard, 15);
            _agentBtns[i]   = btn;
            _agentBtnBgs[i] = btn.GetComponent<Image>();

            // Name label — pinned to a band across the top of the card
            var nameLabel = btn.GetComponentInChildren<Text>();
            nameLabel.fontSize = 16;
            var nameRt = nameLabel.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 1f);
            nameRt.anchorMax = new Vector2(1f, 1f);
            nameRt.pivot     = new Vector2(0.5f, 1f);
            nameRt.offsetMin = new Vector2(0f, -34f);
            nameRt.offsetMax = new Vector2(0f, -4f);
            _agentBtnLabels[i] = nameLabel;

            // Portrait (left) — sprite assigned in RefreshAgentBtn once _agents is set
            _agentPortraits[i] = UIHelper.Portrait(btn.transform, null,
                                                   new Vector2(-105, -14), new Vector2(150, 150));

            // Mini spider chart (right) — agent's own skills, ENG/DIP… + values
            _agentCharts[i] = SpiderChart.Create(btn.transform, new Vector2(95, -10), 150f,
                                                 ChartLabelMode.NamesAndValues, 11);

            int idx = i;
            btn.onClick.AddListener(() => ToggleAgent(idx));
        }

        // ── Divider ───────────────────────────────────────────────────────────
        UIHelper.Panel(t, "Div2", UIHelper.AccentBlue, new Vector2(0, -130), new Vector2(1240, 2));

        // ── Summary spider chart (left) ─────────────────────────────────────────
        _spider = SpiderChart.Create(t, new Vector2(-430, -290), 210f);

        // ── Success chance + legend (right) ────────────────────────────────────
        _lblSuccessHeader = UIHelper.Label(t, "Success Chance:", 20, UIHelper.ColSubtext,
                       new Vector2(180, -232), new Vector2(340, 30));

        _lblChance = UIHelper.Label(t, "—", 38, UIHelper.ColText,
                                    new Vector2(180, -292), new Vector2(340, 48),
                                    TextAnchor.MiddleCenter, FontStyle.Bold);

        _legendReq = UIHelper.Label(t, "<color=#FFB840>■</color> Mission required", 15, UIHelper.ColSubtext,
                       new Vector2(180, -344), new Vector2(360, 24));
        _legendTeam = UIHelper.Label(t, "<color=#7AB3FF>■</color> Your team", 15, UIHelper.ColSubtext,
                       new Vector2(180, -370), new Vector2(360, 24));

        // ── OII notice (shown instead of the success preview for OII missions) ──
        _lblOiiNotice = UIHelper.Label(t, "Ops info incomplete.\nSuccess rate unknown.", 28,
                                       UIHelper.ColWarn, new Vector2(0, -280), new Vector2(1100, 130),
                                       TextAnchor.MiddleCenter, FontStyle.Bold);
        _lblOiiNotice.gameObject.SetActive(false);

        // ── Buttons ───────────────────────────────────────────────────────────
        var btnCancel = UIHelper.Btn(t, "Cancel", new Vector2(-170, -440),
                                     new Vector2(200, 52), UIHelper.ColDisabled, 19);
        btnCancel.onClick.AddListener(Cancel);

        _btnAssign = UIHelper.Btn(t, "Assign", new Vector2(170, -440),
                                  new Vector2(240, 52), UIHelper.AccentBlue, 22);
        _btnAssign.onClick.AddListener(Confirm);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Show(Mission mission, AgentData[] agents)
    {
        _mission  = mission;
        _agents   = agents;
        _selected = new bool[agents.Length];

        _lblTitle.text = mission.Template.missionTitle;
        _lblDesc.text  = FormatDescription(mission.Template.description);
        PopulateKeySkills(mission.Template.description);

        ApplyOiiVisibility(mission.OpsInfoIncomplete);

        for (int i = 0; i < agents.Length; i++)
            RefreshAgentBtn(i);

        RefreshChance();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    public void Hide() => _root.SetActive(false);

    /// <summary>
    /// For "Ops Info Incomplete" missions, hides the success preview (summary chart +
    /// Success % + legend) and shows the notice instead. Agent cards are unaffected.
    /// </summary>
    private void ApplyOiiVisibility(bool oii)
    {
        _spider.gameObject.SetActive(!oii);
        _lblSuccessHeader.gameObject.SetActive(!oii);
        _lblChance.gameObject.SetActive(!oii);
        _legendReq.gameObject.SetActive(!oii);
        _legendTeam.gameObject.SetActive(!oii);
        _lblOiiNotice.gameObject.SetActive(oii);
    }

    /// <summary>
    /// Strips the trailing skill hint (e.g. "[DIP, RES]") from the prose description —
    /// those skills are shown separately as the "Key skills required" icon row.
    /// </summary>
    private static string FormatDescription(string desc)
    {
        if (string.IsNullOrEmpty(desc)) return desc;
        int b = desc.LastIndexOf('[');
        if (b > 0 && desc.TrimEnd().EndsWith("]"))
            return desc.Substring(0, b).TrimEnd();
        return desc;
    }

    /// <summary>Parses the abbreviations out of the trailing "[SSM, NAV]" hint.</summary>
    private static List<string> ExtractKeySkills(string desc)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(desc)) return list;
        int b = desc.LastIndexOf('[');
        int e = desc.LastIndexOf(']');
        if (b < 0 || e <= b) return list;
        foreach (var part in desc.Substring(b + 1, e - b - 1).Split(','))
        {
            string s = part.Trim();
            if (s.Length > 0) list.Add(s);
        }
        return list;
    }

    /// <summary>Rebuilds the "Key skills required:" caption + icon/abbr chips for this mission.</summary>
    private void PopulateKeySkills(string desc)
    {
        for (int i = _keySkillsRow.transform.childCount - 1; i >= 0; i--)
            Destroy(_keySkillsRow.transform.GetChild(i).gameObject);

        var abbrevs = ExtractKeySkills(desc);
        _keySkillsRow.SetActive(abbrevs.Count > 0);
        if (abbrevs.Count == 0) return;

        AddKeySkillLabel("Key skills required:", UIHelper.ColSubtext, FontStyle.Normal);
        foreach (var ab in abbrevs)
        {
            var icon = ArtLoader.Load($"skill_{ab.ToLowerInvariant()}.png");
            if (icon != null) AddKeySkillIcon(icon);
            AddKeySkillLabel(ab, UIHelper.ColText, FontStyle.Bold);
        }
    }

    private void AddKeySkillLabel(string text, Color color, FontStyle style)
    {
        var lbl = UIHelper.Label(_keySkillsRow.transform, text, 17, color,
                                 Vector2.zero, new Vector2(10, 30), TextAnchor.MiddleLeft, style);
        lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
    }

    private void AddKeySkillIcon(Sprite icon)
    {
        var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(_keySkillsRow.transform, false);
        var img = go.GetComponent<Image>();
        img.sprite = icon; img.preserveAspect = true; img.raycastTarget = false;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 22f; le.preferredHeight = 22f;
    }

    // ── Private ───────────────────────────────────────────────────────────────
    private void ToggleAgent(int idx)
    {
        if (!_agents[idx].isAvailable) return;

        int selCount = 0;
        foreach (var b in _selected) if (b) selCount++;

        if (!_selected[idx] && selCount >= 3) return;  // max 3

        _selected[idx] = !_selected[idx];
        RefreshAgentBtn(idx);
        RefreshChance();
    }

    private void RefreshAgentBtn(int idx)
    {
        bool avail = _agents[idx].isAvailable;
        bool sel   = _selected[idx];

        _agentBtnBgs[idx].color    = sel   ? UIHelper.AccentBlue
                                   : avail ? UIHelper.BgCard
                                           : UIHelper.ColDisabled;
        _agentBtnLabels[idx].text  = _agents[idx].isIncapacitated
            ? $"{_agents[idx].agentName}   <color=#{ColorUtility.ToHtmlStringRGB(UIHelper.ColFail)}>[Incapacitated]</color>"
            : _agents[idx].agentName;
        _agentBtnLabels[idx].color = avail ? Color.white : UIHelper.ColSubtext;
        _agentBtns[idx].interactable = avail;

        UIHelper.SetPortrait(_agentPortraits[idx], _agents[idx].portrait, tinted: !avail);
        _agentCharts[idx].SetSingle(_agents[idx].skills, dimmed: !avail);
    }

    private void RefreshChance()
    {
        var selectedAgents = SelectedAgents();

        // OII mission: no preview to compute — just gate Assign on having a team.
        if (_mission.OpsInfoIncomplete)
        {
            _btnAssign.interactable = selectedAgents.Count > 0;
            return;
        }

        var required = _mission.Template.requiredSkills;
        if (selectedAgents.Count == 0)
        {
            _lblChance.text  = "—";
            _lblChance.color = UIHelper.ColSubtext;
            _btnAssign.interactable = false;
            _spider.SetData(required, true, default, false); // requirement only
            return;
        }

        var sets = new SkillSet[selectedAgents.Count];
        for (int i = 0; i < selectedAgents.Count; i++)
            sets[i] = selectedAgents[i].skills;
        var mode = GameManager.Instance.Config.skillCombineMode;
        var combined = SkillSet.CombineAll(sets, mode);
        float overlap = SkillSet.ComputeOverlap(combined, required);
        int pct = Mathf.RoundToInt(overlap * 100f);

        _lblChance.text  = $"{pct}%";
        _lblChance.color = pct >= 70 ? UIHelper.ColSuccess
                         : pct >= 40 ? UIHelper.ColWarn
                         : UIHelper.ColFail;

        _btnAssign.interactable = true;
        _spider.SetData(required, true, combined, true);
    }

    private void Confirm()
    {
        var agents = SelectedAgents();
        if (agents.Count == 0) return;
        Hide();
        OnAssigned?.Invoke(_mission, agents);
    }

    private void Cancel()
    {
        Hide();
        OnCancelled?.Invoke();
    }

    private List<AgentData> SelectedAgents()
    {
        var list = new List<AgentData>();
        for (int i = 0; i < _agents.Length; i++)
            if (_selected[i]) list.Add(_agents[i]);
        return list;
    }
}
