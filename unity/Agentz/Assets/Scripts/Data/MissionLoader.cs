using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Loads mission definitions from StreamingAssets/missions.csv.
/// Falls back to DefaultMissions.cs if the file is missing or contains errors.
///
/// CSV format (first row is a header, ignored):
///   Title,Description,Eng,Dip,Nav,SS,Res,Gold,Tier
///   Fields with commas must be wrapped in double-quotes.
/// </summary>
public static class MissionLoader
{
    private const string FileName = "missions.csv";

    /// <summary>
    /// Returns mission definitions from the CSV file, or the built-in defaults on failure.
    /// </summary>
    public static DefaultMissions.MissionDef[] Load()
    {
        string path = Path.Combine(Application.streamingAssetsPath, FileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[MissionLoader] {FileName} not found at '{path}'. Using built-in missions.");
            return DefaultMissions.All;
        }

        try
        {
            string[] lines = File.ReadAllLines(path);
            var results = new List<DefaultMissions.MissionDef>();

            // Skip header row (line 0)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var fields = ParseCsvLine(line);
                if (fields.Count < 9)
                {
                    Debug.LogWarning($"[MissionLoader] Line {i + 1} has {fields.Count} fields (expected 9), skipping.");
                    continue;
                }

                string title = fields[0];
                string desc  = fields[1];

                if (!float.TryParse(fields[2], out float eng)  ||
                    !float.TryParse(fields[3], out float dip)  ||
                    !float.TryParse(fields[4], out float nav)  ||
                    !float.TryParse(fields[5], out float ss)   ||
                    !float.TryParse(fields[6], out float res)  ||
                    !int.TryParse(fields[7],   out int gold)   ||
                    !int.TryParse(fields[8],   out int tier))
                {
                    Debug.LogWarning($"[MissionLoader] Line {i + 1} has invalid numeric values, skipping.");
                    continue;
                }

                results.Add(new DefaultMissions.MissionDef(title, desc, eng, dip, nav, ss, res, gold, tier));
            }

            if (results.Count == 0)
            {
                Debug.LogWarning("[MissionLoader] CSV parsed but contained no valid missions. Using built-in missions.");
                return DefaultMissions.All;
            }

            Debug.Log($"[MissionLoader] Loaded {results.Count} missions from {FileName}.");
            return results.ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MissionLoader] Failed to parse {FileName}: {ex.Message}. Using built-in missions.");
            return DefaultMissions.All;
        }
    }

    // ── Minimal CSV parser — handles quoted fields containing commas ──────────
    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        int i = 0;

        while (i < line.Length)
        {
            if (line[i] == '"')
            {
                // Quoted field
                i++; // skip opening quote
                var sb = new System.Text.StringBuilder();
                while (i < line.Length)
                {
                    if (line[i] == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"'); // escaped quote ""
                            i += 2;
                        }
                        else
                        {
                            i++; // closing quote
                            break;
                        }
                    }
                    else
                    {
                        sb.Append(line[i++]);
                    }
                }
                fields.Add(sb.ToString());
                if (i < line.Length && line[i] == ',') i++; // skip comma
            }
            else
            {
                // Unquoted field
                int start = i;
                while (i < line.Length && line[i] != ',') i++;
                fields.Add(line.Substring(start, i - start).Trim());
                if (i < line.Length) i++; // skip comma
            }
        }

        return fields;
    }
}
