# CLAUDE.md — Agentz (Unity 2D)

Guidance for AI sessions working on this project. Read the docs before making changes:
[README](README.md) · [GDD](docs/GDD.md) · [Architecture](docs/ARCHITECTURE.md) ·
[Editing Guide](docs/EDITING_GUIDE.md).

## What this is
A real-time dispatch game — assign 1–3 skill-differentiated agents to timed missions, resolve
via overlap%→D100 roll, survive rounds. **M1–M4 are complete** — core loop, spider-chart
visuals + portraits, random events (OII + incapacitation), the economy / upgrade screen
(round & no-fails bonuses, persisted high score), and the M4 presentation layer: intro screens
(premise + how-to-play), integrated art (dispatch frame, logo, skill/gold icons, result
stamps) via `ArtLoader`, and status-colored mission feedback. Next milestone is **M5: content
& balance pass**.

## Working with the user
- **Beginner to Unity.** Any manual Editor work (creating assets, setting Inspector fields,
  wiring the scene) must be given as **explicit step-by-step instructions**. Don't assume
  Unity fluency.
- The user handles Play-mode testing and gives iterative visual/gameplay feedback; Claude
  does the primary coding.
- **Commit after each meaningful milestone or feature batch**; the user verifies on the
  GitHub web UI. Branch: `claude/dazzling-raman`.

## Architecture in one breath
- **Data-driven** ScriptableObjects: `GameConfig` (all tunables), `AgentData`,
  `MissionTemplate`. **All UI is built programmatically at runtime** (no prefabs), on a
  ScreenSpace-Overlay Canvas.
- **Events, not polling:** `GameManager` and `MissionSpawner` raise C# events; the UI
  subscribes. Core logic never reaches into UI.
- `GameBootstrapper.Awake()` constructs GameManager, MissionSpawner, and GameUI.
- Round state machine: **Playing → Draining → RoundOver**. Mission state machine: **Waiting →
  Busy → Resolved**.
- The 5 skills are **ENG, DIP, NAV, SSM (Street Smarts), RES**, 0–10, always in that order
  (`SkillSet.SkillNames` / `SkillAbbreviations`).

## Key facts / conventions
- **Skill combine mode defaults to `Max`** (not Average as the original GDD said) — set in
  `GameConfig.skillCombineMode`. Options: Average / Additive / Max.
- **Success math:** `overlap = Σ min(agent, required) / Σ required`, then (M2) **roll high to
  win:** `success = D100 > (100 − matchPct)` — same odds as the old `≤ matchPct`, but intuitive.
  Requirements are scaled by round difficulty first. In `SkillSet.ComputeOverlap` +
  `Mission.Resolve`. `Mission` also exposes `CombinedSkills` + `ScaledRequirement` for the
  result chart's win/loss coloring.
- **Missions** are editable via `Assets/StreamingAssets/missions.csv` — **but only if the
  scene's Mission Pool slot is empty** (otherwise assigned `MissionTemplate`s win). Fallback:
  `DefaultMissions.cs` (30 missions).
- **All three data sets are plain-text editable (Model B):** missions (`missions.csv`),
  agents (`agents.csv`), config (`config.json`) in `StreamingAssets`. A text file wins over
  its Inspector asset at Play if present (missions excepted — Inspector pool wins, so it's
  left empty). Overrides never mutate the assets; sync explicitly via the **Agentz** Editor
  menu (Export/Import). Loaders: `AgentLoader`, `ConfigLoader`, `MissionLoader`;
  `CsvUtil` is shared.
- **Portraits (M2):** loaded at boot by `PortraitLoader` from
  `StreamingAssets/Portraits/<name-slug>.png` into `AgentData.portrait`; Inspector portraits
  win. Shown on assign-popup + roster cards via `UIHelper.Portrait`/`SetPortrait`.
- **Spider charts (M2):** `SpiderChart` (a `Graphic`) has dual / single-agent / result modes;
  reuse it, don't reinvent. The roster hides while the assign popup is open.
- **Popup pause:** any open popup sets `MissionSpawner.PauseMissions`, freezing all timers
  (mission, spawn, round). Preserve this when adding UI.
- **Defined-but-unused config:** `baseGoldReward`, `roundCompletionBonus`, and the
  `skillUpgradeCost*` fields are not read yet — don't assume they affect gameplay.

## M3 (done) — key facts for future changes
- **Random events** (GDD §4.9): `OpsInfoIncomplete` rolled per mission at spawn (hides the
  assign popup's summary chart + Success % only); `IncapacitationManager` pulls random **idle**
  agents out (event popup + roster countdown). All knobs in `GameConfig`/`config.json`, each
  with an `enable*` toggle; both pause with popups and skip Draining/RoundOver.
- **Economy/upgrade:** `RoundOverPanel` success → `UpgradePanel` (spend `GameManager.TrySpendGold`
  to raise `AgentData.skills`, `−` undo via `RefundGold`). Round + no-fails bonuses in
  `EndRound`; high score persisted in `PlayerPrefs`. GameUI snapshots base skills to reset on
  Play Again.

## M4 (done) — key facts for future changes
- **Runtime art:** `ArtLoader.Load("<file>.png")` decodes from `StreamingAssets/Art/`, cached,
  returns null if missing (callers no-op). Add new imagery this way; don't wire prefab sprites.
- **Intro screens** (`IntroScreens.cs`): premise → how-to-play, dismiss any key/mouse (New Input
  System), shown every launch via `GameUI.ShowIntro`; open intro is a pause source (`_introOpen`).
- **Mission-card status color** lives in `MissionSlotUI.SetAccent` (border + timer bar together).
  Timeouts flash FAILED in place (no popup); `HasMission` keeps a flashing slot from being reused.

## Where the next milestone plugs in
- **M5 — content & balance:** balance difficulty scaling / economy / event rates; expand
  missions and revisit agent spreads; decide the fate of the unused `baseGoldReward`. Not started.

## Gotchas
- Editing a `Default*.cs` file changes *defaults/fallbacks*, not the scene assets already in
  use. To change live values, edit the assets (or ask the user to).
- Keep the canonical skill order everywhere; several arrays index by it.
- Don't reformat unrelated code; match the existing style (aligned fields, `//` section
  banners).
