using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top strip HUD: Round #, timer, failures, gold, score.
/// Created and owned by GameUI.
/// </summary>
public class HUDPanel : MonoBehaviour
{
    private Text _lblRound, _lblTimer, _lblFailures, _lblGold, _lblScore;

    public void Build(Transform canvasRoot, GameConfig config)
    {
        // Background strip anchored to top
        var bg = UIHelper.PanelStretch(canvasRoot, "HUD", UIHelper.BgCard);
        var bgRt = bg.GetComponent<RectTransform>();
        UIHelper.AnchorTopStretch(bgRt, height: 70);

        var t = bg.transform;
        int fs = 22;

        _lblRound    = UIHelper.Label(t, "Round 1",    fs, UIHelper.ColText,
                                      new Vector2(-760, 0), new Vector2(160, 60));
        _lblTimer    = UIHelper.Label(t, "3:00",       30, UIHelper.ColText,
                                      new Vector2(0, 0),    new Vector2(160, 60),
                                      TextAnchor.MiddleCenter, FontStyle.Bold);
        _lblFailures = UIHelper.Label(t, "Fails: 0/4",        fs, UIHelper.ColFail,
                                      new Vector2(300, 0),  new Vector2(180, 60));
        // Gold icon (if available) + value; drop the "Gold" word since the icon conveys it
        var goldIcon = ArtLoader.Load("gold.png");
        if (goldIcon != null)
        {
            var iconGo = new GameObject("GoldIcon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(t, false);
            UIHelper.SetRect(iconGo, new Vector2(448, 0), new Vector2(32, 32));
            var img = iconGo.GetComponent<Image>();
            img.sprite = goldIcon;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }
        _lblGold     = UIHelper.Label(t, "R/T: 0/0",         fs, UIHelper.ColWarn,
                                      new Vector2(530, 0),  new Vector2(230, 60));
        _lblScore    = UIHelper.Label(t, "Score (R/T): 0/0", fs, UIHelper.ColText,
                                      new Vector2(760, 0),  new Vector2(260, 60));

        Refresh(config.roundDuration, 0, 0, 0, 0, 0, 1);
    }

    public void Refresh(float timeRemaining, int failures,
                        int roundGold, int totalGold,
                        int roundScore, int totalScore,
                        int round)
    {
        int m = Mathf.FloorToInt(timeRemaining / 60f);
        int s = Mathf.FloorToInt(timeRemaining % 60f);
        _lblTimer.text    = $"{m}:{s:D2}";
        _lblTimer.color   = timeRemaining < 30f ? UIHelper.ColFail
                          : timeRemaining < 60f ? UIHelper.ColWarn
                          : UIHelper.ColText;

        _lblFailures.text = $"Fails: {failures}/4";
        _lblGold.text     = $"R/T: {roundGold}/{totalGold}";
        _lblScore.text    = $"Score (R/T): {roundScore}/{totalScore}";
        _lblRound.text    = $"Round {round}";
    }
}
