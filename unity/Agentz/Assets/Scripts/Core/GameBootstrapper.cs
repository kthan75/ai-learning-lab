using UnityEngine;

/// <summary>
/// Scene entry point. Creates and initializes all game systems:
/// GameManager, MissionSpawner, and GameUI.
///
/// Inspector slots (already wired from M0):
///   - config      : GameConfig asset
///   - agents      : 6 AgentData assets
///   - missionPool : (optional) MissionTemplate assets — built-in pool used if empty
///
/// Plain-text overrides (Model B — see docs/EDITING_GUIDE.md): at boot, if
/// StreamingAssets/config.json or agents.csv exist, they override the Inspector
/// config / roster for this run without mutating the underlying assets.
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

        // ── Plain-text overrides (config.json / agents.csv) ───────────────────
        var runtimeConfig = ConfigLoader.LoadOverride(config);
        var roster        = AgentLoader.LoadRoster(agents);

        // Reset agent availability on whichever roster we ended up with
        foreach (var a in roster)
            if (a != null) a.isAvailable = true;

        // ── GameManager ──────────────────────────────────────────────────────
        var gmGo = new GameObject("GameManager");
        var gm   = gmGo.AddComponent<GameManager>();
        gm.Initialize(runtimeConfig);

        // ── MissionSpawner ───────────────────────────────────────────────────
        var msGo = new GameObject("MissionSpawner");
        var ms   = msGo.AddComponent<MissionSpawner>();
        ms.Initialize(runtimeConfig, missionPool);

        // ── GameUI ────────────────────────────────────────────────────────────
        var uiGo = new GameObject("GameUI");
        var ui   = uiGo.AddComponent<GameUI>();
        ui.Initialize(roster);

        Debug.Log("[GameBootstrapper] M1 systems initialized.");
    }
}
