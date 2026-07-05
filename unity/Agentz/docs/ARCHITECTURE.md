# Agentz — Code Architecture

How the systems fit together, for anyone (human or AI) picking the project back up. Paths
are relative to `unity/Agentz/Assets/Scripts/`.

## Design tenets
- **Data-driven:** gameplay values live in ScriptableObjects (`GameConfig`, `AgentData`,
  `MissionTemplate`), not in code.
- **UI built at runtime:** no prefabs. Everything is constructed programmatically on a
  ScreenSpace-Overlay Canvas. Trades Inspector-editability for reviewable, diffable C#.
- **Events over polling:** systems raise C# `event`s; the UI subscribes. `GameManager` and
  `MissionSpawner` never reach into UI.
- **Plain C# where MonoBehaviour isn't needed:** `Mission` is a plain class ticked by the
  spawner; only long-lived systems are MonoBehaviours.

## Boot sequence
`GameBootstrapper` (the one component you place in the scene) runs on `Awake` and wires
everything — see `Core/GameBootstrapper.cs`:

```
GameBootstrapper.Awake()
  ├─ validate GameConfig (assigned in Inspector)
  ├─ runtimeConfig = ConfigLoader.LoadOverride(config)   // config.json wins if present
  ├─ roster        = AgentLoader.LoadRoster(agents)      // agents.csv wins if present
  ├─ PortraitLoader.LoadInto(roster)                     // fills AgentData.portrait from files
  ├─ reset each roster AgentData.isAvailable = true
  ├─ new GameObject "GameManager"    → GameManager.Initialize(runtimeConfig)
  ├─ new GameObject "MissionSpawner" → MissionSpawner.Initialize(runtimeConfig, missionPool)
  └─ new GameObject "GameUI"         → GameUI.Initialize(roster)
```

Inspector slots on `GameBootstrapper`: `config` (GameConfig), `agents` (6 AgentData),
`missionPool` (optional MissionTemplate[] — if empty, the CSV/built-in pool is used).

**Plain-text overrides (Model B):** at boot, `ConfigLoader` and `AgentLoader` check
`StreamingAssets/` for `config.json` and `agents.csv`. If a file exists it overrides the
Inspector config / roster *for that run only* — the underlying assets are never mutated
(config is cloned via `Instantiate`; agents are rebuilt, cloning any name-matched asset to
keep its portrait). If a file is absent, the Inspector values are used exactly as before.
See [EDITING_GUIDE.md](EDITING_GUIDE.md) for the editing workflow and the Editor sync menu.

## Module map

### Config — `Config/`
- **`GameConfig.cs`** — one ScriptableObject holding every tunable: round, spawning,
  resolution, economy, difficulty scaling, and (future) upgrade-shop costs. Single source of
  truth for balance.
- **`ConfigLoader.cs`** — applies `StreamingAssets/config.json` onto a runtime clone of the
  `GameConfig` at boot (partial overrides supported via `JsonUtility.FromJsonOverwrite`).
  Never mutates the asset.

### Data — `Data/`
- **`SkillSet.cs`** — the 5-axis skill struct (ENG/DIP/NAV/SSM/RES, 0–10) **plus the core
  math**:
  - `CombineAll(sets, mode)` — combine multiple agents by Average / Additive / Max.
  - `ComputeOverlap(agentCombined, missionRequired)` — success-chance math
    (`Σ min(agent, req) / Σ req`, clamped). *This is what the M2 spider chart visualizes.*
  - `SkillNames` / `SkillAbbreviations` — canonical labels & order.
- **`AgentData.cs`** — ScriptableObject: identity + `SkillSet skills` + a `[NonSerialized]
  isAvailable` runtime flag.
- **`MissionTemplate.cs`** — ScriptableObject: presentation, `requiredSkills`, `goldReward`,
  `difficultyTier`.
- **`DefaultAgents.cs`** — hard-coded 6-agent roster (fallback / initial data).
- **`DefaultMissions.cs`** — hard-coded 30-mission pool (fallback for the CSV).
- **`MissionLoader.cs`** — reads `StreamingAssets/missions.csv`, falls back to
  `DefaultMissions.All` on missing file / parse error.
- **`AgentLoader.cs`** — reads `StreamingAssets/agents.csv` (Model B). Precedence:
  `agents.csv` → Inspector roster → `DefaultAgents.All`. Clones name-matched Inspector assets
  so portraits survive.
- **`PortraitLoader.cs`** *(M2)* — at boot, fills `AgentData.portrait` from
  `StreamingAssets/Portraits/<name-slug>.png` (decodes to a runtime `Sprite`). Inspector-
  assigned portraits take precedence; missing files are skipped.
- **`CsvUtil.cs`** — shared quoted-CSV line parser + field-quoting helper, used by both
  loaders and the Editor menu.

### Core — `Core/`
- **`GameManager.cs`** *(singleton MonoBehaviour)* — the round state machine and scoreboard.
  - State: `Playing → Draining → RoundOver`. Owns the round timer, failures, gold, score,
    round number, streak.
  - Events: `OnFailureAdded`, `OnGoldChanged`, `OnTimerTick`, `OnRoundEnd`.
  - Called by the spawner: `RegisterSuccess(gold)`, `RegisterFailure()`, `CompleteRound()`.
  - Difficulty helpers: `GetDifficultyScale()`, `GetSpawnInterval()`.
