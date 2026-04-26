using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen overlay shown when a round ends.
/// Success: shows "ROUND COMPLETE" with Next Round + Quit.
/// Failure: shows "GAME OVER" with Play Again + Quit.
/// </summary>
public class RoundOverPanel : MonoBehaviour
{
    public event Action OnNextRound;
    public event Action OnRestart;
    public event Action OnQuit;

    private Text       _lblHeader, _lblRound, _lblMissions, _lblGold, _lblScore;
    private Button     _btnPrimary;
    private Text       _btnPrimaryLabel;
    private GameObject _root;

    // ── Build ────────────────────────────────────────────────────────────────
    public void Build(Transform canvasRoot)
    {
        _root = UIHelper.PanelStretch(canvasRoot, "RoundOver",
                                      new Color(0f, 0f, 0f, 0.82f));
        _root.SetActive(false);

        var card = UIHelper.Panel(_root.transform, "Card",
                                  UIHelper.BgPopup, Vector2.zero, new Vector2(520, 440));
        var t = card.transform;

        _lblHeader = UIHelper.Label(t, "ROUND COMPLETE", 42, UIHelper.ColSuccess,
                                    new Vector2(0, 165), new Vector2(480, 60),
                                    TextAnchor.MiddleCenter, FontStyle.Bold);

        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 125), new Vector2(460, 2));

        _lblRound    = UIHelper.Label(t, "Round reached: 1",      22, UIHelper.ColText,
                                      new Vector2(0, 78), new Vector2(460, 34));
        _lblMissions = UIHelper.Label(t, "Missions completed: 0", 22, UIHelper.ColText,
                                      new Vector2(0, 38), new Vector2(460, 34));
        _lblGold     = UIHelper.Label(t, "Gold earned: 0",        22, UIHelper.ColWarn,
                                      new Vector2(0, -2), new Vector2(460, 34));
        _lblScore    = UIHelper.Label(t, "Score: 0",              30, UIHelper.ColText,
                                      new Vector2(0, -50), new Vector2(460, 40),
                                      TextAnchor.MiddleCenter, FontStyle.Bold);

        // Primary action button (Next Round / Play Again)
        _btnPrimary = UIHelper.Btn(t, "Next Round", new Vector2(-80, -140),
                                   new Vector2(210, 54), UIHelper.AccentBlue, 22);
        _btnPrimaryLabel = _btnPrimary.GetComponentInChildren<Text>();
        _btnPrimary.onClick.AddListener(OnPrimaryClicked);

        // Quit button
        var btnQuit = UIHelper.Btn(t, "Quit", new Vector2(110, -140),
                                   new Vector2(130, 54), UIHelper.ColDisabled, 20);
        btnQuit.onClick.AddListener(() => OnQuit?.Invoke());
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Show(int round, int missions, int roundGold, int totalGold,
                     int roundScore, int totalScore, bool wasFailure)
    {
        _lblHeader.text  = wasFailure ? "GAME OVER" : "ROUND COMPLETE";
        _lblHeader.color = wasFailure ? UIHelper.ColFail : UIHelper.ColSuccess;

        _btnPrimaryLabel.text = wasFailure ? "Play Again" : "Next Round";

        _lblRound.text    = $"Round reached:  {round}";
        _lblMissions.text = $"Missions completed:  {missions}";
        _lblGold.text     = $"Gold (round/total):  {roundGold} / {totalGold}";
        _lblScore.text    = $"Score (round/total):  {roundScore} / {totalScore}";

        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    public void Hide() => _root.SetActive(false);

    // ── Private ───────────────────────────────────────────────────────────────
    private void OnPrimaryClicked()
    {
        if (GameManager.Instance.RoundWasFailure)
            OnRestart?.Invoke();
        else
            OnNextRound?.Invoke();
    }
}
