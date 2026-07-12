using UnityEngine;

/// <summary>
/// All tunable game constants in one place.
/// Create one instance via: Assets > Create > Agentz > Game Config
/// Attach it to the GameBootstrapper in the scene.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Agentz/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Skill Combination")]
    [Tooltip("How multiple agents' skills are combined.\n• Average — simple mean (adding a weak agent can lower the score)\n• Additive — sum capped at 10 (more agents always helps)\n• Max — best value per axis (each agent covers their strongest skill)")]
    public SkillCombineMode skillCombineMode = SkillCombineMode.Max;

    [Header("Round")]
    [Tooltip("Duration of each round in seconds.")]
    public float roundDuration = 180f;

    [Tooltip("Number of failures that end the round.")]
    public int failureLimit = 4;

    [Header("Mission Spawning")]
    [Tooltip("Minimum seconds between mission spawns.")]
    public float missionSpawnIntervalMin = 3f;

    [Tooltip("Maximum seconds between mission spawns.")]
    public float missionSpawnIntervalMax = 6f;

    [Tooltip("Maximum number of missions active on the board at once.")]
    public int maxActiveMissions = 4;

    [Tooltip("Time (seconds) a mission stays open before expiring.")]
    public float missionExpiryDuration = 20f;

    [Header("Mission Resolution")]
    [Tooltip("Duration of the 'busy' phase after agents are assigned (seconds).")]
    public float missionBusyDuration = 7f;

    [Header("Economy")]
    [Tooltip("Base gold awarded for a successful mission.")]
    public int baseGoldReward = 10;

    [Tooltip("Bonus gold for completing a round without hitting the failure limit.")]
    public int roundCompletionBonus = 25;

    [Tooltip("Bonus gold multiplier applied after this many consecutive successes.")]
    public int streakBonusThreshold = 5;

    [Tooltip("Gold added per success beyond the streak threshold.")]
    public int streakBonusGold = 5;

    [Header("Difficulty Scaling")]
    [Tooltip("Each round, mission required skill values are multiplied by this factor (cumulative).")]
    [Range(1f, 1.5f)]
    public float skillScalePerRound = 1.05f;

    [Tooltip("Each round, the spawn interval is reduced by this fraction (minimum capped).")]
    [Range(0f, 0.2f)]
    public float spawnIntervalReductionPerRound = 0.03f;

    [Tooltip("Minimum allowed spawn interval regardless of difficulty scaling.")]
    public float minSpawnInterval = 1.5f;

    [Header("Random Events — Ops Info Incomplete (OII)")]
    [Tooltip("Enable OII: some missions spawn with no success-rate preview in the assign popup.")]
    public bool enableOII = true;

    [Tooltip("Base chance (0–1) that a mission is OII, at round 1.")]
    [Range(0f, 1f)] public float startingOccurrenceOII = 0.25f;

    [Tooltip("Added to the OII chance each round (cumulative).")]
    [Range(0f, 1f)] public float roundScalingOII = 0.05f;

    [Tooltip("Maximum OII chance regardless of round.")]
    [Range(0f, 1f)] public float maxOccurrenceOII = 0.75f;

    [Header("Upgrade Shop")]
    [Tooltip("Base gold cost to increase one skill point by 1.")]
    public int skillUpgradeCostBase = 15;

    [Tooltip("Additional cost per existing skill level (cost = base + existing * ramp).")]
    public int skillUpgradeCostRamp = 5;
}
