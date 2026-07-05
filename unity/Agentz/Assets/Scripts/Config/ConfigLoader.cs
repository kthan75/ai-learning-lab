using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Applies "Model B" config overrides from StreamingAssets/config.json.
///   config.json (if present) -> values overwrite a runtime *copy* of the asset
///   no file                  -> the GameConfig asset is used unchanged
///
/// The JSON only needs to contain the fields you want to override; any field it
/// omits keeps the asset's value (JsonUtility.FromJsonOverwrite semantics).
/// The asset itself is never mutated at runtime — we clone it first.
///
/// Note: skillCombineMode serializes as an integer (0 = Average, 1 = Additive,
/// 2 = Max), matching the SkillCombineMode enum order.
/// </summary>
public static class ConfigLoader
{
    private const string FileName = "config.json";

    public static GameConfig LoadOverride(GameConfig asset)
    {
        if (asset == null) return null;

        string path = Path.Combine(Application.streamingAssetsPath, FileName);
        if (!File.Exists(path))
            return asset; // no override file → use the asset as-is

        try
        {
            string json    = File.ReadAllText(path);
            var    runtime = UnityEngine.Object.Instantiate(asset); // don't mutate the asset
            JsonUtility.FromJsonOverwrite(json, runtime);
            Debug.Log($"[ConfigLoader] Applied overrides from {FileName}.");
            return runtime;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConfigLoader] Failed to parse {FileName}: {ex.Message}. Using asset defaults.");
            return asset;
        }
    }
}
