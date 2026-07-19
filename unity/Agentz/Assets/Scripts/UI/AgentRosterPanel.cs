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

        // Transparent strip matched to the frame's slot interior (flat dark area, measured
        // from dispatch_bg: canvas y -365..-480).
        _bgRoot = UIHelper.PanelStretch(canvasRoot, "AgentRoster", new Color(0f, 0f, 0f, 0f));
        UIHelper.AnchorBottomStretch(_bgRoot.GetComponent<RectTransform>(), height: 128, offsetY: 48);

        // Anchor each card to its slot as a FRACTION of the screen width (measured from
        // dispatch_bg). Screen-fraction anchoring tracks the stretched background at any
        // window resolution/aspect — absolute canvas positions drift (worse toward the edges).
        float[] slotFrac = { 0.105f, 0.260f, 0.416f, 0.573f, 0.728f, 0.884f };
        float cardW = 258f, cardH = 126f;

        for (int i = 0; i < agents.Length; i++)
        {
            var card = UIHelper.Panel(_bgRoot.transform, "Agent_" + i,
                                      new Color(0f, 0f, 0f, 0f), Vector2.zero, new Vector2(cardW, cardH));
            var rt = card.GetComponent<RectTransform>();
            float fx = slotFrac[Mathf.Min(i, slotFrac.Length - 1)];
            rt.anchorMin = new Vector2(fx, 0.5f);
            rt.anchorMax = new Vector2(fx, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(cardW, cardH);
            _cardBgs[i] = card.GetComponent<Image>();
            var ct = card.transform;

            // Name — top (inside the slot box)
            _nameLabels[i] = UIHelper.Label(ct, agents[i].agentName, 12, UIHelper.ColText,
                                            new Vector2(10, 56), new Vector2(cardW - 18, 18),
                                            TextAnchor.MiddleCenter, FontStyle.Bold);

            // Portrait (left) + mini chart (right). Shifted right of geometric centre for
            // visual balance (the solid portrait carries more weight than the sparse chart).
            _portraits[i] = UIHelper.Portrait(ct, agents[i].portrait,
                                              new Vector2(-54, 0), new Vector2(88, 88));
            _charts[i] = SpiderChart.Create(ct, new Vector2(60, -4), 84f,
                                            ChartLabelMode.NamesAndValues, 9);

            // Deployment status — bottom
            _statusLabels[i] = UIHelper.Label(ct, "Available", 11, UIHelper.ColSuccess,
                                              new Vector2(10, -54), new Vector2(cardW - 18, 15),
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

            // Transparent when available (frame slot shows); dark overlay when deployed/out.
            _cardBgs[i].color    = avail ? new Color(0f, 0f, 0f, 0f) : new Color(0f, 0f, 0f, 0.5f);
            _nameLabels[i].color = avail ? UIHelper.ColText : UIHelper.ColSubtext;

            UIHelper.SetPortrait(_portraits[i], _agents[i].portrait, tinted: !avail);
            _charts[i].SetSingle(_agents[i].skills, dimmed: !avail);

            if (_agents[i].isIncapacitated)
            {
                _statusLabels[i].text  = IncapStatusText(_agents[i]);
                _statusLabels[i].color = UIHelper.ColFail;
            }
            else
            {
                _statusLabels[i].text  = avail ? "Available" : "On Mission";
                _statusLabels[i].color = avail ? UIHelper.ColSuccess : UIHelper.ColWarn;
            }
        }
    }

    // Live-update the incapacitation countdown (cheap: text only, no chart rebuilds).
    private void Update()
    {
        if (_agents == null) return;
        for (int i = 0; i < _agents.Length; i++)
            if (_agents[i] != null && _agents[i].isIncapacitated)
                _statusLabels[i].text = IncapStatusText(_agents[i]);
    }

    private static string IncapStatusText(AgentData a)
        => $"{a.incapReason} ({Mathf.CeilToInt(Mathf.Max(0f, a.incapTimeRemaining))}s)";
}
