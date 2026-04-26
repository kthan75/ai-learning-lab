using UnityEngine;

/// <summary>
/// Scene entry point. Creates and initializes all game systems:
/// GameManager, MissionSpawner, and GameUI.
///
/// Inspector slots (already wired from M0):
///   - config      : GameConfig asset
///   - agents      : 6 AgentData assets
///   - missionPool : (optional) MissionTemplate assets — built-in pool used if empty
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("Configuration")]
    public GameConfig config;

    [Header("Agent Roster (6 AgentData assets)")]
    public AgentData[] agents = new AgentData[6];

    [Header("Mission Pool (optional — built-in pool used if empty)")]
    public MissionTemplate[] missionPool;

    private void Awake()
    {
        if (config == null)
        {
            Debug.LogError("[GameBootstrapper] GameConfig is not assigned!", this);
            return;
        }

        // Reset agent availability
        foreach (var a in agents)
            if (a != null) a.isAvailable = true;

        // ── GameManager ──────────────────────────────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gm   = gmGo.AddComponent<GameManager>();
        gm.Initialize(config);

        // ── MissionSpawner ───────────────────────────────────────────────────
        var msGo = new GameObject("MissionSpawner");
        var ms   = msGo.AddComponent<MissionSpawner>();
        ms.Initialize(config, missionPool);

        // ── GameUI ────────────────────────────────────────────────────────────
        var uiGo = new GameObject("GameUI");
        var ui   = uiGo.AddComponent<GameUI>();
        ui.Initialize(agents);

        Debug.Log("[GameBootstrapper] M1 systems initialized.");
    }
}
