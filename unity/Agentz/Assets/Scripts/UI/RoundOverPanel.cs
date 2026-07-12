using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen overlay shown when a round ends.
/// Success: shows "ROUND COMPLETE" with Upgrade Agents (→ upgrade screen) + Quit.
/// Failure: shows "GAME OVER" with Play Again + Quit.
/// </summary>
public class RoundOverPanel : MonoBehaviour
{
    public event Action OnUpgrade;   // success → open the upgrade screen
    public event Action OnRestart;   // failure → play again from round 1
    public event Action OnQuit;

    private Text       _lblHeader, _lblRound, _lblMissions, _lblRoundBonus, _lblNoFailBonus, _lblGold, _lblScore, _lblHighScore;
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
                                  UIHelper.BgPopup, Vector2.zero, new Vector2(520, 500));
        var t = card.transform;

        _lblHeader = UIHelper.Label(t, "ROUND COMPLETE", 40, UIHelper.ColSuccess,
                                    new Vector2(0, 205), new Vector2(480, 56),
                                    TextAnchor.MiddleCenter, FontStyle.Bold);

        UIHelper.Panel(t, "Div", UIHelper.AccentBlue, new Vector2(0, 165), new Vector2(460, 2));

        _lblRound       = UIHelper.Label(t, "Round reached: 1",      21, UIHelper.ColText,
                                         new Vector2(0, 122), new Vector2(460, 32));
        _lblMissions    = UIHelper.Label(t, "Missions completed: 0", 21, UIHelper.ColText,
                                         new Vector2(0, 88), new Vector2(460, 32));
        _lblRoundBonus  = UIHelper.Label(t, "Round bonus:  +25 g",   20, UIHelper.ColSuccess,
                                         new Vector2(0, 54), new Vector2(460, 30));
        _lblNoFailBonus = UIHelper.Label(t, "No-fails bonus:  +25 g", 20, UIHelper.ColSuccess,
                                         new Vector2(0, 22), new Vector2(460, 30));
        _lblGold        = UIHelper.Label(t, "Gold earned: 0",        21, UIHelper.ColWarn,
                                         new Vector2(0, -16), new Vector2(460, 32));
        _lblScore       = UIHelper.Label(t, "Score: 0",              28, UIHelper.ColText,
                                         new Vector2(0, -58), new Vector2(460, 38),
                                         TextAnchor.MiddleCenter, FontStyle.Bold);
        _lblHighScore   = UIHelper.Label(t, "Current Highscore: 0",  19, UIHelper.ColSubtext,
                                         new Vector2(0, -100), new Vector2(460, 28));

        // Primary action button (Upgrade Agents / Play Again)
        _btnPrimary = UIHelper.Btn(t, "Upgrade Agents", new Vector2(-80, -164),
                                   new Vector2(210, 54), UIHelper.AccentBlue, 22);
        _btnPrimaryLabel = _btnPrimary.GetComponentInChildren<Text>();
        _btnPrimary.onClick.AddListener(OnPrimaryClicked);

        // Quit button
        var btnQuit = UIHelper.Btn(t, "Quit", new Vector2(110, -164),
                                   new Vector2(130, 54), UIHelper.ColDisabled, 20);
        btnQuit.onClick.AddListener(() => OnQuit?.Invoke());
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Show(int round, int missions, int roundGold, int totalGold,
                     int roundScore, int totalScore, bool wasFailure)
    {
        _lblHeader.text  = wasFailure ? "GAME OVER" : "ROUND COMPLETE";
        _lblHeader.color = wasFailure ? UIHelper.ColFail : UIHelper.ColSuccess;

        _btnPrimaryLabel.text = wasFailure ? "Play Again" : "Upgrade Agents";

        _lblRound.text    = $"Round reached:  {round}";
        _lblMissions.text = $"Missions completed:  {missions}";
        _lblGold.text      = $"Gold (round/total):  {roundGold} / {totalGold}";
        _lblScore.text     = $"Score (round/total):  {roundScore} / {totalScore}";
        _lblHighScore.text = $"Current Highscore:  {GameManager.Instance.HighScore}";

        // Bonus breakdown — only on a survived round.
        var gm = GameManager.Instance;
        int noFail = gm.Failures == 0 ? gm.Config.noFailBonus : 0;
        _lblRoundBonus.text   = $"Round bonus:  +{gm.Config.roundCompletionBonus} g";
        _lblNoFailBonus.text  = $"No-fails bonus:  +{noFail} g";
        _lblNoFailBonus.color = noFail > 0 ? UIHelper.ColSuccess : UIHelper.ColSubtext;
        _lblRoundBonus.gameObject.SetActive(!wasFailure);
        _lblNoFailBonus.gameObject.SetActive(!wasFailure);

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
            OnUpgrade?.Invoke();
    }
}
