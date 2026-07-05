# Agentz

A fast-paced, real-time dispatch game: assign skill-differentiated agents to timed missions
on a chaotic space station, resolve them with a visible dice roll, and survive as many
rounds as you can. Built in **Unity (2D)**.

**Status:** M1 + M2 complete — playable core loop plus skill-matching visuals (spider charts,
agent portraits). Next up: **M3**, economy polish & the between-round upgrade screen.

## Documentation
- **[docs/GDD.md](docs/GDD.md)** — Game Design Document (design intent + how it's actually
  implemented, with drift notes). Start here.
- **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** — code structure, boot sequence, data
  flow, and where M2/M3 plug in.
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
