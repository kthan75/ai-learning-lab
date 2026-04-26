using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns Mission objects at random intervals and ticks them each frame.
/// Notifies GameManager of successes/failures and fires events for the UI.
/// </summary>
public class MissionSpawner : MonoBehaviour
{
    public static MissionSpawner Instance { get; private set; }

    // ── Events for UI ────────────────────────────────────────────────────────
    public event Action<Mission>                    OnMissionSpawned;
    public event Action<Mission>                    OnMissionExpired;
    public event Action<Mission, bool, float, int>  OnMissionResolved; // mission, success, overlap, roll

    // ── State ────────────────────────────────────────────────────────────────
    private GameConfig       _config;
    private MissionTemplate[] _pool;
    private readonly List<Mission> _active = new List<Mission>();
    private float _spawnTimer;

    public IReadOnlyList<Mission> ActiveMissions => _active;

    /// <summary>
    /// When true, all mission timers (expiry + busy) and the spawn timer are frozen.
    /// Set by GameUI whenever any popup is open.
    /// </summary>
    public bool PauseMissions { get; set; }

    // ────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(GameConfig config, MissionTemplate[] pool)
    {
        _config = config;
        _pool   = (pool != null && pool.Length > 0) ? pool : BuildDefaultPool();
        RestartSpawner(config);
    }

    public void RestartSpawner(GameConfig config)
    {
        _config = config;
        _active.Clear();
        _spawnTimer = UnityEngine.Random.Range(config.missionSpawnIntervalMin,
                                               config.missionSpawnIntervalMax);
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.State != GameManager.GameState.Playing) return;

        if (PauseMissions) return;

        // Tick active missions (iterate backwards so removal is safe)
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var m = _active[i];
            if (m.State == Mission.MissionState.Resolved)
            {
                _active.RemoveAt(i);
                continue;
            }
            m.Tick(Time.deltaTime);
        }

        // Spawn timer
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            TrySpawn();
            _spawnTimer = GameManager.Instance.GetSpawnInterval();
        }
    }

    // ── Assignment (called by UI) ────────────────────────────────────────────
    public void AssignAgents(Mission mission, List<AgentData> agents)
    {
        foreach (var a in agents) a.isAvailable = false;
        mission.Assign(agents, _config.missionBusyDuration);
    }

    // ── Private ──────────────────────────────────────────────────────────────
    private void TrySpawn()
    {
        if (_active.Count >= _config.maxActiveMissions) return;
        if (_pool == null || _pool.Length == 0) return;

        var template = _pool[UnityEngine.Random.Range(0, _pool.Length)];
        var mission  = new Mission(template, _config.missionExpiryDuration,
                                  GameManager.Instance.GetDifficultyScale());

        mission.OnExpired  += () => HandleExpired(mission);
        mission.OnResolved += (ok, pct, roll) => HandleResolved(mission, ok, pct, roll);

        _active.Add(mission);
        OnMissionSpawned?.Invoke(mission);
    }

    private void HandleExpired(Mission mission)
    {
        GameManager.Instance.RegisterFailure();
        OnMissionExpired?.Invoke(mission);
        _active.Remove(mission);
    }

    private void HandleResolved(Mission mission, bool success, float overlap, int roll)
    {
        if (success)
            GameManager.Instance.RegisterSuccess(mission.Template.goldReward);
        else
            GameManager.Instance.RegisterFailure();

        foreach (var a in mission.AssignedAgents)
            a.isAvailable = true;

        OnMissionResolved?.Invoke(mission, success, overlap, roll);
        _active.Remove(mission);
    }

    private MissionTemplate[] BuildDefaultPool()
    {
        var defs = DefaultMissions.All;
        var pool = new MissionTemplate[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var so = ScriptableObject.CreateInstance<MissionTemplate>();
            so.name           = defs[i].Title;
            so.missionTitle   = defs[i].Title;
            so.description    = defs[i].Description;
            so.requiredSkills = defs[i].Skills;
            so.goldReward     = defs[i].GoldReward;
            so.difficultyTier = defs[i].Tier;
            pool[i] = so;
        }
        return pool;
    }
}
