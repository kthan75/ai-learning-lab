using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Modal event popup shown when an agent is incapacitated: portrait, name, reason, and
/// how long they're out. OK dismisses it and returns to the dispatch screen.
/// </summary>
public class IncapacitationPopup : MonoBehaviour
{
    public event Action OnDismissed;

    private Image      _portrait;
    private Text       _lblName, _lblReason, _lblDuration;
    private GameObject _root;

    public void Build(Transform canvasRoot)
    {
        _root = UIHelper.Panel(canvasRoot, "IncapacitationPopup",
                               UIHelper.BgPopup, Vector2.zero, new Vector2(520, 440));
        _root.SetActive(false);
        var t = _root.transform;

        UIHelper.Label(t, "AGENT UNAVAILABLE", 30, UIHelper.ColWarn,
                       new Vector2(0, 168), new Vector2(480, 46),
                       TextAnchor.MiddleCenter, FontStyle.Bold);

        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 134), new Vector2(460, 2));

        _portrait = UIHelper.Portrait(t, null, new Vector2(0, 48), new Vector2(150, 150));

        _lblName = UIHelper.Label(t, "Agent Name", 24, UIHelper.ColText,
                                  new Vector2(0, -52), new Vector2(480, 34),
                                  TextAnchor.MiddleCenter, FontStyle.Bold);

        _lblReason = UIHelper.Label(t, "Reason", 19, UIHelper.ColSubtext,
                                    new Vector2(0, -88), new Vector2(480, 28));

        _lblDuration = UIHelper.Label(t, "Out of action for ~5 seconds.", 18, UIHelper.ColFail,
                                      new Vector2(0, -122), new Vector2(480, 28));

        var btnOK = UIHelper.Btn(t, "OK", new Vector2(0, -178),
                                 new Vector2(180, 48), UIHelper.AccentBlue, 20);
        btnOK.onClick.AddListener(Dismiss);
    }

    public void Show(AgentData agent, float duration)
    {
        UIHelper.SetPortrait(_portrait, agent.portrait, tinted: false);
        _lblName.text   = agent.agentName;
        _lblReason.text = agent.incapReason;

        int secs = Mathf.Max(1, Mathf.CeilToInt(duration));
        _lblDuration.text = $"Out of action for ~{secs} second{(secs == 1 ? "" : "s")}.";

        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    private void Dismiss()
    {
        _root.SetActive(false);
        OnDismissed?.Invoke();
    }
}
