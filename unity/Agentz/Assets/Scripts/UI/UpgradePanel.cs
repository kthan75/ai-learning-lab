using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Between-round upgrade screen (flat grid). Spend cumulative gold to raise agent skills
/// (+1 per click). Cost to raise a skill from level L → L+1 is
/// `skillUpgradeCostBase + L * skillUpgradeCostRamp`, capped at 10. Reached from the
/// Round Complete panel; "Next Round" continues the run.
/// </summary>
public class UpgradePanel : MonoBehaviour
{
    public event Action OnNextRound;

    private AgentData[] _agents;
    private GameObject  _root;
    private Text        _lblGold;
    private Text[,]     _lblValue;
    private Text[,]     _lblCost;
    private Button[,]   _btnPlus;
    private Button[,]   _btnMinus;
    private int[,]      _sessionBase; // skill levels when this upgrade screen opened (undo floor)

    // ── Build ────────────────────────────────────────────────────────────────
    public void Build(Transform canvasRoot, AgentData[] agents)
    {
        _agents      = agents;
        int n        = agents.Length;
        _lblValue    = new Text[n, 5];
        _lblCost     = new Text[n, 5];
        _btnPlus     = new Button[n, 5];
        _btnMinus    = new Button[n, 5];
        _sessionBase = new int[n, 5];

        _root = UIHelper.PanelStretch(canvasRoot, "UpgradePanel", new Color(0f, 0f, 0f, 0.88f));
        _root.SetActive(false);
        var t = _root.transform;

        UIHelper.Label(t, "UPGRADE AGENTS", 34, UIHelper.ColText,
                       new Vector2(0, 430), new Vector2(800, 50), TextAnchor.MiddleCenter, FontStyle.Bold);

        _lblGold = UIHelper.Label(t, "Gold:  0", 24, UIHelper.ColWarn,
                                  new Vector2(0, 384), new Vector2(400, 34),
                                  TextAnchor.MiddleCenter, FontStyle.Bold);

        float cardW = 500f, cardH = 260f, gapX = 30f, gapY = 26f;
        float[] xs = { -(cardW + gapX), 0f, cardW + gapX };
        float rowY0 = 150f, rowY1 = rowY0 - cardH - gapY;
        float[] rowYs = { 38f, 6f, -26f, -58f, -90f };

        for (int i = 0; i < n; i++)
        {
            int row = i / 3, col = i % 3;
            var card = UIHelper.Panel(t, "Up_" + i, UIHelper.BgCard,
                                      new Vector2(xs[col], row == 0 ? rowY0 : rowY1),
                                      new Vector2(cardW, cardH));
            var ct = card.transform;

            UIHelper.Portrait(ct, agents[i].portrait, new Vector2(-210, 96), new Vector2(56, 56));
            UIHelper.Label(ct, agents[i].agentName, 17, UIHelper.ColText,
                           new Vector2(40, 96), new Vector2(400, 26), TextAnchor.MiddleLeft, FontStyle.Bold);
            UIHelper.Panel(ct, "Div", UIHelper.AccentBlue, new Vector2(0, 66), new Vector2(470, 2));

            for (int j = 0; j < 5; j++)
            {
                float y = rowYs[j];
                int ai = i, aj = j;

                UIHelper.Label(ct, SkillSet.SkillAbbreviations[j], 15, UIHelper.ColSubtext,
                               new Vector2(-205, y), new Vector2(60, 24), TextAnchor.MiddleLeft);
                _lblValue[i, j] = UIHelper.Label(ct, "Lv 0", 15, Color.white,
                               new Vector2(-125, y), new Vector2(90, 24), TextAnchor.MiddleLeft);
                _lblCost[i, j] = UIHelper.Label(ct, "0 g", 15, UIHelper.ColWarn,
                               new Vector2(-5, y), new Vector2(110, 24), TextAnchor.MiddleLeft);

                // Minus (undo) — appears once you've upgraded this skill this session
                var minus = UIHelper.Btn(ct, "-", new Vector2(120, y), new Vector2(50, 26),
                                         UIHelper.ColDisabled, 22);
                minus.onClick.AddListener(() => Refund(ai, aj));
                minus.gameObject.SetActive(false);
                _btnMinus[i, j] = minus;

                var plus = UIHelper.Btn(ct, "+", new Vector2(186, y), new Vector2(50, 26),
                                        UIHelper.AccentBlue, 22);
                plus.onClick.AddListener(() => Purchase(ai, aj));
                _btnPlus[i, j] = plus;
            }
        }

        var btnNext = UIHelper.Btn(t, "Next Round  →", new Vector2(0, -430),
                                   new Vector2(260, 56), UIHelper.AccentBlue, 22);
        btnNext.onClick.AddListener(() => { _root.SetActive(false); OnNextRound?.Invoke(); });
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void Show()
    {
        // Snapshot the round-end levels — the floor the undo (−) can revert to.
        for (int i = 0; i < _agents.Length; i++)
        {
            var vals = _agents[i].skills.ToArray();
            for (int j = 0; j < 5; j++)
                _sessionBase[i, j] = Mathf.RoundToInt(vals[j]);
        }

        RefreshAll();
        _root.SetActive(true);
        _root.transform.SetAsLastSibling();
    }

    public void Hide() => _root.SetActive(false);

    // ── Private ───────────────────────────────────────────────────────────────
    private void Purchase(int i, int j)
    {
        var cfg = GameManager.Instance.Config;
        int level = Mathf.RoundToInt(_agents[i].skills.ToArray()[j]);
        if (level >= 10) return;

        int cost = cfg.skillUpgradeCostBase + level * cfg.skillUpgradeCostRamp;
        if (!GameManager.Instance.TrySpendGold(cost)) return;

        _agents[i].skills = _agents[i].skills.WithIncremented(j, 1f);
        RefreshAll();
    }

    private void Refund(int i, int j)
    {
        var cfg = GameManager.Instance.Config;
        int level = Mathf.RoundToInt(_agents[i].skills.ToArray()[j]);
        if (level <= _sessionBase[i, j]) return; // can't undo below this round's start

        // Refund exactly what was paid to reach the current level (from level-1).
        int refund = cfg.skillUpgradeCostBase + (level - 1) * cfg.skillUpgradeCostRamp;
        GameManager.Instance.RefundGold(refund);

        _agents[i].skills = _agents[i].skills.WithIncremented(j, -1f);
        RefreshAll();
    }

    private void RefreshAll()
    {
        var gm  = GameManager.Instance;
        var cfg = gm.Config;
        _lblGold.text = $"Gold:  {gm.TotalGold}";

        for (int i = 0; i < _agents.Length; i++)
        {
            var vals = _agents[i].skills.ToArray();
            for (int j = 0; j < 5; j++)
            {
                int level = Mathf.RoundToInt(vals[j]);
                _lblValue[i, j].text = $"Lv {level}";

                if (level >= 10)
                {
                    _lblCost[i, j].text        = "MAX";
                    _lblCost[i, j].color       = UIHelper.ColSubtext;
                    _btnPlus[i, j].interactable = false;
                }
                else
                {
                    int  cost   = cfg.skillUpgradeCostBase + level * cfg.skillUpgradeCostRamp;
                    bool afford = gm.TotalGold >= cost;
                    _lblCost[i, j].text        = $"{cost} g";
                    _lblCost[i, j].color       = afford ? UIHelper.ColWarn : UIHelper.ColFail;
                    _btnPlus[i, j].interactable = afford;
                }

                // Undo button visible only if this skill was raised during this session.
                _btnMinus[i, j].gameObject.SetActive(level > _sessionBase[i, j]);
            }
        }
    }
}
