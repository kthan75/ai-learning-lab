using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor menu (under "Agentz") to sync the agent roster and GameConfig between the
/// Unity assets and the plain-text override files in StreamingAssets.
///
/// Sync is always explicit and one-way per click — there is no automatic two-way sync:
///   Export  = assets  -> text file   (agents.csv / config.json)
///   Import  = text file -> assets     (overwrites the .asset, creates an Undo step)
///
/// At Play time the text files (if present) win over the assets — see AgentLoader /
/// ConfigLoader. Use Import if you want the assets to match a hand-edited text file, or
/// Export to seed the text files from the current assets.
/// </summary>
public static class AgentzDataMenu
{
    private const string AgentsFile = "agents.csv";
    private const string ConfigFile = "config.json";

    // ── Agents ───────────────────────────────────────────────────────────────
    [MenuItem("Agentz/Export Agents to agents.csv")]
    public static void ExportAgents()
    {
        var guids = AssetDatabase.FindAssets("t:AgentData");
        if (guids.Length == 0)
        {
            Debug.LogWarning("[Agentz] No AgentData assets found to export.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Name,Bio,ENG,DIP,NAV,SSM,RES");
        foreach (var guid in guids)
        {
            var a = AssetDatabase.LoadAssetAtPath<AgentData>(AssetDatabase.GUIDToAssetPath(guid));
            if (a == null) continue;
            var s = a.skills;
            sb.AppendLine($"{CsvUtil.Quote(a.agentName)},{CsvUtil.Quote(a.bio)}," +
                          $"{s.engineering},{s.diplomacy},{s.navigation},{s.streetSmarts},{s.resilience}");
        }

        WriteStreamingAsset(AgentsFile, sb.ToString());
        Debug.Log($"[Agentz] Exported {guids.Length} agents to StreamingAssets/{AgentsFile}.");
    }

    [MenuItem("Agentz/Import agents.csv into Agent assets")]
    public static void ImportAgents()
    {
        string path = Path.Combine(Application.streamingAssetsPath, AgentsFile);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[Agentz] StreamingAssets/{AgentsFile} not found. Export first, or create it.");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        int updated = 0, skipped = 0;

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var fields = CsvUtil.ParseLine(line);
            if (fields.Count < 7)
            {
                Debug.LogWarning($"[Agentz] Line {i + 1}: expected 7 fields, got {fields.Count}. Skipped.");
                skipped++;
                continue;
            }

            var asset = FindAgentAsset(fields[0]);
            if (asset == null)
            {
                Debug.LogWarning($"[Agentz] Line {i + 1}: no AgentData asset named \"{fields[0]}\". Skipped.");
                skipped++;
                continue;
            }

            if (!float.TryParse(fields[2], out float eng) ||
                !float.TryParse(fields[3], out float dip) ||
                !float.TryParse(fields[4], out float nav) ||
                !float.TryParse(fields[5], out float ss)  ||
                !float.TryParse(fields[6], out float res))
            {
                Debug.LogWarning($"[Agentz] Line {i + 1}: invalid skill values. Skipped.");
                skipped++;
                continue;
            }

            Undo.RecordObject(asset, "Import Agent");
            asset.bio    = fields[1];
            asset.skills = new SkillSet(eng, dip, nav, ss, res);
            EditorUtility.SetDirty(asset);
            updated++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Agentz] Imported agents.csv — {updated} asset(s) updated, {skipped} row(s) skipped.");
    }

    // ── Config ───────────────────────────────────────────────────────────────
    [MenuItem("Agentz/Export Config to config.json")]
    public static void ExportConfig()
    {
        var cfg = FindConfig();
        if (cfg == null)
        {
            Debug.LogWarning("[Agentz] No GameConfig asset found to export.");
            return;
        }

        WriteStreamingAsset(ConfigFile, JsonUtility.ToJson(cfg, true));
        Debug.Log($"[Agentz] Exported GameConfig to StreamingAssets/{ConfigFile}.");
    }

    [MenuItem("Agentz/Import config.json into GameConfig asset")]
    public static void ImportConfig()
    {
        var cfg = FindConfig();
        if (cfg == null)
        {
            Debug.LogWarning("[Agentz] No GameConfig asset found to import into.");
            return;
        }

        string path = Path.Combine(Application.streamingAssetsPath, ConfigFile);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[Agentz] StreamingAssets/{ConfigFile} not found. Export first, or create it.");
            return;
        }

        Undo.RecordObject(cfg, "Import Config");
        JsonUtility.FromJsonOverwrite(File.ReadAllText(path), cfg);
        EditorUtility.SetDirty(cfg);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Agentz] Imported config.json into GameConfig asset.");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private static void WriteStreamingAsset(string fileName, string contents)
    {
        Directory.CreateDirectory(Application.streamingAssetsPath);
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, fileName), contents);
        AssetDatabase.Refresh();
    }

    private static AgentData FindAgentAsset(string agentName)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:AgentData"))
        {
            var a = AssetDatabase.LoadAssetAtPath<AgentData>(AssetDatabase.GUIDToAssetPath(guid));
            if (a != null && a.agentName == agentName) return a;
        }
        return null;
    }

    private static GameConfig FindConfig()
    {
        var guids = AssetDatabase.FindAssets("t:GameConfig");
        if (guids.Length == 0) return null;
        if (guids.Length > 1)
            Debug.LogWarning("[Agentz] Multiple GameConfig assets found; using the first.");
        return AssetDatabase.LoadAssetAtPath<GameConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}
