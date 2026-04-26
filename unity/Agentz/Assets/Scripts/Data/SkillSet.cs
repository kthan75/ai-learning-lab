using System;
using UnityEngine;

/// <summary>
/// The five skills shared by both agents and missions.
/// Values range from 0 to 10.
/// </summary>
[Serializable]
public struct SkillSet
{
    [Range(0, 10)] public float engineering;
    [Range(0, 10)] public float diplomacy;
    [Range(0, 10)] public float navigation;
    [Range(0, 10)] public float streetSmarts;
    [Range(0, 10)] public float resilience;

    public SkillSet(float eng, float dip, float nav, float ss, float res)
    {
        engineering = Mathf.Clamp(eng, 0, 10);
        diplomacy   = Mathf.Clamp(dip, 0, 10);
        navigation  = Mathf.Clamp(nav, 0, 10);
        streetSmarts = Mathf.Clamp(ss, 0, 10);
        resilience  = Mathf.Clamp(res, 0, 10);
    }

    /// <summary>Returns skill values as a float array in canonical order.</summary>
    public float[] ToArray() => new[]
    {
        engineering, diplomacy, navigation, streetSmarts, resilience
    };

    public static readonly string[] SkillNames =
    {
        "Engineering", "Diplomacy", "Navigation", "Street Smarts", "Resilience"
    };

    /// <summary>
    /// Averages this SkillSet with another (used when combining two agents).
    /// For N agents, chain: combined = a.Average(b).Average(c)... or use the static helper.
    /// </summary>
    public SkillSet Average(SkillSet other) => new SkillSet(
        (engineering  + other.engineering)  / 2f,
        (diplomacy    + other.diplomacy)    / 2f,
        (navigation   + other.navigation)   / 2f,
        (streetSmarts + other.streetSmarts) / 2f,
        (resilience   + other.resilience)   / 2f
    );

    /// <summary>
    /// Averages an array of SkillSets (for 1–3 agents assigned to a mission).
    /// </summary>
    public static SkillSet AverageAll(SkillSet[] sets)
    {
        if (sets == null || sets.Length == 0)
            return new SkillSet();

        float eng = 0, dip = 0, nav = 0, ss = 0, res = 0;
        foreach (var s in sets)
        {
            eng += s.engineering;
            dip += s.diplomacy;
            nav += s.navigation;
            ss  += s.streetSmarts;
            res += s.resilience;
        }
        int n = sets.Length;
        return new SkillSet(eng / n, dip / n, nav / n, ss / n, res / n);
    }

    /// <summary>
    /// Computes the spider-chart overlap percentage between the combined agent skills
    /// and the mission requirements. Returns a value between 0 and 1.
    ///
    /// Algorithm: For each axis, the agent contributes min(agentSkill, missionSkill)/10.
    /// The overlap area fraction is the average across all 5 axes.
    /// </summary>
    public static float ComputeOverlap(SkillSet agentCombined, SkillSet missionRequired)
    {
        float[] a = agentCombined.ToArray();
        float[] m = missionRequired.ToArray();

        float totalOverlap = 0f;
        float totalRequired = 0f;

        for (int i = 0; i < 5; i++)
        {
            totalOverlap  += Mathf.Min(a[i], m[i]);
            totalRequired += m[i];
        }

        if (totalRequired <= 0f) return 1f; // trivial mission, always succeed
        return Mathf.Clamp01(totalOverlap / totalRequired);
    }
}
