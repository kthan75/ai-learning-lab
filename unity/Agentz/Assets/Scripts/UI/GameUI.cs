using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Builds the entire game UI at runtime and wires it to GameManager + MissionSpawner.
/// Attach to any GameObject in the scene — it creates its own Canvas and EventSystem.
/// </summary>
public class GameUI : MonoBehaviour
{
    // ── References set by GameBootstrapper ───────────────────────────────────
    [HideInInspector] public AgentData[] Agents;

    // ── Sub-panels ───────────────────────────────────────────────────────────
    private HUDPanel         _hud;
    private AgentRosterPanel _roster;
    private MissionSlotUI[]  _slots;
    private AssignmentPopup  _assignPopup;
    private ResultPopup      _resultPopup;
    private RoundOverPanel   _roundOver;
    private UpgradePanel     _upgrade;
    private IncapacitationManager _incap;
    private IncapacitationPopup   _incapPopup;
    private IntroScreens          _intro;

    private SkillSet[]       _baseSkills; // snapshot for resetting upgrades on a new run

    private Transform        _canvasRoot;
    private bool             _popupOpen;
    private bool             _resultOpen;
    private bool             _incapOpen;
    private bool             _introOpen;
    private bool             _roundEndPending;

    // ── Layout constants ─────────────────────────────────────────────────────
    const float CardW = 430f, CardH = 195f, CardGapX = 20f, CardGapY = 16f;

    // ── Lifecycle ────────────────────────────────────────────────────────────
    public void Initialize(AgentData[] agents)
    {
        Agents = agents;

        // Snapshot base skills so upgrades can be reset when a run ends (Play Again).
        _baseSkills = new SkillSet[agents.Length];
        for (int i = 0; i < agents.Length; i++)
            if (agents[i] != null) _baseSkills[i] = agents[i].skills;

        BuildCanvas();
        BuildMissionBoard();
        BuildSubPanels();
        WireEvents();
        ShowIntro();
    }

    // ── Intro screens (once per launch, before round 1) ──────────────────────
    private void ShowIntro()
    {
        var introGo = new GameObject("IntroScreens", typeof(RectTransform));
        introGo.transform.SetParent(_canvasRoot, false);
        _intro = introGo.AddComponent<IntroScreens>();
        _intro.Build(_canvasRoot);
        _intro.OnComplete += () => { _introOpen = false; RefreshPause(); };

        _introOpen = true;      // freeze the game until the intro is dismissed
        RefreshPause();
        _intro.Show();
    }

