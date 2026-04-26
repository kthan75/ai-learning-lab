using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime data for one active mission. Plain C# class (not MonoBehaviour).
/// State machine: Waiting → Busy → Resolved.
/// </summary>
public class Mission
{
    public enum MissionState { Waiting, Busy, Resolved }

    // ── Identity ─────────────────────────────────────────────────────────────
    public MissionTemplate Template        { get; }
    public MissionState    State           { get; private set; }
    public float           TimeRemaining   { get; private set; }   // while Waiting
    public float           BusyRemaining   { get; private set; }   // while Busy
    public float           MaxExpiry       { get; }
    public List<AgentData> AssignedAgents  { get; } = new List<AgentData>();

    // ── Resolution results (valid after Resolved) ────────────────────────────
    public bool  WasSuccess    { get; private set; }
    public float OverlapPct    { get; private set; }   // 0..1
    public int   DiceRoll      { get; private set; }   // 1..100

    // ── Events ───────────────────────────────────────────────────────────────
    public event Action                    OnExpired;
    public event Action<bool, float, int>  OnResolved;   // success, overlapPct, roll
    public event Action                    OnStateChanged;

    private readonly float _difficultyScale;

    // ────────────────────────────────────────────────────────────────────────
    public Mission(MissionTemplate template, float expiryDuration, float difficultyScale)
    {
        Template         = template;
        TimeRemaining    = expiryDuration;
        MaxExpiry        = expiryDuration;
        _difficultyScale = difficultyScale;
        State            = MissionState.Waiting;
    }

    /// <summary>Call once per frame from MissionSpawner.Update().</summary>
    public void Tick(float dt)
    {
        if (State == MissionState.Waiting)
        {
            TimeRemaining -= dt;
            if (TimeRemaining <= 0f)
            {
                State      = MissionState.Resolved;
                WasSuccess = false;
                OnExpired?.Invoke();
                OnStateChanged?.Invoke();
            }
        }
        else if (State == MissionState.Busy)
        {
            BusyRemaining -= dt;
            if (BusyRemaining <= 0f)
                Resolve();
        }
    }

    /// <summary>Lock agents and start the busy countdown.</summary>
    public void Assign(IEnumerable<AgentData> agents, float busyDuration)
    {
        if (State != MissionState.Waiting) return;
        AssignedAgents.Clear();
        foreach (var a in agents) AssignedAgents.Add(a);
        State         = MissionState.Busy;
        BusyRemaining = busyDuration;
        OnStateChanged?.Invoke();
    }

    public float TimeFraction => Mathf.Clamp01(TimeRemaining / MaxExpiry);

    // ── Private ──────────────────────────────────────────────────────────────
    private void Resolve()
    {
        // Combine the assigned agents' skills using the configured mode
        var sets = new SkillSet[AssignedAgents.Count];
        for (int i = 0; i < AssignedAgents.Count; i++)
            sets[i] = AssignedAgents[i].skills;
        var mode = GameManager.Instance.Config.skillCombineMode;
        var combined = SkillSet.CombineAll(sets, mode);

        // Scale requirements by difficulty
        var req = Template.requiredSkills;
        float s  = _difficultyScale;
        var scaled = new SkillSet(
            Mathf.Min(req.engineering  * s, 10f),
            Mathf.Min(req.diplomacy    * s, 10f),
            Mathf.Min(req.navigation   * s, 10f),
            Mathf.Min(req.streetSmarts * s, 10f),
            Mathf.Min(req.resilience   * s, 10f)
        );

        OverlapPct = SkillSet.ComputeOverlap(combined, scaled);
        DiceRoll   = UnityEngine.Random.Range(1, 101);
        WasSuccess = DiceRoll <= Mathf.RoundToInt(OverlapPct * 100f);

        State = MissionState.Resolved;
        OnResolved?.Invoke(WasSuccess, OverlapPct, DiceRoll);
        OnStateChanged?.Invoke();
    }
}
