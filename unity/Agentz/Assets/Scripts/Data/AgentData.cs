using UnityEngine;

/// <summary>
/// Persistent data for one agent. Create instances via:
///   Assets > Create > Agentz > Agent Data
/// </summary>
[CreateAssetMenu(fileName = "NewAgent", menuName = "Agentz/Agent Data")]
public class AgentData : ScriptableObject
{
    [Header("Identity")]
    public string agentName = "Unnamed Agent";
    [TextArea(1, 3)]
    public string bio = "";
    public Sprite portrait; // optional in MVP; placeholder is fine

    [Header("Skills")]
    public SkillSet skills;

    // Runtime state — not serialized to asset; reset each game session.
    [System.NonSerialized] public bool   isAvailable = true;

    // Incapacitation (temporary, random) — isAvailable is set false while incapacitated.
    [System.NonSerialized] public bool   isIncapacitated;
    [System.NonSerialized] public float  incapTimeRemaining;
    [System.NonSerialized] public string incapReason;
}