- **`MissionSpawner.cs`** *(singleton MonoBehaviour)* — spawns and ticks missions.
  - Owns the active-mission list, spawn timer, and the `PauseMissions` flag.
  - Drives the *Draining* logic (clear Waiting without penalty, let Busy finish, then end
    round).
  - Events for UI: `OnMissionSpawned`, `OnMissionExpired`, `OnMissionResolved`.
  - `AssignAgents(mission, agents)` locks agents and starts the busy phase.
- **`Mission.cs`** *(plain C# class)* — one active mission.
  - State: `Waiting → Busy → Resolved`. `Tick(dt)` advances timers.
  - `Resolve()` combines assigned agents (per `GameConfig.skillCombineMode`), applies the
    round difficulty scale to requirements, computes overlap, rolls D100, sets `WasSuccess`.
  - Events: `OnExpired`, `OnResolved(success, overlap, roll)`, `OnStateChanged`.

### UI — `UI/` (all runtime-built)
- **`GameUI.cs`** — top-level UI controller; builds the Canvas, owns child panels, sets
  `PauseMissions` when popups are open, sequences result popups, and hides the roster while
  the assign popup is open.
- **`HUDPanel.cs`** — top bar: score, round #, round timer, failures, gold (per-round/total
  "R/T").
- **`SpiderChart.cs`** *(M2)* — reusable radar-chart `Graphic` drawn in one runtime mesh.
  Modes: **dual** (mission-required vs. combined team), **single** (one agent's skills, for
  mini cards), and **result** (win → green overlap, loss → red uncovered requirement). Label
  modes: names, or names+values. Built via `SpiderChart.Create(...)`.
- **`AgentRosterPanel.cs`** — bottom roster of agent cards (portrait + mini chart + name +
  deployment status). `SetVisible(false)` hides it while the assign popup is open.
- **`MissionSlotUI.cs`** — a mission card on the board.
- **`AssignmentPopup.cs`** — mission detail + agent cards (portrait + mini chart) + live
  team-vs-mission summary chart + success % + Assign button.
- **`ResultPopup.cs`** — resolution result: skill match %, result spider chart (outcome
  coloring), dice roll, agents.
- **`RoundOverPanel.cs`** — end-of-round summary; next round / restart.
- **`UIHelper.cs`** — shared factory helpers (panels, labels, buttons, **portraits**) for
  building UI elements in code.

### Editor — `Editor/` (editor-only assembly)
- **`AgentzDataMenu.cs`** — the **Agentz** menu bar entries that sync data between assets and
  the text override files: *Export/Import agents.csv* and *Export/Import config.json*. Sync is
  explicit and one-way per click (no automatic two-way sync). Import writes to the assets with
  an Undo step. This folder is named `Editor`, so Unity compiles it into the editor-only
  assembly automatically — it is stripped from builds.

## Data & control flow (one mission)
```
MissionSpawner.TrySpawn()
   → new Mission(template, expiry, difficultyScale)      // Waiting
   → OnMissionSpawned ─────────────► GameUI builds a MissionSlotUI

player clicks card → AssignmentPopup (PauseMissions = true)
   → MissionSpawner.AssignAgents(mission, chosen)
   → Mission.Assign(...)                                 // Busy, agents locked

Mission.Tick() counts down busy → Mission.Resolve()
   → SkillSet.CombineAll(agents, mode)
   → scale requirements by difficulty
   → SkillSet.ComputeOverlap(...) → D100 → WasSuccess
   → OnResolved ──► MissionSpawner.HandleResolved
        → GameManager.RegisterSuccess/RegisterFailure
        → free agents
        → OnMissionResolved ──► GameUI shows ResultPopup (PauseMissions = true)
```

## Where M2 / M3 plug in
- **M2 (spider chart + portraits):** ✅ done. `SpiderChart` renders the overlaps; `Mission`
  exposes `CombinedSkills` + `ScaledRequirement` for the truthful result chart; `PortraitLoader`
  fills `AgentData.portrait` at boot. See the UI section above.
- **M3 (upgrade screen):** `RoundOverPanel` → new upgrade UI that spends `GameManager.Gold`
  to raise `AgentData.skills`, priced via `GameConfig.skillUpgradeCost*` (currently unused).

## Known loose ends
- `GameConfig.baseGoldReward` and `roundCompletionBonus` are defined but **not read** by any
  system yet (see GDD §4.5). The end-of-round bonus is currently a hard-coded `RoundNumber ×
  100` added to **Score**.
- Missions, agents, and `GameConfig` are now all externally editable in plain text (CSV / CSV
  / JSON). Precedence differs slightly: agents/config = *text file wins if present*; missions
  = *Inspector pool wins if assigned, so leave it empty to use the CSV*. See
  [EDITING_GUIDE.md](EDITING_GUIDE.md).
- `config.json` serializes `skillCombineMode` as an **int** (0/1/2) — a JsonUtility
  limitation. If human-friendliness matters, revisit with a custom DTO that uses the enum name.
