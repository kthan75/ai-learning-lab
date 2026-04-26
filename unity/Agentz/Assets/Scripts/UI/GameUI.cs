using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    private Transform        _canvasRoot;
    private bool             _popupOpen;

    // ── Layout constants ─────────────────────────────────────────────────────
    const float CardW = 430f, CardH = 195f, CardGapX = 20f, CardGapY = 16f;

    // ── Lifecycle ────────────────────────────────────────────────────────────
    public void Initialize(AgentData[] agents)
    {
        Agents = agents;
        BuildCanvas();
        BuildMissionBoard();
        BuildSubPanels();
        WireEvents();
    }

    // ── Canvas + EventSystem ─────────────────────────────────────────────────
    private void BuildCanvas()
    {
        // EventSystem (needed for UI clicks)
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem",
                                    typeof(EventSystem),
                                    typeof(StandaloneInputModule));
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

        // Dark background panel
        UIHelper.PanelStretch(_canvasRoot, "Background", UIHelper.BgDark);
    }

    // ── Mission board (2×2 grid) ─────────────────────────────────────────────
    private void BuildMissionBoard()
    {
        _slots = new MissionSlotUI[4];

        // Positions: 2 columns, 2 rows, centered vertically between HUD and roster
        float[] xs = { -(CardW / 2f + CardGapX / 2f), (CardW / 2f + CardGapX / 2f) };
        float[] ys = { CardH / 2f + CardGapY / 2f + 5f, -(CardH / 2f + CardGapY / 2f) + 5f };

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

        // Assignment popup
        var apGo = new GameObject("AssignmentPopup", typeof(RectTransform));
        apGo.transform.SetParent(_canvasRoot, false);
        _assignPopup = apGo.AddComponent<AssignmentPopup>();
        _assignPopup.Build(_canvasRoot);
        _assignPopup.OnAssigned  += OnAssigned;
        _assignPopup.OnCancelled += () => _popupOpen = false;

        // Result popup
        var rpGo = new GameObject("ResultPopup", typeof(RectTransform));
        rpGo.transform.SetParent(_canvasRoot, false);
        _resultPopup = rpGo.AddComponent<ResultPopup>();
        _resultPopup.Build(_canvasRoot);

        // Round over panel
        var roGo = new GameObject("RoundOverPanel", typeof(RectTransform));
        roGo.transform.SetParent(_canvasRoot, false);
        _roundOver = roGo.AddComponent<RoundOverPanel>();
        _roundOver.Build(_canvasRoot);
        _roundOver.OnRestart += OnRestart;
    }

    // ── Event wiring ─────────────────────────────────────────────────────────
    private void WireEvents()
    {
        var gm = GameManager.Instance;
        var ms = MissionSpawner.Instance;

        gm.OnTimerTick    += t => _hud.Refresh(t, gm.Failures, gm.Gold, gm.Score, gm.RoundNumber);
        gm.OnFailureAdded += _ => _hud.Refresh(gm.RoundTimeRemaining, gm.Failures,
                                               gm.Gold, gm.Score, gm.RoundNumber);
        gm.OnGoldChanged  += _ => _hud.Refresh(gm.RoundTimeRemaining, gm.Failures,
                                               gm.Gold, gm.Score, gm.RoundNumber);
        gm.OnRoundEnd += () =>
        {
            _assignPopup.Hide();
            _roundOver.Show(gm.RoundNumber, gm.MissionsCompleted, gm.Gold, gm.Score);
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
            if (!_missionToSlot.ContainsValue(slot))
            {
                _missionToSlot[m] = slot;
                slot.SetMission(m);
                return;
            }
        }
    }

    private void OnMissionExpired(Mission m)
    {
        if (_missionToSlot.TryGetValue(m, out var slot))
        {
            slot.Clear();
            _missionToSlot.Remove(m);
        }
    }

    private void OnMissionResolved(Mission m, bool success, float overlap, int roll)
    {
        _resultPopup.Enqueue(m, success, overlap, roll);
        _roster.RefreshAll();

        if (_missionToSlot.TryGetValue(m, out var slot))
        {
            // Slot will self-clear after its flash timer
            _missionToSlot.Remove(m);
        }
    }

    // ── Interaction handlers ──────────────────────────────────────────────────
    private void OnMissionCardClicked(Mission m)
    {
        if (_popupOpen) return;
        if (m.State != Mission.MissionState.Waiting) return;
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        _popupOpen = true;
        _assignPopup.Show(m, Agents);
    }

    private void OnAssigned(Mission mission, List<AgentData> agents)
    {
        _popupOpen = false;
        MissionSpawner.Instance.AssignAgents(mission, agents);
        _roster.RefreshAll();

        // Update the slot so it shows Busy immediately
        if (_missionToSlot.TryGetValue(mission, out var slot))
            slot.SetMission(mission);
    }

    private void OnRestart()
    {
        _roundOver.Hide();
        _missionToSlot.Clear();
        foreach (var s in _slots) s.Clear();
        GameManager.Instance.RestartGame();
        _roster.RefreshAll();
        _hud.Refresh(GameManager.Instance.Config.roundDuration,
                     0, 0, 0, GameManager.Instance.RoundNumber);
    }
}
