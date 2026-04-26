using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom strip showing all 6 agents with availability state and skill values.
/// Read-only in gameplay — selection happens inside AssignmentPopup.
/// </summary>
public class AgentRosterPanel : MonoBehaviour
{
    private AgentData[] _agents;
    private Image[]     _cardBgs;
    private Text[]      _nameLabels;
    private Text[]      _skillLabels;
    private Text[]      _statusLabels;

    // Same amber as AssignmentPopup
    private static readonly Color ColSkillValue = new Color(0.91f, 0.78f, 0.29f);

    public void Build(Transform canvasRoot, AgentData[] agents)
    {
        _agents       = agents;
        _cardBgs      = new Image[agents.Length];
        _nameLabels   = new Text[agents.Length];
        _skillLabels  = new Text[agents.Length];
        _statusLabels = new Text[agents.Length];

        // Taller strip to fit skills
        var bg = UIHelper.PanelStretch(canvasRoot, "AgentRoster", UIHelper.BgCard);
        UIHelper.AnchorBottomStretch(bg.GetComponent<RectTransform>(), height: 130);

        UIHelper.Label(bg.transform, "DEPLOYED\nAGENTS", 15, UIHelper.ColText,
                       new Vector2(-860, 0), new Vector2(110, 130),
                       TextAnchor.MiddleCenter, FontStyle.Bold);

        float cardW  = 220f;
        float cardH  = 100f;
        float gap    = 10f;
        float totalW = agents.Length * cardW + (agents.Length - 1) * gap;
        float startX = -totalW / 2f + cardW / 2f;

        for (int i = 0; i < agents.Length; i++)
        {
            float x = startX + i * (cardW + gap);

            var card = UIHelper.Panel(bg.transform, "Agent_" + i,
                                      UIHelper.BgCard, new Vector2(x, -2), new Vector2(cardW, cardH));
            _cardBgs[i] = card.GetComponent<Image>();
            var ct = card.transform;

            // Agent name — upper section
            _nameLabels[i] = UIHelper.Label(ct, agents[i].agentName, 15, UIHelper.ColText,
                                             new Vector2(0, 26), new Vector2(cardW - 12, 26),
                                             TextAnchor.MiddleCenter, FontStyle.Bold);

            // Skill line — middle
            _skillLabels[i] = UIHelper.Label(ct, "", 12, Color.white,
                                              new Vector2(0, 0), new Vector2(cardW - 12, 22),
                                              TextAnchor.MiddleCenter);

            // Status — bottom
            _statusLabels[i] = UIHelper.Label(ct, "Available", 12, UIHelper.ColSuccess,
                                               new Vector2(0, -24), new Vector2(cardW - 12, 20),
                                               TextAnchor.MiddleCenter);
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (_agents == null) return;
        for (int i = 0; i < _agents.Length; i++)
        {
            bool avail = _agents[i].isAvailable;

            _cardBgs[i].color    = avail ? UIHelper.BgCard : new Color(0.10f, 0.10f, 0.14f);
            _nameLabels[i].color = avail ? UIHelper.ColText : UIHelper.ColSubtext;

            // Skill line with rich text coloring
            var s = _agents[i].skills;
            string busyGrey = "464646";
            string valHex   = avail ? ColorUtility.ToHtmlStringRGB(ColSkillValue) : busyGrey;
            string lblHex   = avail ? ColorUtility.ToHtmlStringRGB(UIHelper.ColSubtext) : busyGrey;

            _skillLabels[i].text =
                $"<color=#{lblHex}>ENG:</color><color=#{valHex}>{s.engineering:0}</color> " +
                $"<color=#{lblHex}>DIP:</color><color=#{valHex}>{s.diplomacy:0}</color> " +
                $"<color=#{lblHex}>NAV:</color><color=#{valHex}>{s.navigation:0}</color> " +
                $"<color=#{lblHex}>SSM:</color><color=#{valHex}>{s.streetSmarts:0}</color> " +
                $"<color=#{lblHex}>RES:</color><color=#{valHex}>{s.resilience:0}</color>";

            _statusLabels[i].text  = avail ? "Available" : "On Mission";
            _statusLabels[i].color = avail ? UIHelper.ColSuccess : UIHelper.ColWarn;
        }
    }
}
