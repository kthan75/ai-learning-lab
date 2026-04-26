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

    private Text       _lblOutcome, _lblMission, _lblMatch, _lblRoll, _lblAgents;
    private Image      _bg;
    private GameObject _root;

    private readonly Queue<ResultData> _queue = new Queue<ResultData>();
    private bool _showing;

    private struct ResultData
    {
        public string MissionTitle;
        public bool   Success;
        public float  OverlapPct;
        public int    DiceRoll;
        public string AgentNames;
    }

    // ── Build ────────────────────────────────────────────────────────────────
    public void Build(Transform canvasRoot)
    {
        _root = UIHelper.Panel(canvasRoot, "ResultPopup",
                               UIHelper.BgPopup, Vector2.zero, new Vector2(480, 320));
        _root.SetActive(false);

        _bg = _root.GetComponent<Image>();
        var t = _root.transform;

        _lblOutcome = UIHelper.Label(t, "SUCCESS", 40, UIHelper.ColSuccess,
                                     new Vector2(0, 110), new Vector2(440, 60),
                                     TextAnchor.MiddleCenter, FontStyle.Bold);

        _lblMission = UIHelper.Label(t, "Mission Title", 18, UIHelper.ColSubtext,
                                     new Vector2(0, 65), new Vector2(440, 28),
                                     TextAnchor.MiddleCenter);

        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 42), new Vector2(420, 2));

        _lblMatch = UIHelper.Label(t, "Skill match: 72%", 20, UIHelper.ColText,
                                   new Vector2(0, 15), new Vector2(440, 30));

        _lblRoll  = UIHelper.Label(t, "Rolled: 45  (needed ≤ 72)", 18, UIHelper.ColText,
                                   new Vector2(0, -20), new Vector2(440, 28));

        _lblAgents = UIHelper.Label(t, "Agents: Zara, Mira", 15, UIHelper.ColSubtext,
                                    new Vector2(0, -55), new Vector2(440, 24));

        var btnOK = UIHelper.Btn(t, "OK", new Vector2(0, -120),
                                 new Vector2(180, 48), UIHelper.AccentBlue, 20);
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
            AgentNames   = names.ToString()
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
