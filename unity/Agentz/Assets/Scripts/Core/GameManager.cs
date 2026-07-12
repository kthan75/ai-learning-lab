using System;
using UnityEngine;

/// <summary>
/// Central game state machine. Tracks round timer, failures, gold, and score.
/// Other systems subscribe to its events instead of polling.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Draining, RoundOver }

    // ── Public state ────────────────────────────────────────────────────────
    public GameState  State              { get; private set; }
    public bool       RoundWasFailure    { get; private set; }
    public GameConfig Config             { get; private set; }
    public int        Failures           { get; private set; }
    public int        Gold               { get; private set; }  // this round
    public int        TotalGold          { get; private set; }  // all rounds
    public int        Score              { get; private set; }  // this round
    public int        TotalScore         { get; private set; }  // all rounds
    public int        RoundNumber        { get; private set; } = 1;
    public int        MissionsCompleted  { get; private set; }
    public float      RoundTimeRemaining { get; private set; }
    public int        ConsecSuccesses    { get; private set; }

    // ── Events ──────────────────────────────────────────────────────────────
    public event Action<int>   OnFailureAdded;    // new failure count
    public event Action<int>   OnGoldChanged;     // new gold total
    public event Action<float> OnTimerTick;       // time remaining (every frame)
    public event Action        OnRoundEnd;

    // ────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Initialize(GameConfig config)
    {
        Config             = config;
        Failures           = 0;
        Gold               = 0;
        Score              = 0;
        MissionsCompleted  = 0;
        ConsecSuccesses    = 0;
        RoundTimeRemaining = config.roundDuration;
        State              = GameState.Playing;
    }

    /// <summary>Resets per-round state without clearing cross-round totals.</summary>
    private void InitializeRound()
    {
        Initialize(Config);
    }

    private void Update()
    {
        if (State != GameState.Playing) return;
        if (MissionSpawner.Instance != null && MissionSpawner.Instance.PauseMissions) return;

        RoundTimeRemaining -= Time.deltaTime;

        if (RoundTimeRemaining <= 0f)
        {
            RoundTimeRemaining = 0f;
            State = GameState.Draining; // MissionSpawner drains active missions, then calls CompleteRound()
            OnTimerTick?.Invoke(0f);
            return;
        }

        OnTimerTick?.Invoke(RoundTimeRemaining);
    }

    // ── Called by MissionSpawner ────────────────────────────────────────────
    public void RegisterSuccess(int goldReward)
    {
        if (State == GameState.RoundOver) return;

        MissionsCompleted++;
        ConsecSuccesses++;

        int earned = goldReward;
        if (ConsecSuccesses >= Config.streakBonusThreshold)
            earned += Config.streakBonusGold;

        Gold       += earned;
        TotalGold  += earned;
        Score      += 10;
        TotalScore += 10;
        OnGoldChanged?.Invoke(Gold);
    }

    public void RegisterFailure()
    {
        if (State == GameState.RoundOver) return;

        Failures++;
        ConsecSuccesses = 0;
        OnFailureAdded?.Invoke(Failures);

        // Only trigger game-over by failure limit while still in normal play
        if (State == GameState.Playing && Failures >= Config.failureLimit)
            EndRound(failure: true);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    /// <summary>Called by MissionSpawner once all in-flight missions finish after the timer expires.</summary>
    public void CompleteRound() => EndRound(failure: false);

    private void EndRound(bool failure = false)
    {
        if (State == GameState.RoundOver) return;
        RoundWasFailure = failure;
        State       = GameState.RoundOver;
        int bonus   = RoundNumber * 100;
        Score      += bonus;
        TotalScore += bonus;
        OnRoundEnd?.Invoke();
    }

    public void StartNextRound()
    {
        RoundNumber++;
        int savedTotalGold  = TotalGold;
        int savedTotalScore = TotalScore;
        Initialize(Config);
        TotalGold  = savedTotalGold;
        TotalScore = savedTotalScore;
        MissionSpawner.Instance?.RestartSpawner(Config);
    }

    public void RestartGame()
    {
        RoundNumber = 1;
        TotalGold   = 0;
        TotalScore  = 0;
        Initialize(Config);
        MissionSpawner.Instance?.RestartSpawner(Config);
    }

    // Difficulty helpers used by MissionSpawner
    public float GetDifficultyScale() =>
        Mathf.Pow(Config.skillScalePerRound, RoundNumber - 1);

    public float GetSpawnInterval()
    {
        float reduction = Config.spawnIntervalReductionPerRound * (RoundNumber - 1);
        float lo = Mathf.Max(Config.missionSpawnIntervalMin * (1f - reduction), Config.minSpawnInterval);
        float hi = Mathf.Max(Config.missionSpawnIntervalMax * (1f - reduction), Config.minSpawnInterval + 0.5f);
        return UnityEngine.Random.Range(lo, hi);
    }

    /// <summary>Current chance (0..1) that a newly spawned mission is "Ops Info Incomplete".</summary>
    public float GetOIIChance()
    {
        if (!Config.enableOII) return 0f;
        return Mathf.Min(Config.startingOccurrenceOII + Config.roundScalingOII * (RoundNumber - 1),
                         Config.maxOccurrenceOII);
    }
}
