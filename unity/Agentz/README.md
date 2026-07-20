# Agentz

A fast-paced, real-time dispatch game: assign skill-differentiated agents to timed missions
on a chaotic space station, resolve them with a visible dice roll, and survive as many
rounds as you can. Built in **Unity (2D)**.

**Status:** M1–M4 complete — playable core loop, skill-matching visuals (spider charts, agent
portraits), random events (OII + agent incapacitation), the economy / between-round upgrade
screen (round & no-fails bonuses, persisted high score), and the M4 presentation layer: intro
screens (premise + how-to-play), integrated art (dispatch frame, logo, skill/gold icons,
result stamps), and status-colored mission feedback. Next up: **M5** — content & balance pass.

## Documentation
- **[docs/GDD.md](docs/GDD.md)** — Game Design Document (design intent + how it's actually
  implemented, with drift notes). Start here.
- **[docs/DEVELOPMENT_PLAN.md](docs/DEVELOPMENT_PLAN.md)** — milestone roadmap, per-milestone
  breakdown, and what's next.
- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** — code structure, boot sequence, data
  flow, and where each milestone plugs in.
- **[docs/EDITING_GUIDE.md](docs/EDITING_GUIDE.md)** — where every agent, mission, timer and
  balance value lives and how to edit it (plain-text vs. Unity).

## Quick start
1. Open `unity/Agentz/` in Unity.
2. Open the game scene and press **Play**.
3. `GameBootstrapper` builds all systems and UI at runtime — no prefab wiring needed.

## Tuning without opening code
All game data is editable as plain text in `Assets/StreamingAssets/` (Model B — a text file
wins over its Unity asset at Play if present). Use the **Agentz** menu in Unity to create the
files from current values (Export) or push edits back into assets (Import).
- **Missions:** `missions.csv` — used when the scene's Mission Pool slot is left empty.
- **Agents:** `agents.csv` — wins over the Inspector roster if present.
- **Timers / rules / economy:** `config.json` — overrides the `GameConfig` asset (per field).

See [docs/EDITING_GUIDE.md](docs/EDITING_GUIDE.md) for formats, gotchas, and the sync menu.

## Project layout
```
unity/Agentz/Assets/
  Scripts/
    Config/   GameConfig, ConfigLoader           # tunables + config.json overrides
    Data/     SkillSet, AgentData, MissionTemplate, Default*, MissionLoader,
              AgentLoader, PortraitLoader, CsvUtil  # data model + loaders + core math
    Core/     GameManager, MissionSpawner, Mission, GameBootstrapper
    UI/       GameUI + panels/popups + SpiderChart (all built at runtime)
    Editor/   AgentzDataMenu                     # Export/Import sync menu (editor-only)
  StreamingAssets/  missions.csv, agents.csv, config.json, Portraits/  # plain-text data
docs/                                            # this documentation set
```
