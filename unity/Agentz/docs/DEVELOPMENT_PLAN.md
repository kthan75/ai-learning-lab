# Agentz — Development Plan

The milestone roadmap and per-milestone breakdown. This is the working plan; the
[GDD](GDD.md) holds the design detail and the [Architecture](ARCHITECTURE.md) doc holds the
code structure. **Last updated: 2026-07-20.**

## Status snapshot
**M0–M4 complete. M5 is next.** All completed work is verified in Play mode and at least one
successful player build.

| Milestone | Scope | Status |
|-----------|-------|--------|
| **M0** | Project scaffolding (ScriptableObjects, `GameConfig`, 6 `AgentData`, GameScene) | ✅ Done |
| **M1** | Core loop — spawn, assign, resolve, multi-round, HUD, round-over screen + next-round/restart | ✅ Done |
| **M2** | Spider/radar chart visuals + agent portraits (assign popup, result popup, roster) | ✅ Done |
| **M3** | Random events (OII + incapacitation) + economy (bonuses, high score) & upgrade screen | ✅ Done |
| **M4** | Intro screens (premise + how-to-play) + art integration & status-colored HUD feedback | ✅ Done |
| **M5** | Content & balance pass | ⬜ **Next** |

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

### M4 — Intro screens + art integration + HUD feedback ✅
- **Intro screens** (`IntroScreens.cs`): a full-art **premise** screen → a **how-to-play**
  screen laid out as a 2×2 bordered grid (instructions · skills legend with icons · a main-HUD
  screenshot · an assign-agents screenshot). Shown in sequence on **every launch** before round
  1; each dismissed by **any key / mouse** (New Input System). Open intro pauses missions.
- **Art integration** via `ArtLoader.cs` (runtime PNG loader from `StreamingAssets/Art/`):
  dispatch-frame background with the HUD/roster/mission board aligned to its regions; "Agentz"
  logo; gold icon; 5 skill icons (on mission cards, upgrade rows, and the assign popup's "Key
  skills required" row); success/failure stamps flanking the result-popup outcome.
- **Status-colored HUD feedback:** each mission card's border + timer bar share one color —
  blue (waiting) → red (time short) → orange (in progress) → green/red (success/fail). A
  timeout now shows the FAILED flash in the box (no popup) instead of vanishing.
- **Deviations from the original plan:** the how-to-play uses two real in-game screenshots
  instead of three illustrated panels; the *nice-to-have* mission-category and incap-status
  icons (Appendix B italics) were not made.

---

## Upcoming milestones

### M5 — Content & balance ⬜ (next)
- Balance pass on difficulty scaling, economy/upgrade costs, and random-event rates.
- Content expansion (more missions; revisit agent spreads).
- Decide the fate of the still-unused `baseGoldReward` config field.
- Any cleanup deferred from earlier milestones.

---

## Plan revision history
- **2026-07-20:** Marked **M4 complete** (intro screens, art integration, status-colored HUD
  feedback) and promoted **M5** to next. Noted the how-to-play screenshot approach and the
  skipped nice-to-have icons.
- **2026-07-12:** Recorded the round-over/next-round flow under **M1** (it was mislabeled M3).
  Repurposed **M3** from "economy & upgrade screen" to **random events + economy/upgrade**
  (the two were kept together). Added the M4 intro-screens scope and the Appendix B image list.
- **2026-07-05:** Initial roadmap captured in the GDD when the project was resumed (M1 done,
  M2 next).

## Beyond MVP (future variants)
The core loop is theme-agnostic; the same systems can be re-skinned by swapping the 5 skill
axes, roster, and mission text (e.g. Covert Ops, Game Dev Studio, Paranormal). See
[GDD §12](GDD.md).
