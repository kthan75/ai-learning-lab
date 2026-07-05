using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom strip showing all 6 agents — portrait, mini spider chart, and deployment
/// status. Read-only in gameplay (selection happens inside AssignmentPopup). Hidden
/// while the assignment popup is open (see <see cref="SetVisible"/>).
/// </summary>
public class AgentRosterPanel : MonoBehaviour
{
    private AgentData[]   _agents;
    private GameObject    _bgRoot;
    private Image[]       _cardBgs;
    private Text[]        _nameLabels;
    private Text[]        _statusLabels;
    private Image[]       _portraits;
    private SpiderChart[] _charts;

    public void Build(Transform canvasRoot, AgentData[] agents)
    {
        _agents       = agents;
        _cardBgs      = new Image[agents.Length];
        _nameLabels   = new Text[agents.Length];
        _statusLabels = new Text[agents.Length];
        _portraits    = new Image[agents.Length];
        _charts       = new SpiderChart[agents.Length];

        _bgRoot = UIHelper.PanelStretch(canvasRoot, "AgentRoster", UIHelper.BgCard);
        UIHelper.AnchorBottomStretch(_bgRoot.GetComponent<RectTransform>(), height: 190);

        UIHelper.Label(_bgRoot.transform, "DEPLOYED\nAGENTS", 15, UIHelper.ColText,
                       new Vector2(-845, 0), new Vector2(120, 160),
                       TextAnchor.MiddleCenter, FontStyle.Bold);

        float cardW  = 235f;
        float cardH  = 168f;
        float gap    = 10f;
        float totalW = agents.Length * cardW + (agents.Length - 1) * gap;
        float startX = -totalW / 2f + cardW / 2f;

        for (int i = 0; i < agents.Length; i++)
        {
            float x = startX + i * (cardW + gap);

            var card = UIHelper.Panel(_bgRoot.transform, "Agent_" + i,
                                      UIHelper.BgCard, new Vector2(x, 0), new Vector2(cardW, cardH));
            _cardBgs[i] = card.GetComponent<Image>();
            var ct = card.transform;

            // Name — top
            _nameLabels[i] = UIHelper.Label(ct, agents[i].agentName, 13, UIHelper.ColText,
                                            new Vector2(0, 68), new Vector2(cardW - 12, 22),
                                            TextAnchor.MiddleCenter, FontStyle.Bold);

            // Portrait (left) + mini chart (right)
            _portraits[i] = UIHelper.Portrait(ct, agents[i].portrait,
                                              new Vector2(-56, -4), new Vector2(88, 88));
            _charts[i] = SpiderChart.Create(ct, new Vector2(48, -4), 92f,
                                            ChartLabelMode.NamesAndValues, 9);

            // Deployment status — bottom
            _statusLabels[i] = UIHelper.Label(ct, "Available", 11, UIHelper.ColSuccess,
                                              new Vector2(0, -70), new Vector2(cardW - 12, 18),
                                              TextAnchor.MiddleCenter);
        }

        RefreshAll();
    }

    /// <summary>Show/hide the whole roster strip (hidden while the assign popup is open).</summary>
    public void SetVisible(bool visible)
    {
        if (_bgRoot != null) _bgRoot.SetActive(visible);
    }

    public void RefreshAll()
    {
        if (_agents == null) return;
        for (int i = 0; i < _agents.Length; i++)
        {
            bool avail = _agents[i].isAvailable;

            _cardBgs[i].color    = avail ? UIHelper.BgCard : new Color(0.10f, 0.10f, 0.14f);
            _nameLabels[i].color = avail ? UIHelper.ColText : UIHelper.ColSubtext;

            UIHelper.SetPortrait(_portraits[i], _agents[i].portrait, tinted: !avail);
            _charts[i].SetSingle(_agents[i].skills, dimmed: !avail);

            _statusLabels[i].text  = avail ? "Available" : "On Mission";
            _statusLabels[i].color = avail ? UIHelper.ColSuccess : UIHelper.ColWarn;
        }
    }
}
