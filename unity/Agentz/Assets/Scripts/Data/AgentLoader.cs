using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Builds the runtime agent roster, applying the "Model B" precedence:
///   StreamingAssets/agents.csv  (if present)  ->  wins
///   Inspector roster            (if non-empty) ->  used when no CSV
///   DefaultAgents.All           (last resort)  ->  used when both are empty
///
/// When a CSV row's name matches an Inspector AgentData, that asset is cloned so
/// its portrait (and any other non-CSV fields) are preserved; the clone's name,
/// bio, and skills are then overwritten from the CSV. The original assets are
/// never mutated at runtime.
///
/// CSV format (first row is a header, ignored):
///   Name,Bio,ENG,DIP,NAV,SSM,RES
///   Fields with commas must be wrapped in double-quotes.
/// </summary>
public static class AgentLoader
{
    private const string FileName = "agents.csv";

    public static AgentData[] LoadRoster(AgentData[] inspectorRoster)
    {
        string path = Path.Combine(Application.streamingAssetsPath, FileName);

        if (!File.Exists(path))
            return FallbackRoster(inspectorRoster); // no CSV → Inspector/defaults

        try
        {
            string[] lines = File.ReadAllLines(path);
            var result = new List<AgentData>();

            // Skip header row (line 0)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var fields = CsvUtil.ParseLine(line);
                if (fields.Count < 7)
                {
                    Debug.LogWarning($"[AgentLoader] Line {i + 1} has {fields.Count} fields (expected 7), skipping.");
                    continue;
                }

                string name = fields[0];
                string bio  = fields[1];

                if (!float.TryParse(fields[2], out float eng) ||
                    !float.TryParse(fields[3], out float dip) ||
                    !float.TryParse(fields[4], out float nav) ||
                    !float.TryParse(fields[5], out float ss)  ||
                    !float.TryParse(fields[6], out float res))
                {
                    Debug.LogWarning($"[AgentLoader] Line {i + 1} has invalid skill values, skipping.");
                    continue;
                }

                result.Add(BuildAgent(inspectorRoster, name, bio, new SkillSet(eng, dip, nav, ss, res)));
            }

            if (result.Count == 0)
            {
                Debug.LogWarning("[AgentLoader] CSV parsed but contained no valid agents. Using Inspector/default roster.");
                return FallbackRoster(inspectorRoster);
            }

            Debug.Log($"[AgentLoader] Loaded {result.Count} agents from {FileName}.");
            return result.ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AgentLoader] Failed to parse {FileName}: {ex.Message}. Using Inspector/default roster.");
            return FallbackRoster(inspectorRoster);
        }
    }

    // ── Private ──────────────────────────────────────────────────────────────
    private static AgentData[] FallbackRoster(AgentData[] inspectorRoster)
    {
        if (inspectorRoster != null && inspectorRoster.Length > 0)
            return inspectorRoster;

        // Last resort: synthesize AgentData from the hard-coded defaults.
        var defs = DefaultAgents.All;
        var arr  = new AgentData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
            arr[i] = MakeAgent(defs[i].Name, defs[i].Bio, defs[i].Skills);
        return arr;
    }

    private static AgentData BuildAgent(AgentData[] inspectorRoster, string name, string bio, SkillSet skills)
    {
        AgentData template = FindByName(inspectorRoster, name);
        AgentData a = template != null
            ? UnityEngine.Object.Instantiate(template) // clone → keeps portrait, doesn't touch the asset
            : ScriptableObject.CreateInstance<AgentData>();

        a.agentName   = name;
        a.bio         = bio;
        a.skills      = skills;
        a.isAvailable = true;
        return a;
    }

    private static AgentData MakeAgent(string name, string bio, SkillSet skills)
    {
        var a = ScriptableObject.CreateInstance<AgentData>();
        a.agentName   = name;
        a.bio         = bio;
        a.skills      = skills;
        a.isAvailable = true;
        return a;
    }

    private static AgentData FindByName(AgentData[] roster, string name)
    {
        if (roster == null) return null;
        foreach (var a in roster)
            if (a != null && a.agentName == name) return a;
        return null;
    }
}
