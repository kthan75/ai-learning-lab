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
    private SpiderChart[]    _agentCharts;
    private Button           _btnAssign;
    private GameObject       _root;
    private SpiderChart      _spider;

    // ── Build (called once by GameUI) ─────────────────────────────────────────
    public void Build(Transform canvasRoot)
    {
        _root = UIHelper.Panel(canvasRoot, "AssignmentPopup",
                               UIHelper.BgPopup, Vector2.zero, new Vector2(880, 820));
        _root.SetActive(false);

        _root.transform.SetAsLastSibling();
        _root.AddComponent<CanvasGroup>();

        var t = _root.transform;

        // ── Header ────────────────────────────────────────────────────────────
        _lblTitle = UIHelper.Label(t, "Mission Title", 26, UIHelper.ColText,
                                   new Vector2(0, 372), new Vector2(820, 42),
                                   TextAnchor.UpperCenter, FontStyle.Bold);

        _lblDesc = UIHelper.Label(t, "Description", 16, UIHelper.ColSubtext,
                                  new Vector2(0, 320), new Vector2(800, 58),
                                  TextAnchor.UpperCenter);
        _lblDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
        _lblDesc.verticalOverflow   = VerticalWrapMode.Overflow; // never truncate the description

        // ── Divider ───────────────────────────────────────────────────────────
        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 282), new Vector2(820, 2));

        // ── Agent cards (2 rows × 3), each with a mini spider chart ─────────────
        UIHelper.Label(t, "Select agents (max 3):", 17, UIHelper.ColSubtext,
                       new Vector2(-250, 258), new Vector2(320, 24), TextAnchor.MiddleLeft);

        _agentBtns      = new Button[6];
        _agentBtnBgs    = new Image[6];
        _agentBtnLabels = new Text[6];
        _agentCharts    = new SpiderChart[6];

        float cardW = 250f, cardH = 152f, gapY = 14f;
        float rowY0 = 150f, rowY1 = rowY0 - cardH - gapY;
        float[] xs = { -268f, 0f, 268f };

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
            nameLabel.fontSize = 15;
            var nameRt = nameLabel.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 1f);
            nameRt.anchorMax = new Vector2(1f, 1f);
            nameRt.pivot     = new Vector2(0.5f, 1f);
            nameRt.offsetMin = new Vector2(0f, -30f);
            nameRt.offsetMax = new Vector2(0f, -4f);
            _agentBtnLabels[i] = nameLabel;

            // Mini spider chart (agent's own skills, with ENG/DIP… + values)
            _agentCharts[i] = SpiderChart.Create(btn.transform, new Vector2(0, -16), 116f,
                                                 ChartLabelMode.NamesAndValues, 10);

            int idx = i;
            btn.onClick.AddListener(() => ToggleAgent(idx));
        }

        // ── Divider ───────────────────────────────────────────────────────────
        UIHelper.Panel(t, "Div2", UIHelper.AccentBlue, new Vector2(0, -108), new Vector2(820, 2));

        // ── Summary spider chart (left) ─────────────────────────────────────────
        _spider = SpiderChart.Create(t, new Vector2(-245, -232), 205f);

        // ── Success chance + legend (right) ────────────────────────────────────
        UIHelper.Label(t, "Success Chance:", 20, UIHelper.ColSubtext,
                       new Vector2(175, -168), new Vector2(320, 30));

        _lblChance = UIHelper.Label(t, "—", 36, UIHelper.ColText,
                                    new Vector2(175, -220), new Vector2(320, 46),
                                    TextAnchor.MiddleCenter, FontStyle.Bold);

        UIHelper.Label(t, "<color=#FFB840>■</color> Mission required", 15, UIHelper.ColSubtext,
                       new Vector2(175, -268), new Vector2(320, 24));
        UIHelper.Label(t, "<color=#7AB3FF>■</color> Your team", 15, UIHelper.ColSubtext,
                       new Vector2(175, -294), new Vector2(320, 24));

        // ── Buttons ───────────────────────────────────────────────────────────
        var btnCancel = UIHelper.Btn(t, "Cancel", new Vector2(-160, -372),
                                     new Vector2(190, 52), UIHelper.ColDisabled, 19);
        btnCancel.onClick.AddListener(Cancel);

        _btnAssign = UIHelper.Btn(t, "Assign", new Vector2(160, -372),
                                  new Vector2(230, 52), UIHelper.AccentBlue, 22);
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

        for (int i = 0; i < agents.Length; i++)
            RefreshAgentBtn(i);

        RefreshChance();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    public void Hide() => _root.SetActive(false);

    /// <summary>
    /// Puts the trailing skill hint (e.g. "[DIP, RES]") on its own line so it always
    /// reads as a centered second line under the prose description.
    /// </summary>
    private static string FormatDescription(string desc)
    {
        if (string.IsNullOrEmpty(desc)) return desc;
        int b = desc.LastIndexOf('[');
        if (b > 0 && desc.TrimEnd().EndsWith("]"))
            return desc.Substring(0, b).TrimEnd() + "\n" + desc.Substring(b).Trim();
        return desc;
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
        _agentBtnLabels[idx].text  = _agents[idx].agentName;
        _agentBtnLabels[idx].color = avail ? Color.white : UIHelper.ColSubtext;
        _agentBtns[idx].interactable = avail;

        _agentCharts[idx].SetSingle(_agents[idx].skills, dimmed: !avail);
    }

    private void RefreshChance()
    {
        var required = _mission.Template.requiredSkills;
        var selectedAgents = SelectedAgents();
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
