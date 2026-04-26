using UnityEngine;

/// <summary>
/// Template used to generate missions at runtime.
/// Create instances via: Assets > Create > Agentz > Mission Template
/// </summary>
[CreateAssetMenu(fileName = "NewMission", menuName = "Agentz/Mission Template")]
public class MissionTemplate : ScriptableObject
{
    [Header("Presentation")]
    public string missionTitle = "Unnamed Mission";
    [TextArea(2, 4)]
    public string description = "";

    [Header("Requirements")]
    public SkillSet requiredSkills;

    [Header("Rewards")]
    [Min(0)] public int goldReward = 10;

    [Header("Difficulty weight — higher = spawns more often at harder rounds")]
    [Range(1, 10)] public int difficultyTier = 1;
}
