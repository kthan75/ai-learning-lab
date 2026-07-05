using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the mission resolution result: skill match %, dice roll, and outcome.
/// Results queue up if multiple missions resolve simultaneously.
/// </summary>
public class ResultPopup : MonoBehaviour
{
    public event Action OnDismissed;

    private Text        _lblOutcome, _lblMission, _lblMatch, _lblRoll, _lblAgents;
    private Image       _bg;
    private GameObject  _root;
    private SpiderChart _spider;

    private readonly Queue<ResultData> _queue = new Queue<ResultData>();
    private bool _showing;

    public bool IsShowing  => _showing;
    public bool HasQueued  => _queue.Count > 0 || _showing;

    private struct ResultData
    {
        public string   MissionTitle;
        public bool     Success;
        public float    OverlapPct;
        public int      DiceRoll;
        public string   AgentNames;
        public SkillSet Combined;
        public SkillSet Requirement;
        public bool     HasChart;
    }

    // ── Build ────────────────────────────────────────────────────────────────
    public void Build(Transform canvasRoot)
    {
        _root = UIHelper.Panel(canvasRoot, "ResultPopup",
                               UIHelper.BgPopup, Vector2.zero, new Vector2(520, 460));
        _root.SetActive(false);

        _bg = _root.GetComponent<Image>();
        var t = _root.transform;

        _lblOutcome = UIHelper.Label(t, "SUCCESS", 38, UIHelper.ColSuccess,
                                     new Vector2(0, 180), new Vector2(480, 56),
                                     TextAnchor.MiddleCenter, FontStyle.Bold);

        _lblMission = UIHelper.Label(t, "Mission Title", 18, UIHelper.ColSubtext,
                                     new Vector2(0, 140), new Vector2(480, 26),
                                     TextAnchor.MiddleCenter);

        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 118), new Vector2(460, 2));

        // ── Spider chart ────────────────────────────────────────────────────────
        _spider = SpiderChart.Create(t, new Vector2(0, 30), 175f);

        UIHelper.Label(t, "<color=#FFB840>■</color> Required   <color=#7AB3FF>■</color> Team",
                       14, UIHelper.ColSubtext, new Vector2(0, -75), new Vector2(480, 24));

        _lblMatch = UIHelper.Label(t, "Skill match: 72%", 19, UIHelper.ColText,
                                   new Vector2(0, -104), new Vector2(480, 28));

        _lblRoll  = UIHelper.Label(t, "Rolled: 45  (needed ≤ 72)", 17, UIHelper.ColText,
                                   new Vector2(0, -134), new Vector2(480, 26));

        _lblAgents = UIHelper.Label(t, "Agents: Zara, Mira", 14, UIHelper.ColSubtext,
                                    new Vector2(0, -162), new Vector2(480, 24));

        var btnOK = UIHelper.Btn(t, "OK", new Vector2(0, -200),
                                 new Vector2(180, 46), UIHelper.AccentBlue, 20);
        btnOK.onClick.AddListener(Dismiss);
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Enqueue(Mission mission, bool success, float overlapPct, int roll)
    {
        var names = new System.Text.StringBuilder();
        for (int i = 0; i < mission.AssignedAgents.Count; i++)
        {
            if (i > 0) names.Append(", ");
            names.Append(mission.AssignedAgents[i].agentName);
        }

        _queue.Enqueue(new ResultData
        {
            MissionTitle = mission.Template.missionTitle,
            Success      = success,
            OverlapPct   = overlapPct,
            DiceRoll     = roll,
            AgentNames   = names.ToString(),
            Combined     = mission.CombinedSkills,
            Requirement  = mission.ScaledRequirement,
            HasChart     = mission.AssignedAgents.Count > 0
        });

        if (!_showing) ShowNext();
    }

    // ── Private ──────────────────────────────────────────────────────────────
    private void ShowNext()
    {
        if (_queue.Count == 0) { _showing = false; return; }

        _showing = true;
        var d    = _queue.Dequeue();
        int pct  = Mathf.RoundToInt(d.OverlapPct * 100f);
        int need = pct;

        _lblOutcome.text  = d.Success ? "✓  SUCCESS" : "✗  FAILED";
        _lblOutcome.color = d.Success ? UIHelper.ColSuccess : UIHelper.ColFail;
        _bg.color         = d.Success ? new Color(0.06f, 0.14f, 0.08f, 0.97f)
                                      : new Color(0.16f, 0.06f, 0.06f, 0.97f);

        _lblMission.text = d.MissionTitle;
        _lblMatch.text   = $"Skill match:  {pct}%";
        _lblRoll.text    = $"Rolled: {d.DiceRoll}   (needed ≤ {need})";
        _lblAgents.text  = $"Agents: {d.AgentNames}";
        _spider.SetData(d.Requirement, d.HasChart, d.Combined, d.HasChart);

        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    private void Dismiss()
    {
        _root.SetActive(false);
        _showing = false;
        OnDismissed?.Invoke();
        ShowNext();
    }
}
