# CLAUDE.md — Agentz (Unity 2D)

Guidance for AI sessions working on this project. Read the docs before making changes:
[README](README.md) · [GDD](docs/GDD.md) · [Architecture](docs/ARCHITECTURE.md) ·
[Editing Guide](docs/EDITING_GUIDE.md).

## What this is
A real-time dispatch game — assign 1–3 skill-differentiated agents to timed missions, resolve
via overlap%→D100 roll, survive rounds. **M1 (playable core loop) is complete.** Next
milestone is **M2: the spider/radar chart visual** for skill matching.

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
- **Success math:** `overlap = Σ min(agent, required) / Σ required`, then
  `success = D100 <= round(overlap × 100)`. Requirements are scaled by round difficulty
  first. All in `SkillSet.ComputeOverlap` + `Mission.Resolve`.
- **Missions** are editable via `Assets/StreamingAssets/missions.csv` — **but only if the
  scene's Mission Pool slot is empty** (otherwise assigned `MissionTemplate`s win). Fallback:
  `DefaultMissions.cs` (30 missions).
- **All three data sets are plain-text editable (Model B):** missions (`missions.csv`),
  agents (`agents.csv`), config (`config.json`) in `StreamingAssets`. A text file wins over
  its Inspector asset at Play if present (missions excepted — Inspector pool wins, so it's
  left empty). Overrides never mutate the assets; sync explicitly via the **Agentz** Editor
  menu (Export/Import). Loaders: `AgentLoader`, `ConfigLoader`, `MissionLoader`;
  `CsvUtil` is shared.
- **Popup pause:** any open popup sets `MissionSpawner.PauseMissions`, freezing all timers
  (mission, spawn, round). Preserve this when adding UI.
- **Defined-but-unused config:** `baseGoldReward`, `roundCompletionBonus`, and the
  `skillUpgradeCost*` fields are not read yet — don't assume they affect gameplay.

## Where the next milestones plug in
- **M2 (spider chart):** the math already exists; add a **UI widget** that draws two overlaid
  pentagons (mission required vs. combined agents) in `AssignmentPopup` and `ResultPopup`. No
  core-logic change needed.
- **M3 (upgrade screen):** extend `RoundOverPanel`; spend `GameManager.Gold` to raise
  `AgentData.skills`, priced via the (currently unused) `GameConfig.skillUpgradeCost*`.

## Gotchas
- Editing a `Default*.cs` file changes *defaults/fallbacks*, not the scene assets already in
  use. To change live values, edit the assets (or ask the user to).
- Keep the canonical skill order everywhere; several arrays index by it.
- Don't reformat unrelated code; match the existing style (aligned fields, `//` section
  banners).
