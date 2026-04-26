using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One of the 4 mission card slots on the board.
/// Displays mission state: empty / waiting / busy / resolved flash.
/// </summary>
public class MissionSlotUI : MonoBehaviour
{
    public event Action<Mission> OnClicked;

    private Mission _mission;
    private Button  _btn;
    private Text    _lblTitle, _lblStatus, _lblTimer;
    private Image   _timerFill, _background;
    private GameObject _emptyOverlay, _activeOverlay;

    // Skill bar fills (5 bars)
    private Image[] _skillFills = new Image[5];

    private float _flashTimer;     // shows resolved state briefly
    private const float FlashDuration = 1.8f;

    // ── Build ────────────────────────────────────────────────────────────────
    public void Build(Transform parent, Vector2 pos, Vector2 size)
    {
        var root = UIHelper.Panel(parent, "MissionSlot", UIHelper.BgCard, pos, size);
        root.transform.SetParent(parent, false);
        UIHelper.SetRect(root, pos, size);
        _background = root.GetComponent<Image>();

        _btn = root.AddComponent<Button>();
        _btn.onClick.AddListener(() => { if (_mission != null) OnClicked?.Invoke(_mission); });

        var t = root.transform;

        // ── Empty state ──────────────────────────────────────────────────────
        _emptyOverlay = new GameObject("Empty", typeof(RectTransform));
        _emptyOverlay.transform.SetParent(t, false);
        var eRt = _emptyOverlay.GetComponent<RectTransform>();
        eRt.anchorMin = Vector2.zero; eRt.anchorMax = Vector2.one;
        eRt.offsetMin = eRt.offsetMax = Vector2.zero;
        UIHelper.LabelStretch(_emptyOverlay.transform, "— no mission —",
                               18, UIHelper.ColDisabled);

        // ── Active state ─────────────────────────────────────────────────────
        _activeOverlay = new GameObject("Active", typeof(RectTransform));
        _activeOverlay.transform.SetParent(t, false);
        var aRt = _activeOverlay.GetComponent<RectTransform>();
        aRt.anchorMin = Vector2.zero; aRt.anchorMax = Vector2.one;
        aRt.offsetMin = new Vector2(8, 8); aRt.offsetMax = new Vector2(-8, -8);
        var at = _activeOverlay.transform;

        _lblTitle  = UIHelper.Label(at, "Mission Title", 20, UIHelper.ColText,
                                    new Vector2(0,  55), new Vector2(size.x - 20, 40),
                                    TextAnchor.UpperLeft, FontStyle.Bold);
        _lblStatus = UIHelper.Label(at, "Click to Assign", 16, UIHelper.ColSubtext,
                                    new Vector2(0, -55), new Vector2(size.x - 20, 30),
                                    TextAnchor.LowerCenter);

        // Timer bar
        float bw = size.x - 20;
        var (_, fill) = UIHelper.ProgressBar(at,
                                              new Vector2(0, 28), new Vector2(bw, 10),
                                              UIHelper.ColDisabled, UIHelper.AccentBlue);
        _timerFill = fill;

        _lblTimer = UIHelper.Label(at, "20s", 14, UIHelper.ColSubtext,
                                   new Vector2(bw / 2 - 20, 28), new Vector2(40, 20));

        // Skill bars (5 mini bars)
        for (int i = 0; i < 5; i++)
        {
            float bwSingle = (bw - 4 * 6) / 5f;
            float xOff = -bw / 2f + i * (bwSingle + 6) + bwSingle / 2f;
            var (_, sf) = UIHelper.ProgressBar(at,
                                                new Vector2(xOff, 5), new Vector2(bwSingle, 8),
                                                UIHelper.ColDisabled, UIHelper.AccentBlue);
            _skillFills[i] = sf;

            UIHelper.Label(at, SkillSet.SkillNames[i].Substring(0, 3),
                           11, UIHelper.ColSubtext,
                           new Vector2(xOff, -5), new Vector2(bwSingle, 14));
        }

        ShowEmpty();
    }

    // ── Public API ───────────────────────────────────────────────────────────
    public void SetMission(Mission mission)
    {
        _mission = mission;
        _flashTimer = 0f;
        RefreshVisuals();
    }

    public void Clear()
    {
        _mission = null;
        _flashTimer = 0f;
        ShowEmpty();
    }

    private void Update()
    {
        if (_mission == null) return;

        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f) Clear();
            return;
        }

        RefreshVisuals();
    }

    // ── Private ──────────────────────────────────────────────────────────────
    private void RefreshVisuals()
    {
        if (_mission == null) { ShowEmpty(); return; }

        _emptyOverlay.SetActive(false);
        _activeOverlay.SetActive(true);
        _lblTitle.text = _mission.Template.missionTitle;

        var req = _mission.Template.requiredSkills;
        var arr = req.ToArray();
        for (int i = 0; i < 5; i++)
            _skillFills[i].fillAmount = arr[i] / 10f;

        switch (_mission.State)
        {
            case Mission.MissionState.Waiting:
                float frac = _mission.TimeFraction;
                _timerFill.fillAmount = frac;
                _timerFill.color      = frac > 0.5f ? UIHelper.AccentBlue
                                      : frac > 0.25f ? UIHelper.ColWarn
                                      : UIHelper.ColFail;
                int secs = Mathf.CeilToInt(_mission.TimeRemaining);
                _lblTimer.text   = $"{secs}s";
                _lblStatus.text  = "[ Click to Assign ]";
                _lblStatus.color = UIHelper.ColSubtext;
                _background.color = UIHelper.BgCard;
                break;

            case Mission.MissionState.Busy:
                _timerFill.fillAmount = 0f;
                _lblTimer.text   = "...";
                _lblStatus.text  = "In Progress";
                _lblStatus.color = UIHelper.ColWarn;
                _background.color = new Color(0.12f, 0.10f, 0.05f);
                break;

            case Mission.MissionState.Resolved:
                _timerFill.fillAmount = 1f;
                bool ok = _mission.WasSuccess;
                _lblStatus.text  = ok ? "✓  SUCCESS" : "✗  FAILED";
                _lblStatus.color = ok ? UIHelper.ColSuccess : UIHelper.ColFail;
                _background.color = ok ? new Color(0.05f, 0.14f, 0.07f)
                                       : new Color(0.15f, 0.05f, 0.05f);
                _lblTimer.text = "";
                if (_flashTimer <= 0f) _flashTimer = FlashDuration;
                break;
        }
    }

    private void ShowEmpty()
    {
        _emptyOverlay.SetActive(true);
        _activeOverlay.SetActive(false);
        _background.color = new Color(0.07f, 0.07f, 0.16f);
    }
}
