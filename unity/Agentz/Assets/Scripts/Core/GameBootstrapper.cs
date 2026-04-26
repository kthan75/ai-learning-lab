using UnityEngine;

/// <summary>
/// Entry point for the game scene.
/// Holds references to the GameConfig and AgentData assets,
/// and will wire up all game systems in M1.
///
/// Attach this MonoBehaviour to the "GameBootstrapper" GameObject in GameScene.
/// Drag the config and agent assets into the inspector slots.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("Configuration")]
    public GameConfig config;

    [Header("Agent Roster (assign 6 AgentData assets)")]
    public AgentData[] agents = new AgentData[6];

    [Header("Mission Pool (assign MissionTemplate assets)")]
    public MissionTemplate[] missionPool;

    private void Awake()
    {
        ValidateSetup();
        // M1: initialize GameManager, MissionSpawner, UIManager etc. here
    }

    private void ValidateSetup()
    {
        if (config == null)
            Debug.LogError("[GameBootstrapper] GameConfig is not assigned!", this);

        if (agents == null || agents.Length == 0)
            Debug.LogWarning("[GameBootstrapper] No agents assigned. " +
                             "Create AgentData assets and assign them.", this);

        if (missionPool == null || missionPool.Length == 0)
            Debug.LogWarning("[GameBootstrapper] No mission templates assigned. " +
                             "Create MissionTemplate assets and assign them.", this);

        // Reset agent availability at scene start
        if (agents != null)
            foreach (var agent in agents)
                if (agent != null)
                    agent.isAvailable = true;

        Debug.Log("[GameBootstrapper] Setup OK — ready for M1 wiring.");
    }
}
