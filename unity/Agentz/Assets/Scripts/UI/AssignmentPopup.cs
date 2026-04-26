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
    private Text[]           _agentSkillLabels;
    private Button           _btnAssign;
    private GameObject       _root;

    // Skill value color (bright amber — contrasts with the grey label text)
    private static readonly Color ColSkillValue = new Color(0.91f, 0.78f, 0.29f);

    // ── Build (called once by GameUI) ─────────────────────────────────────────
    public void Build(Transform canvasRoot)
    {
        _root = UIHelper.Panel(canvasRoot, "AssignmentPopup",
                               UIHelper.BgPopup, Vector2.zero, new Vector2(780, 560));
        _root.SetActive(false);

        _root.transform.SetAsLastSibling();
        _root.AddComponent<CanvasGroup>();

        var t = _root.transform;

        // ── Header ────────────────────────────────────────────────────────────
        _lblTitle = UIHelper.Label(t, "Mission Title", 26, UIHelper.ColText,
                                   new Vector2(0, 228), new Vector2(740, 44),
                                   TextAnchor.UpperCenter, FontStyle.Bold);

        _lblDesc = UIHelper.Label(t, "Description", 17, UIHelper.ColSubtext,
                                  new Vector2(0, 178), new Vector2(740, 38),
                                  TextAnchor.UpperCenter);

        // ── Divider ───────────────────────────────────────────────────────────
        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 150), new Vector2(720, 2));

        // ── Agent buttons (2 rows × 3) ────────────────────────────────────────
        UIHelper.Label(t, "Select agents (max 3):", 17, UIHelper.ColSubtext,
                       new Vector2(-260, 128), new Vector2(280, 28));

        _agentBtns        = new Button[6];
        _agentBtnBgs      = new Image[6];
        _agentBtnLabels   = new Text[6];
        _agentSkillLabels = new Text[6];

        float btnW = 230f, btnH = 76f, gapY = 12f;
        float rowY0 = 70f, rowY1 = rowY0 - btnH - gapY;
        float[] xs = { -240f, 0f, 240f };

        for (int i = 0; i < 6; i++)
        {
            int row = i / 3, col = i % 3;
            float x = xs[col], y = (row == 0) ? rowY0 : rowY1;

            var btn = UIHelper.Btn(t, "", new Vector2(x, y), new Vector2(btnW, btnH),
                                   UIHelper.BgCard, 15);
            _agentBtns[i]   = btn;
            _agentBtnBgs[i] = btn.GetComponent<Image>();

            // Name label — upper 45% of the button
            var nameLabel = btn.GetComponentInChildren<Text>();
            var nameRt    = nameLabel.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 0.45f);
            nameRt.anchorMax = new Vector2(1f, 1f);
            nameRt.offsetMin = nameRt.offsetMax = Vector2.zero;
            _agentBtnLabels[i] = nameLabel;

            // Skill label — lower 45% of the button
            var skillGo = new GameObject("SkillLabel", typeof(RectTransform));
            skillGo.transform.SetParent(btn.transform, false);
            var skillRt       = skillGo.GetComponent<RectTransform>();
            skillRt.anchorMin = new Vector2(0f, 0f);
            skillRt.anchorMax = new Vector2(1f, 0.45f);
            skillRt.offsetMin = skillRt.offsetMax = Vector2.zero;
            var skillText         = skillGo.AddComponent<UnityEngine.UI.Text>();
            skillText.font        = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            skillText.fontSize    = 13;
            skillText.color       = Color.white; // base color; rich text overrides per-token
            skillText.alignment   = TextAnchor.MiddleCenter;
            _agentSkillLabels[i]  = skillText;

            int idx = i;
            btn.onClick.AddListener(() => ToggleAgent(idx));
        }

        // ── Success chance ────────────────────────────────────────────────────
        UIHelper.Label(t, "Success Chance:", 20, UIHelper.ColSubtext,
                       new Vector2(-140, -108), new Vector2(220, 32));

        _lblChance = UIHelper.Label(t, "—", 32, UIHelper.ColText,
                                    new Vector2(120, -108), new Vector2(180, 40),
                                    TextAnchor.MiddleCenter, FontStyle.Bold);

        // ── Buttons ───────────────────────────────────────────────────────────
        var btnCancel = UIHelper.Btn(t, "Cancel", new Vector2(-170, -230),
                                     new Vector2(180, 52), UIHelper.ColDisabled, 19);
        btnCancel.onClick.AddListener(Cancel);

        _btnAssign = UIHelper.Btn(t, "Assign", new Vector2(160, -230),
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
        _lblDesc.text  = mission.Template.description;

        for (int i = 0; i < agents.Length; i++)
            RefreshAgentBtn(i);

        RefreshChance();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    public void Hide() => _root.SetActive(false);

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

        var s = _agents[idx].skills;
        string valHex  = avail ? ColorUtility.ToHtmlStringRGB(ColSkillValue) : "666666";
        string lblHex  = avail ? ColorUtility.ToHtmlStringRGB(UIHelper.ColSubtext) : "444444";
        _agentSkillLabels[idx].text =
            $"<color=#{lblHex}>ENG:</color><color=#{valHex}>{s.engineering:0}</color>  " +
            $"<color=#{lblHex}>DIP:</color><color=#{valHex}>{s.diplomacy:0}</color>  " +
            $"<color=#{lblHex}>NAV:</color><color=#{valHex}>{s.navigation:0}</color>  " +
            $"<color=#{lblHex}>SSM:</color><color=#{valHex}>{s.streetSmarts:0}</color>  " +
            $"<color=#{lblHex}>RES:</color><color=#{valHex}>{s.resilience:0}</color>";
    }

    private void RefreshChance()
    {
        var selectedAgents = SelectedAgents();
        if (selectedAgents.Count == 0)
        {
            _lblChance.text  = "—";
            _lblChance.color = UIHelper.ColSubtext;
            _btnAssign.interactable = false;
            return;
        }

        var sets = new SkillSet[selectedAgents.Count];
        for (int i = 0; i < selectedAgents.Count; i++)
            sets[i] = selectedAgents[i].skills;
        var mode = GameManager.Instance.Config.skillCombineMode;
        var combined = SkillSet.CombineAll(sets, mode);
        float overlap = SkillSet.ComputeOverlap(combined, _mission.Template.requiredSkills);
        int pct = Mathf.RoundToInt(overlap * 100f);

        _lblChance.text  = $"{pct}%";
        _lblChance.color = pct >= 70 ? UIHelper.ColSuccess
                         : pct >= 40 ? UIHelper.ColWarn
                         : UIHelper.ColFail;

        _btnAssign.interactable = true;
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
