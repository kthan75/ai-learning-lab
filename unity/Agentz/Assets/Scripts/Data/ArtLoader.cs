using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Loads UI art sprites at runtime from StreamingAssets/Art/ (Model B), matched by exact
/// filename (e.g. "premise_bg.png"). Results are cached for the session; a missing file
/// caches as null so it isn't re-checked. Backgrounds, logo, icons, and stamps all load
/// through here.
/// </summary>
public static class ArtLoader
{
    private const string FolderName = "Art";
    private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

    /// <summary>Loads a sprite by filename, or returns null if the file is missing/undecodable.</summary>
    public static Sprite Load(string fileName)
    {
        if (_cache.TryGetValue(fileName, out var cached)) return cached;

        Sprite sprite = null;
        string path = Path.Combine(Application.streamingAssetsPath, FolderName, fileName);

        if (File.Exists(path))
        {
            try
            {
                byte[] data = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                if (tex.LoadImage(data))
                {
                    tex.wrapMode = TextureWrapMode.Clamp;
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                           new Vector2(0.5f, 0.5f), 100f);
                }
                else
                {
                    Debug.LogError($"[ArtLoader] Could not decode {fileName}.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ArtLoader] Failed to load {fileName}: {ex.Message}");
            }
        }
        else
        {
            Debug.Log($"[ArtLoader] {fileName} not found in StreamingAssets/{FolderName} — skipping.");
        }

        _cache[fileName] = sprite;
        return sprite;
    }
}
