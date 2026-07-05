using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Loads agent portraits at runtime from StreamingAssets/Portraits/ (Model B), matching
/// each agent to a file by a slug of its name — e.g. "Sergeant Brutus Kael" →
/// "sergeant_brutus_kael.png". Keeps portraits editable outside Unity and works for
/// agents defined via agents.csv.
///
/// Precedence: an Inspector-assigned portrait is kept; the folder only fills agents that
/// don't already have one. Missing files are skipped (the agent simply has no portrait).
/// Supported extensions: .png, .jpg, .jpeg.
/// </summary>
public static class PortraitLoader
{
    private const string FolderName = "Portraits";
    private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg" };

    public static void LoadInto(AgentData[] agents)
    {
        if (agents == null) return;

        string dir = Path.Combine(Application.streamingAssetsPath, FolderName);
        if (!Directory.Exists(dir))
        {
            Debug.Log($"[PortraitLoader] No '{FolderName}' folder in StreamingAssets — skipping portraits.");
            return;
        }

        int loaded = 0;
        foreach (var a in agents)
        {
            if (a == null || a.portrait != null) continue; // keep any Inspector-assigned portrait

            string path = FindFile(dir, Slug(a.agentName));
            if (path == null)
            {
                Debug.LogWarning($"[PortraitLoader] No portrait for '{a.agentName}' (expected {Slug(a.agentName)}.png).");
                continue;
            }

            var sprite = LoadSprite(path);
            if (sprite != null) { a.portrait = sprite; loaded++; }
        }

        Debug.Log($"[PortraitLoader] Loaded {loaded} portrait(s) from StreamingAssets/{FolderName}.");
    }

    /// <summary>Converts an agent name to a lowercase file slug (drops quotes/apostrophes, other runs → '_').</summary>
    public static string Slug(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";

        var sb = new StringBuilder();
        bool pendingSep = false;
        foreach (char raw in name)
        {
            char c = char.ToLowerInvariant(raw);
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                if (pendingSep && sb.Length > 0) sb.Append('_');
                pendingSep = false;
                sb.Append(c);
            }
            else if (c == '\'' || c == '’' || c == '"')
            {
                // drop quotes/apostrophes entirely (no separator)
            }
            else
            {
                pendingSep = true; // collapse any run of separators into a single '_'
            }
        }
        return sb.ToString();
    }

    // ── Private ──────────────────────────────────────────────────────────────
    private static string FindFile(string dir, string slug)
    {
        if (string.IsNullOrEmpty(slug)) return null;
        foreach (var ext in Extensions)
        {
            string p = Path.Combine(dir, slug + ext);
            if (File.Exists(p)) return p;
        }
        return null;
    }

    private static Sprite LoadSprite(string path)
    {
        try
        {
            byte[] data = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            if (!tex.LoadImage(data)) // resizes tex to the image dimensions
            {
                Debug.LogError($"[PortraitLoader] Could not decode image: {Path.GetFileName(path)}");
                return null;
            }
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                 new Vector2(0.5f, 0.5f), 100f);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PortraitLoader] Failed to load {Path.GetFileName(path)}: {ex.Message}");
            return null;
        }
    }
}
