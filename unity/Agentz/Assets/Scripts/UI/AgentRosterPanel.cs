using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom strip showing all 6 agents with availability state.
/// Read-only in gameplay — selection happens inside AssignmentPopup.
/// </summary>
public class AgentRosterPanel : MonoBehaviour
{
    private AgentData[] _agents;
    private Image[]     _cardBgs;
    private Text[]      _cardLabels;

    public void Build(Transform canvasRoot, AgentData[] agents)
    {
        _agents     = agents;
        _cardBgs    = new Image[agents.Length];
        _cardLabels = new Text[agents.Length];

        var bg = UIHelper.PanelStretch(canvasRoot, "AgentRoster", UIHelper.BgCard);
        UIHelper.AnchorBottomStretch(bg.GetComponent<RectTransform>(), height: 110);

        UIHelper.Label(bg.transform, "AGENTS", 14, UIHelper.ColSubtext,
                       new Vector2(-860, 30), new Vector2(80, 25));

        float cardW  = 230f;
        float cardH  = 80f;
        float totalW = agents.Length * cardW + (agents.Length - 1) * 12f;
        float startX = -totalW / 2f + cardW / 2f;

        for (int i = 0; i < agents.Length; i++)
        {
            float x = startX + i * (cardW + 12f);
            var card = UIHelper.Panel(bg.transform, "Agent_" + i,
                                      UIHelper.BgCard, new Vector2(x, 0), new Vector2(cardW, cardH));

            _cardBgs[i] = card.GetComponent<Image>();

            _cardLabels[i] = UIHelper.LabelStretch(card.transform,
                                                    agents[i].agentName, 16, UIHelper.ColText,
                                                    TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (_agents == null) return;
        for (int i = 0; i < _agents.Length; i++)
        {
            bool avail = _agents[i].isAvailable;
            _cardBgs[i].color   = avail ? UIHelper.BgCard : UIHelper.ColDisabled;
            _cardLabels[i].color = avail ? UIHelper.ColText : UIHelper.ColSubtext;
        }
    }
}
