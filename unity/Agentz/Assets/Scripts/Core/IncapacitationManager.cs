using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Randomly pulls idle agents out of action for a short time (injury, emergency, …).
/// Only agents with `isAvailable == true` (idle, not on a mission) are eligible — an
/// agent on a mission is never incapacitated. Ticking pauses with popups (via
/// `MissionSpawner.PauseMissions`) and only runs during the Playing state.
///
/// All tuning lives in GameConfig (config.json-editable). Raises <see cref="OnChanged"/>
/// whenever an agent is incapacitated or released, so the roster UI can refresh.
/// </summary>
[DefaultExecutionOrder(100)] // run after MissionSpawner/GameUI so this frame's pause flag is set
public class IncapacitationManager : MonoBehaviour
{
    private static readonly string[] Reasons =
    {
        "Agent injured.",
        "Personal emergency.",
        "Stuck doing paperwork."
    };

    public event Action             OnChanged;        // incapacitated OR released (roster refresh)
    public event Action<AgentData>  OnIncapacitated;  // an agent was just incapacitated (event popup)

    private AgentData[] _agents;
    private float       _checkTimer;
    private int         _lastRound = -1;

    public void Initialize(AgentData[] agents) => _agents = agents;

    private void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || _agents == null) return;
        var cfg = gm.Config;

        // New round → reset the check cadence and clear any leftover incapacitations.
        if (gm.RoundNumber != _lastRound)
        {
            _lastRound  = gm.RoundNumber;
            _checkTimer = 0f;
            if (ClearAll()) OnChanged?.Invoke();
        }

        if (gm.State != GameManager.GameState.Playing) return;
        if (MissionSpawner.Instance != null && MissionSpawner.Instance.PauseMissions) return;

        if (!cfg.enableIncapacitation)
        {
            if (ClearAll()) OnChanged?.Invoke();
            return;
        }

        float dt = Time.deltaTime;
        bool  changed = false;

        // Tick active incapacitations and release the finished ones.
        foreach (var a in _agents)
        {
            if (a == null || !a.isIncapacitated) continue;
            a.incapTimeRemaining -= dt;
            if (a.incapTimeRemaining <= 0f) { Release(a); changed = true; }
        }

        // Occurrence check — only after the round's safe window.
        float elapsed = cfg.roundDuration - gm.RoundTimeRemaining;
        if (elapsed >= cfg.incapSafeTime)
        {
            _checkTimer += dt;
            if (_checkTimer >= cfg.incapOccurrenceRate)
            {
                _checkTimer -= cfg.incapOccurrenceRate;
                if (UnityEngine.Random.value < cfg.incapOccurrenceChance)
                {
                    var victim = TryIncapacitate(cfg);
                    if (victim != null) { changed = true; OnIncapacitated?.Invoke(victim); }
                }
            }
        }

        if (changed) OnChanged?.Invoke();
    }

    // ── Private ──────────────────────────────────────────────────────────────
    private AgentData TryIncapacitate(GameConfig cfg)
    {
        var pool = new List<AgentData>();
        foreach (var a in _agents)
            if (a != null && a.isAvailable && !a.isIncapacitated) pool.Add(a);
        if (pool.Count == 0) return null;

        var agent = pool[UnityEngine.Random.Range(0, pool.Count)];
        agent.isIncapacitated    = true;
        agent.isAvailable        = false;
        agent.incapReason        = Reasons[UnityEngine.Random.Range(0, Reasons.Length)];
        agent.incapTimeRemaining = UnityEngine.Random.Range(cfg.incapDurationMin, cfg.incapDurationMax);
        return agent;
    }

    /// <summary>Restores an incapacitated (always idle) agent to availability.</summary>
    private static void Release(AgentData a)
    {
        a.isIncapacitated    = false;
        a.incapReason        = null;
        a.incapTimeRemaining = 0f;
        a.isAvailable        = true;
    }

    private bool ClearAll()
    {
        bool any = false;
        foreach (var a in _agents)
            if (a != null && a.isIncapacitated) { Release(a); any = true; }
        return any;
    }
}
