# Agentz — Development Plan

The milestone roadmap and per-milestone breakdown. This is the working plan; the
[GDD](GDD.md) holds the design detail and the [Architecture](ARCHITECTURE.md) doc holds the
code structure. **Last updated: 2026-07-12.**

## Status snapshot
**M0–M3 complete. M4 is next.** All completed work is verified in Play mode and at least one
successful player build.

| Milestone | Scope | Status |
|-----------|-------|--------|
| **M0** | Project scaffolding (ScriptableObjects, `GameConfig`, 6 `AgentData`, GameScene) | ✅ Done |
| **M1** | Core loop — spawn, assign, resolve, multi-round, HUD, round-over screen + next-round/restart | ✅ Done |
| **M2** | Spider/radar chart visuals + agent portraits (assign popup, result popup, roster) | ✅ Done |
| **M3** | Random events (OII + incapacitation) + economy (bonuses, high score) & upgrade screen | ✅ Done |
| **M4** | Intro screens (premise + how-to-play) + visual polish / game feel | ⬜ **Next** |
| **M5** | Content & balance pass | ⬜ Pending |

---

## Completed milestones

### M0 — Scaffolding ✅
Unity 2D project, ScriptableObject data model (`AgentData`, `MissionTemplate`, `GameConfig`),
6 starting agents, `GameScene` with `GameBootstrapper`.

### M1 — Playable core loop ✅
Spawn → assign → resolve (overlap % → D100) → multi-round flow. Round state machine
(Playing → Draining → RoundOver), HUD (round/total gold & score, timer, failures), mission
board, assignment/result popups with popup-pause, and the **round-over summary + next-round /
restart** flow. Missions data-driven from `StreamingAssets/missions.csv`.

### M2 — Skill-matching visuals ✅
- `SpiderChart` widget (dual mission-vs-team / single-agent / result modes).
- Assign popup: live team-vs-mission chart + per-agent cards (portrait + mini chart).
- Result popup: outcome-colored chart (green overlap / red uncovered requirement).
- Roster redesigned (portrait + mini chart + deployment status); hidden while assigning.
- Agent **portraits** loaded from `StreamingAssets/Portraits/` (`PortraitLoader`, Model B).
- Roll direction flipped to "bigger is better."

### M3 — Random events + economy/upgrade ✅
- **OII (Ops Info Incomplete):** per-mission chance to hide the success preview.
- **Incapacitation:** random idle-agent downtime with a modal event popup + roster countdown.
- **Upgrade screen:** flat grid to spend gold on skills (`+`), with a `−` undo.
- **Economy:** round-completion bonus + no-fails bonus; persisted **high score** (PlayerPrefs).
- **Bug fix:** Game Over now triggers on the failure limit even during the Draining phase.
- Full mechanics in [GDD §4.5, §4.9, §5](GDD.md); all knobs in `GameConfig` → `config.json`.

---

## Upcoming milestones

### M4 — Intro screens + visual polish ⬜ (next)
**Intro screens** (see [GDD §7](GDD.md)):
- A short, funny **premise** screen.
- A brief **how-to-play** screen (text + images).
- Shown in sequence on **every launch**, before round 1; each dismissed by **any key / mouse
  button**.

**Visual polish / game feel:**
- Wire in the generated art (backgrounds, logo, skill/gold icons, success/failure stamps).
- General layout polish and feedback (transitions, emphasis) as time allows.

**Art dependency:** images are being generated in parallel — the required-image list (with
descriptions and sizes) is in [GDD Appendix B](GDD.md). Essentials: dispatch background,
premise art, 3 how-to-play panels, "Agentz" logo, 5 skill icons, gold icon, success/failure
stamps.

*Open question to resolve when starting M4:* exact placement of each asset and whether to add
a "New Highscore!" flourish and/or an at-a-glance OII marker on mission cards.

### M5 — Content & balance ⬜
- Balance pass on difficulty scaling, economy/upgrade costs, and random-event rates.
- Content expansion (more missions; revisit agent spreads).
- Decide the fate of the still-unused `baseGoldReward` config field.
- Any cleanup deferred from earlier milestones.

---

## Plan revision history
- **2026-07-12:** Recorded the round-over/next-round flow under **M1** (it was mislabeled M3).
  Repurposed **M3** from "economy & upgrade screen" to **random events + economy/upgrade**
  (the two were kept together). Added the M4 intro-screens scope and the Appendix B image list.
- **2026-07-05:** Initial roadmap captured in the GDD when the project was resumed (M1 done,
  M2 next).

## Beyond MVP (future variants)
The core loop is theme-agnostic; the same systems can be re-skinned by swapping the 5 skill
axes, roster, and mission text (e.g. Covert Ops, Game Dev Studio, Paranormal). See
[GDD §12](GDD.md).
