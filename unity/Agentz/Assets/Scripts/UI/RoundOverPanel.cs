using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen overlay shown when the round ends.
/// Displays score summary and offers Restart.
/// </summary>
public class RoundOverPanel : MonoBehaviour
{
    public event Action OnRestart;

    private Text       _lblHeader, _lblRound, _lblMissions, _lblGold, _lblScore;
    private GameObject _root;

    // ── Build ────────────────────────────────────────────────────────────────
    public void Build(Transform canvasRoot)
    {
        // Semi-transparent full-screen backdrop
        _root = UIHelper.PanelStretch(canvasRoot, "RoundOver",
                                      new Color(0f, 0f, 0f, 0.82f));
        _root.SetActive(false);

        // Card in center
        var card = UIHelper.Panel(_root.transform, "Card",
                                  UIHelper.BgPopup, Vector2.zero, new Vector2(520, 420));
        var t = card.transform;

        _lblHeader = UIHelper.Label(t, "ROUND OVER", 42, UIHelper.ColFail,
                                    new Vector2(0, 155), new Vector2(480, 60),
                                    TextAnchor.MiddleCenter, FontStyle.Bold);

        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 115), new Vector2(460, 2));

        _lblRound    = UIHelper.Label(t, "Round reached: 1",  22, UIHelper.ColText,
                                      new Vector2(0, 68), new Vector2(460, 34));
        _lblMissions = UIHelper.Label(t, "Missions completed: 0", 22, UIHelper.ColText,
                                      new Vector2(0, 28), new Vector2(460, 34));
        _lblGold     = UIHelper.Label(t, "Gold earned: 0",    22, UIHelper.ColWarn,
                                      new Vector2(0, -12), new Vector2(460, 34));
        _lblScore    = UIHelper.Label(t, "Score: 0",          30, UIHelper.ColText,
                                      new Vector2(0, -60), new Vector2(460, 40),
                                      TextAnchor.MiddleCenter, FontStyle.Bold);

        var btnRestart = UIHelper.Btn(t, "Play Again", new Vector2(0, -145),
                                      new Vector2(220, 54), UIHelper.AccentBlue, 22);
        btnRestart.onClick.AddListener(() => OnRestart?.Invoke());
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Show(int round, int missions, int gold, int score)
    {
        _lblRound.text    = $"Round reached:  {round}";
        _lblMissions.text = $"Missions completed:  {missions}";
        _lblGold.text     = $"Gold earned:  {gold}";
        _lblScore.text    = $"Score:  {score}";

        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    public void Hide() => _root.SetActive(false);
}
