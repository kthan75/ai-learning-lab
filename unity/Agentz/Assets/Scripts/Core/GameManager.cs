using System;
using UnityEngine;

/// <summary>
/// Central game state machine. Tracks round timer, failures, gold, and score.
/// Other systems subscribe to its events instead of polling.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, RoundOver }

    // ── Public state ────────────────────────────────────────────────────────
    public GameState  State              { get; private set; }
    public GameConfig Config             { get; private set; }
    public int        Failures           { get; private set; }
    public int        Gold               { get; private set; }
    public int        Score              { get; private set; }
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

    private void Update()
    {
        if (State != GameState.Playing) return;

        RoundTimeRemaining -= Time.deltaTime;
        OnTimerTick?.Invoke(RoundTimeRemaining);

        if (RoundTimeRemaining <= 0f)
            EndRound();
    }

    // ── Called by MissionSpawner ────────────────────────────────────────────
    public void RegisterSuccess(int goldReward)
    {
        if (State != GameState.Playing) return;

        MissionsCompleted++;
        ConsecSuccesses++;

        int earned = goldReward;
        if (ConsecSuccesses >= Config.streakBonusThreshold)
            earned += Config.streakBonusGold;

        Gold  += earned;
        Score += 10;
        OnGoldChanged?.Invoke(Gold);
    }

    public void RegisterFailure()
    {
        if (State != GameState.Playing) return;

        Failures++;
        ConsecSuccesses = 0;
        OnFailureAdded?.Invoke(Failures);

        if (Failures >= Config.failureLimit)
            EndRound();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private void EndRound()
    {
        if (State == GameState.RoundOver) return;
        State  =  GameState.RoundOver;
        Score  += RoundNumber * 100;
        OnRoundEnd?.Invoke();
    }

    public void StartNextRound()
    {
        RoundNumber++;
        Initialize(Config);
        MissionSpawner.Instance?.RestartSpawner(Config);
    }

    public void RestartGame()
    {
        RoundNumber = 1;
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
}