    // ── Canvas + EventSystem ─────────────────────────────────────────────────
    private void BuildCanvas()
    {
        // EventSystem (needed for UI clicks)
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        var canvasGo = new GameObject("GameCanvas",
                                       typeof(RectTransform),
                                       typeof(Canvas),
                                       typeof(CanvasScaler),
                                       typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        _canvasRoot = canvasGo.transform;

        // Background — dispatch frame art if available, else a flat dark panel
        var bg = UIHelper.PanelStretch(_canvasRoot, "Background", UIHelper.BgDark);
        var bgSprite = ArtLoader.Load("dispatch_bg.png");
        if (bgSprite != null)
        {
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.color  = Color.white;
        }
    }

    // ── Mission board (2×2 grid) ─────────────────────────────────────────────
    private void BuildMissionBoard()
    {
        _slots = new MissionSlotUI[4];

        // Positions: 2 columns, 2 rows, centred in the frame's display area (measured
        // centre ~+55 in canvas units — more headroom below than above at +5).
        const float boardY = 55f;
        float[] xs = { -(CardW / 2f + CardGapX / 2f), (CardW / 2f + CardGapX / 2f) };
        float[] ys = { CardH / 2f + CardGapY / 2f + boardY, -(CardH / 2f + CardGapY / 2f) + boardY };

        for (int i = 0; i < 4; i++)
        {
            int row = i / 2, col = i % 2;
            var slotGo = new GameObject($"Slot_{i}", typeof(RectTransform));
            slotGo.transform.SetParent(_canvasRoot, false);

            var rt = slotGo.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(xs[col], ys[row]);
            rt.sizeDelta = new Vector2(CardW, CardH);

            var slot = slotGo.AddComponent<MissionSlotUI>();
            slot.Build(_canvasRoot, new Vector2(xs[col], ys[row]), new Vector2(CardW, CardH));
            slot.OnClicked += OnMissionCardClicked;
            _slots[i] = slot;
        }
    }

    // ── Sub-panels ────────────────────────────────────────────────────────────
    private void BuildSubPanels()
    {
        // HUD
        var hudGo = new GameObject("HUDPanel", typeof(RectTransform));
        hudGo.transform.SetParent(_canvasRoot, false);
        _hud = hudGo.AddComponent<HUDPanel>();
        _hud.Build(_canvasRoot, GameManager.Instance.Config);

        // Agent roster
        var rosterGo = new GameObject("AgentRoster", typeof(RectTransform));
        rosterGo.transform.SetParent(_canvasRoot, false);
        _roster = rosterGo.AddComponent<AgentRosterPanel>();
        _roster.Build(_canvasRoot, Agents);

        // Incapacitation event popup
        var ipGo = new GameObject("IncapacitationPopup", typeof(RectTransform));
        ipGo.transform.SetParent(_canvasRoot, false);
        _incapPopup = ipGo.AddComponent<IncapacitationPopup>();
        _incapPopup.Build(_canvasRoot);
        _incapPopup.OnDismissed += () => { _incapOpen = false; RefreshPause(); };

        // Incapacitation manager — random idle-agent downtime
        var incapGo = new GameObject("IncapacitationManager");
        _incap = incapGo.AddComponent<IncapacitationManager>();
        _incap.Initialize(Agents);
        _incap.OnChanged += () => _roster.RefreshAll();
        _incap.OnIncapacitated += agent =>
        {
            _incapOpen = true;
            RefreshPause();
            _incapPopup.Show(agent, agent.incapTimeRemaining);
        };

        // Assignment popup
        var apGo = new GameObject("AssignmentPopup", typeof(RectTransform));
        apGo.transform.SetParent(_canvasRoot, false);
        _assignPopup = apGo.AddComponent<AssignmentPopup>();
        _assignPopup.Build(_canvasRoot);
        _assignPopup.OnAssigned  += OnAssigned;
        _assignPopup.OnCancelled += () => { _popupOpen = false; RefreshPause(); _roster.SetVisible(true); };

        // Result popup
        var rpGo = new GameObject("ResultPopup", typeof(RectTransform));
        rpGo.transform.SetParent(_canvasRoot, false);
        _resultPopup = rpGo.AddComponent<ResultPopup>();
        _resultPopup.Build(_canvasRoot);
        _resultPopup.OnDismissed += () =>
        {
            _resultOpen = _resultPopup.HasQueued;
            RefreshPause();
            if (!_resultOpen && _roundEndPending)
            {
                _roundEndPending = false;
                ShowRoundOver();
            }
        };

        // Upgrade screen (between rounds)
        var upGo = new GameObject("UpgradePanel", typeof(RectTransform));
        upGo.transform.SetParent(_canvasRoot, false);
        _upgrade = upGo.AddComponent<UpgradePanel>();
        _upgrade.Build(_canvasRoot, Agents);
        _upgrade.OnNextRound += OnNextRound;

        // Round over panel
        var roGo = new GameObject("RoundOverPanel", typeof(RectTransform));
        roGo.transform.SetParent(_canvasRoot, false);
        _roundOver = roGo.AddComponent<RoundOverPanel>();
        _roundOver.Build(_canvasRoot);
        _roundOver.OnUpgrade += () => { _roundOver.Hide(); _upgrade.Show(); }; // success → upgrade screen
        _roundOver.OnRestart += OnRestart;
        _roundOver.OnQuit    += () => Application.Quit();
    }

    // ── Event wiring ─────────────────────────────────────────────────────────
    private void WireEvents()
    {
        var gm = GameManager.Instance;
        var ms = MissionSpawner.Instance;

        gm.OnTimerTick    += t => _hud.Refresh(t, gm.Failures, gm.Gold, gm.TotalGold, gm.Score, gm.TotalScore, gm.RoundNumber);
        gm.OnFailureAdded += _ => _hud.Refresh(gm.RoundTimeRemaining, gm.Failures,
                                               gm.Gold, gm.TotalGold, gm.Score, gm.TotalScore, gm.RoundNumber);
        gm.OnGoldChanged  += _ => _hud.Refresh(gm.RoundTimeRemaining, gm.Failures,
                                               gm.Gold, gm.TotalGold, gm.Score, gm.TotalScore, gm.RoundNumber);
        gm.OnRoundEnd += () =>
        {
            _assignPopup.Hide();
            if (_resultOpen)
                _roundEndPending = true;   // defer until last result popup is dismissed
            else
                ShowRoundOver();
        };

        ms.OnMissionSpawned  += OnMissionSpawned;
        ms.OnMissionExpired  += OnMissionExpired;
        ms.OnMissionResolved += OnMissionResolved;
    }

    // ── Mission board management ──────────────────────────────────────────────
    private readonly Dictionary<Mission, MissionSlotUI> _missionToSlot =
        new Dictionary<Mission, MissionSlotUI>();

    private void OnMissionSpawned(Mission m)
    {
        foreach (var slot in _slots)
        {
            // Skip slots still showing a result flash (HasMission) as well as mapped ones.
            if (!slot.HasMission && !_missionToSlot.ContainsValue(slot))
            {
                _missionToSlot[m] = slot;
                slot.SetMission(m);
                return;
            }
        }
    }

    private void OnMissionExpired(Mission m)
    {
        if (!_missionToSlot.TryGetValue(m, out var slot)) return;
        _missionToSlot.Remove(m);

        // A real timeout has already flipped the mission to Resolved(failure): leave the
        // slot to flash its FAILED state and self-clear — same as a skill failure, but with
        // no result popup. Round-end draining removes still-Waiting missions silently.
        if (m.State != Mission.MissionState.Resolved)
            slot.Clear();
    }

    private void OnMissionResolved(Mission m, bool success, float overlap, int roll)
    {
        _resultOpen = true;
        RefreshPause();
        _resultPopup.Enqueue(m, success, overlap, roll);
        _roster.RefreshAll();

        if (_missionToSlot.TryGetValue(m, out var slot))
        {
            // Slot will self-clear after its flash timer
            _missionToSlot.Remove(m);
        }
    }

    // ── Pause helper / Interaction handlers ──────────────────────────────────
    /// <summary>Freeze mission timers whenever any popup is blocking the board.</summary>
    private void ShowRoundOver()
    {
        var gm = GameManager.Instance;
        _roundOver.Show(gm.RoundNumber, gm.MissionsCompleted,
                        gm.Gold, gm.TotalGold, gm.Score, gm.TotalScore,
                        gm.RoundWasFailure);
    }

    private void RefreshPause()
    {
        MissionSpawner.Instance.PauseMissions = _popupOpen || _resultOpen || _incapOpen || _introOpen;
    }

    private void OnMissionCardClicked(Mission m)
    {
        if (_popupOpen) return;
        if (m.State != Mission.MissionState.Waiting) return;
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        _popupOpen = true;
        RefreshPause();
        _roster.SetVisible(false); // redundant with the popup's own agent cards
        _assignPopup.Show(m, Agents);
    }

    private void OnAssigned(Mission mission, List<AgentData> agents)
    {
        _popupOpen = false;
        RefreshPause();
        _roster.SetVisible(true);
        MissionSpawner.Instance.AssignAgents(mission, agents);
        _roster.RefreshAll();

        // Update the slot so it shows Busy immediately
        if (_missionToSlot.TryGetValue(mission, out var slot))
            slot.SetMission(mission);
    }

    private void OnNextRound()
    {
        _roundOver.Hide();
        _upgrade.Hide();
        _roundEndPending = false;
        _resultOpen = false;
        _missionToSlot.Clear();
        foreach (var s in _slots) s.Clear();
        foreach (var a in Agents) if (a != null) a.isAvailable = true;
        GameManager.Instance.StartNextRound();
        _roster.RefreshAll();
        var gm2 = GameManager.Instance;
        _hud.Refresh(gm2.Config.roundDuration, 0,
                     gm2.Gold, gm2.TotalGold, gm2.Score, gm2.TotalScore, gm2.RoundNumber);
    }

    private void OnRestart()
    {
        _roundOver.Hide();
        _upgrade.Hide();
        _roundEndPending = false;
        _resultOpen = false;
        _missionToSlot.Clear();
        foreach (var s in _slots) s.Clear();

        // Fresh run: reset agents to base skills and availability.
        for (int i = 0; i < Agents.Length; i++)
        {
            if (Agents[i] == null) continue;
            Agents[i].skills      = _baseSkills[i];
            Agents[i].isAvailable = true;
        }

        GameManager.Instance.RestartGame();
        _roster.RefreshAll();
        _hud.Refresh(GameManager.Instance.Config.roundDuration,
                     0, 0, 0, 0, 0, GameManager.Instance.RoundNumber);
    }
}
