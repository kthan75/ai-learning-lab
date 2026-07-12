# Agentz — Game Design Document

**Version:** 1.3 (expanded from v1.2)
**Status:** M1 + M2 complete (core loop + skill-matching visuals). M3–M5 pending.
**Engine:** Unity 2D · **Platform:** PC (MVP), mobile later · **Team:** Solo (+ AI coding)

> This document is the living design spec. It began as `Agentz_GDD_v1.2.docx` and has
> been expanded with (a) the mechanics as actually implemented in M1 and (b) explicit
> notes wherever the shipped code differs from the original design intent. Those notes are
> marked **⚙️ Implemented as** and **⚠️ Design drift**. Keep them in sync as the game evolves.

---

## 1. Executive Summary

Agentz is a fast-paced, real-time management game built around a simple but highly
engaging core mechanic: assigning agents with different skill distributions to
time-sensitive missions under pressure. Set in a vibrant space-opera universe, the player
is an operations coordinator on a bustling, multi-species space station, constantly
responding to emergencies, conflicts, and unpredictable events. The fantasy emphasizes
**control within chaos** — juggling limited resources while the station's needs escalate
in both frequency and complexity.

At its core, gameplay is a continuous dispatch loop. Missions appear dynamically on a
central dashboard, each defined by a mix of five skills. The player selects up to 3 agents
from a fixed roster and assigns them before the mission's timer expires. Once assigned,
agents are temporarily locked while the mission resolves. Success is determined by how
closely the agents' combined skills match the mission requirements, translated into a
probability and resolved via a visible dice roll. Failed or ignored missions count against
a limited failure threshold, creating constant tension.

The game is structured in short, high-intensity rounds. Players survive as long as
possible while optimizing decisions in real time. Between rounds they upgrade agents with
earned currency. The system is intentionally clean and modular, allowing rapid iteration
and future re-skins onto the same core mechanics.

---

## 2. Core Concept

- **Game idea:** Real-time resource allocation — assign skill-differentiated agents to
  timed missions, survive as many rounds as possible.
- **Player fantasy (MVP):** Oversee a chaotic multi-species space station, coordinating
  agents to resolve emergencies, conflicts, and strange incidents.
- **Reference inspiration:** *Dispatch* (core mechanic), BG3-style dice-roll probability
  feedback, *Overcooked*-style multitasking pressure (pacing).
- **Working title:** Agentz

### MVP philosophy
- Build the core loop first.
- Keep everything system-driven and data-tunable.
- Avoid content-heavy features (narrative, unique abilities) until the loop is fun.

---

## 3. Core Gameplay Loop

### 3.1 Moment-to-moment
1. Missions appear on the dashboard at random intervals (**3–6 s**).
2. Each mission has a 5-skill requirement profile (0–10 each), an expiry timer, and up to
   **3** agent slots.
3. Player selects **1–3** available agents from the pool of **6** and clicks **Assign**.
4. Mission enters a **Busy** state (**7 s**); assigned agents are locked for that time.
5. On completion the system computes skill overlap %, performs a **D100** roll, and shows
   the success chance and the roll result.
6. Outcome: **Success →** gold reward; **Failure →** counts toward the failure limit.

**⚙️ Implemented as:** matches design. Spawn interval, slot cap, busy duration, and expiry
are all read from `GameConfig` (see [EDITING_GUIDE](EDITING_GUIDE.md)). Mission expiry
timer defaults to **20 s** (the original doc didn't specify a number).

### 3.2 Round structure
- Each round lasts **3 minutes** (`roundDuration = 180 s`).
- Player must stay under **4** failures (`failureLimit`).
- Max **4** active missions at once (`maxActiveMissions`).
- Expired missions count as failures.

**⚙️ Implemented as:** a three-phase state machine per round — **Playing → Draining →
RoundOver**. When the round timer hits 0 the round enters *Draining*: no new missions
spawn and any still-*Waiting* missions are cleared **without** a failure penalty, but
already-*Busy* missions finish resolving so agents free up and results still display. Once
all in-flight missions finish, the round ends. A round can also end early by hitting the
failure limit during *Playing*.

### 3.3 Meta loop (between rounds)
- Player earns gold.
- Player upgrades agents by spending gold on specific skill values.
- No XP system in MVP (planned later).

**⚠️ Design drift / not yet built:** the between-round **upgrade screen is not
implemented** (planned for M3). `GameConfig` already carries the upgrade-cost knobs
(`skillUpgradeCostBase`, `skillUpgradeCostRamp`) but nothing consumes them yet.

### 3.4 Retention driver
- Endless survival structure.
- Increasing difficulty over time.
- Optimization of agent builds.
- Score = rounds survived + missions completed.

**⚙️ Implemented as:** difficulty scales each round — mission skill requirements are
multiplied by `skillScalePerRound` (default **1.05**, cumulative) and the spawn interval
shrinks by `spawnIntervalReductionPerRound` per round (floored at `minSpawnInterval`). See
§4.7.

---

## 4. Game Systems

### 4.1 Skill system
Each agent (and each mission) is described by the **same five skills**, valued **0–10**:

| Axis | Abbrev. | Flavor |
|------|---------|--------|
| Engineering   | ENG | Repairs, overrides, technical faults |
| Diplomacy     | DIP | Negotiation, de-escalation, delegations |
| Navigation    | NAV | Piloting, routing, tracking, search |
| Street Smarts | SSM | Investigation, informants, covert work |
| Resilience    | RES | Endurance, crisis nerves, physical danger |

Skills are visualized as a **radar / spider chart** (built in M2 — see `SpiderChart`).

> **Naming note:** Street Smarts is abbreviated **SSM** everywhere (code field is
> `streetSmarts`) for consistency with the other three-letter codes. The canonical order
> is always ENG, DIP, NAV, SSM, RES — see `SkillSet.SkillNames` / `SkillAbbreviations`.

### 4.2 Mission system
- Each mission has a skill-requirement profile, an expiration timer, and up to 3 slots.
- Flow: **Appears → Assign → Busy (7 s) → Roll → Result**.
- Once a mission enters **Busy** it can no longer expire.
- Missions spawn at random 3–6 s intervals; max 4 active at once.

**⚙️ Implemented as:** `Mission` is a plain C# state machine (`Waiting → Busy →
Resolved`). There are **30 missions** across 3 difficulty tiers in the built-in pool, and
missions can be edited without Unity via `StreamingAssets/missions.csv` (see
[EDITING_GUIDE](EDITING_GUIDE.md)).

### 4.3 Skill matching & mission resolution
Agents and missions use the same 5-skill model; overlap determines success probability.

**Original design (v1.2):** combined agent skill per axis = **average** of the assigned
agents' values, e.g. `combinedEng = average(eng_a1, eng_a2)`. The mission's spider chart is
overlapped with the combined-agent chart to produce an overlap %, which is the success
chance. A die is rolled against that chance.

**⚙️ Implemented as / ⚠️ design drift — skill combination:** the game supports **three
combine modes**, selectable in `GameConfig.skillCombineMode`, and **defaults to `Max`**
(not `Average`) after M1 tuning:

- **Average** — simple mean across agents (the original spec; adding a weak agent can
  *lower* a score).
- **Additive** — sum per axis, capped at 10 (more agents always helps).
- **Max** *(current default)* — best value per axis, so each agent contributes their
  strongest skills and a 3-agent team covers more of the chart.

`Max` was chosen because it makes team composition feel additive and forgiving, which suits
the fast, high-pressure pace. If we ever want "weak agents dilute the team," switch back to
`Average`.

**⚙️ Implemented as — overlap formula:** for each axis, overlap contribution =
`min(agentSkill, missionRequired)`. The success chance is
`Σ min(agent, required) / Σ required`, clamped to `[0,1]`. A mission with zero total
requirement trivially succeeds. This is an area-style overlap: the team only gets credit up
to what each axis actually demands.

**⚙️ Implemented as — resolution:** required skills are first multiplied by the round's
difficulty scale (capped at 10 per axis). Then `DiceRoll = Random(1..100)` and
`success = DiceRoll > (100 − matchPct)` — i.e. **roll high to win** (identical odds to the
old `≤ matchPct`, but intuitive: bigger is better). Both the overlap % and the roll are
surfaced to the UI so the outcome is legible (BG3-style). The result popup also colors the
outcome on the spider chart — the covered overlap green on a win, the uncovered requirement
red on a loss — so the player sees *where* the roll landed.

### 4.4 Failure system
- Failure = a failed roll **or** an expired (Waiting) mission.
- The round ends at 4 failures.

**⚙️ Implemented as:** matches design. Note that missions cleared during the *Draining*
phase do **not** count as failures (the round is already ending).

### 4.5 Economy
- Currency: **Gold**, earned from successful missions.
- Optional round-completion bonus.
- **Streak bonus** after 5 consecutive successes.

**⚙️ Implemented as:**
- Each success awards that mission's own `goldReward` (from the CSV / template).
- Once `ConsecSuccesses >= streakBonusThreshold` (default 5), each further success adds
  `streakBonusGold` (default +5). A failure resets the streak.
- Gold is tracked both **per round** and **cumulative total** (HUD shows both as "R/T").

**⚠️ Defined but not yet wired:** `baseGoldReward` and `roundCompletionBonus` exist in
`GameConfig` but nothing reads them yet (missions use their own reward, and the end-of-round
bonus currently applies to **Score**, as `RoundNumber × 100`, not to gold). Decide during
M3 whether to wire these up or remove them.

### 4.6 Agent system (MVP)
- Fixed roster of **6** agents.
- Same five skills, different distributions (one specialist per axis + one generalist).

**⚙️ Implemented as:** the starting roster (names, bios, skill spreads) lives in
`DefaultAgents.cs` and is mirrored into `AgentData` ScriptableObject assets used by the
scene. See [EDITING_GUIDE](EDITING_GUIDE.md) §Agents.

| # | Agent | Specialty | ENG/DIP/NAV/SSM/RES |
|---|-------|-----------|---------------------|
| 1 | Zara 'Sparks' Vex     | Engineering   | 5/1/2/1/3 |
| 2 | Ambassador Rell'ik    | Diplomacy     | 1/4/2/3/2 |
| 3 | Captain Mira Orvath   | Navigation    | 2/2/5/2/2 |
| 4 | 'Ghost' Sable         | Street Smarts | 1/2/3/5/1 |
| 5 | Sergeant Brutus Kael  | Resilience    | 2/2/1/3/5 |
| 6 | Renn Duskfall         | Generalist    | 3/1/3/1/3 |

> These are the *current tuned* values (committed 2026-04-26). The scene's `AgentData`
> assets are the runtime source of truth; keep `DefaultAgents.cs` in sync when rebalancing.

**Future extensions:** recruitment, traits/abilities, XP progression.

### 4.7 Difficulty scaling *(added — not in v1.2)*
Difficulty ramps each round via `GameConfig`:
- **Skill scaling:** required skills ×= `skillScalePerRound^(round-1)` (default 1.05), capped
  at 10 per axis.
- **Spawn pacing:** min/max spawn intervals shrink by
  `spawnIntervalReductionPerRound × (round-1)`, floored at `minSpawnInterval`.

### 4.8 Popup pause *(added — not in v1.2)*
Whenever any popup is open (assignment or result), **all timers freeze** — mission expiry,
busy countdowns, the spawn timer, and the round clock (`MissionSpawner.PauseMissions`). This
keeps the real-time pressure fair: reading a result never costs you the round.

### 4.9 Random events / run modifiers *(planned — M3)*
Two random systems add variety and tension. Both **pause with popups** (reuse
`PauseMissions`) and do not fire during Draining/RoundOver. All values below live in
`GameConfig` (so they're editable in `config.json`); master toggles `enableOII` /
`enableIncapacitation` (default on) gate each system.

**Ops Info Incomplete (OII).** Some missions spawn with intel gaps: the assign popup can't
show a success prediction. When a mission is OII, the popup **hides the team-vs-mission
summary chart and the Success % only**, replacing them with the text *"Ops info incomplete.
Success rate unknown."* Agent portraits and their per-agent mini charts stay visible — you
still choose a team, you just gamble on the odds. The **result popup shows normally** (you
learn the outcome after). OII is rolled **once per mission at spawn** and fixed for that
mission.
- `startingOccurrenceOII` (default **0.25**) — base chance a mission is OII.
- `roundScalingOII` (default **0.05**) — added per round.
- `maxOccurrenceOII` (default **0.75**) — cap.
- Effective chance = `min(startingOccurrenceOII + roundScalingOII × (round−1), maxOccurrenceOII)`
  (round 1 = 25%).

**Agent incapacitation.** Idle agents can be temporarily pulled out of action for a flavor
reason. **Only idle (available) agents are eligible** — an agent on a mission is never
incapacitated. Incapacitated agents are unselectable in the assign popup and show their
reason + remaining time on the roster card; they return automatically when the timer ends.
- Reason (random, from a small code-defined list): *"Agent injured."*, *"Personal
  emergency."*, *"Stuck doing paperwork."*
- Duration: random in `[incapDurationMin, incapDurationMax]` (default **3–7 s**).
- Cadence: after `incapSafeTime` (default **10 s**, no incapacitations in a round's opening),
  every `incapOccurrenceRate` seconds (default **5 s**) roll `incapOccurrenceChance` (default
  **20%**); on a hit, a random eligible agent is incapacitated.

---

## 5. Player Progression
- Upgrade agent stats with gold; optimize distributions; survive longer runs.
- **Future:** XP, leveling, unlockable agents, meta progression.

*(Upgrade UI is M3 — see §3.3.)*

---

## 6. Style & Presentation
- Stylized 2D visuals; static dashboard camera; light, dynamic tone.

**⚙️ Implemented as:** all UI is built **programmatically at runtime** (no prefabs), on a
ScreenSpace-Overlay Canvas. Visual polish and game-feel are the M4 focus.

---

## 7. UX / Interaction
- **Main Dispatch screen:** mission board, agent roster at the bottom, global stats on top
  (score, round number, round timer, failures remaining, total gold).
- **Mission popup:** title & description, required-skill indication, 3 agent slots, Assign
  button. Click-to-select an agent, click a slot to assign, click **Assign** to start and
  close the popup.
- **Result popup:** after the busy time, shows the spider-chart overlap (mission vs.
  combined agents), success % chance, and the dice roll with its result.
- **Upgrade screen:** opens after each round; click an agent portrait to open their page and
  spend gold to raise skills.
- **Intro screens *(planned — M4)*:** on every launch, before the first round, two full-screen
  screens shown in sequence — (1) a short, funny **premise** blurb, (2) a brief **how-to-play**
  (text + images). Each is dismissed by **any key or mouse button**.

**⚙️ Implemented in M1:** dispatch screen, mission cards with skill hints, assignment popup,
result popups (numeric overlap % + roll), roster cards with a "deployed" label, sequenced
popups (results queue and pause the game), and the **round-over summary + next-round /
restart** flow.
**⚙️ Added in M2:** live spider charts in the assign popup (team vs. mission) and result
popup (with win/loss overlap coloring); per-agent cards showing **portrait + mini chart** in
both the assign popup and the roster; the roster hides while the assign popup is open.
Agent **portraits** load from `StreamingAssets/Portraits/` (see EDITING_GUIDE).
**⚠️ Not yet built:** OII + incapacitation random events and the **upgrade screen** (M3);
intro screens + visual polish (M4).

---

## 8. Constraints / Design Principles
- High decision speed. Clarity over realism. Readable and tunable systems.

---

## 9. MVP Definition
Assign agents to missions · resolve via dice roll · survive timed rounds · core systems
functional. **→ Achieved in M1.**

---

## 10. Out of Scope (MVP)
Narrative system · unique abilities · recruitment · advanced balancing · mobile
optimization.

---

## 11. Milestone Roadmap

| Milestone | Scope | Status |
|-----------|-------|--------|
| **M0** | Project scaffolding (ScriptableObjects, GameConfig, 6 AgentData, GameScene) | ✅ Done |
| **M1** | Core loop — spawn, assign, resolve, multi-round, HUD, **round-over screen + next-round/restart** | ✅ Done |
| **M2** | Spider/radar chart visuals + agent portraits (assign popup, result, roster) | ✅ Done |
| **M3** | **Random events** (OII + incapacitation, §4.9) **+ economy & between-round upgrade screen** | ⬜ Next |
| **M4** | **Intro screens** (premise + how-to-play) + visual polish / game feel (see Appendix B image list) | ⬜ Pending |
| **M5** | Content & balance pass | ⬜ Pending |

> **Plan note (2026-07-12):** the round-over summary + next-round flow, originally listed under
> M3, was actually delivered in M1 and is recorded there now. M3 was repurposed to add the
> random-event systems alongside the economy/upgrade work.

---

## 12. Future Variants
The system is deliberately **theme-agnostic** — the same core loop can be re-skinned by
swapping the five skill axes, agent roster, and mission text. Planned/brainstormed skins:

| Variant | Fantasy | Skills | Tone |
|---------|---------|--------|------|
| **Covert Ops Agency** | Run a covert intelligence agency, coordinating high-risk global ops. | Stealth, Hacking, Social, Intelligence, Combat | Serious, efficient, dry |
| **Escort Agency** *(adult)* | Manage a high-end escort agency, matching agents to demanding clients. | Charm, Intelligence, Discretion, Passion, Conversation | Stylish, suggestive, satirical |
| **Game Dev Studio (Age of AI)** | Run a studio surviving the AI era where everyone can do everything but execution matters. | Programming, Art, Design, QA, Communication | Satirical, industry-aware, chaotic |
| **Criminal Operations** | Run a criminal network balancing speed, precision, and exposure. | Stealth, Force, Hacking, Deception, Driving | Gritty, tense, dark humor |
| **Paranormal Investigation** | Coordinate agents dealing with supernatural phenomena. | Occult, Courage, Investigation, Intuition, Physical | Eerie / quirky / absurd |

Because agents, missions, and skill axes are data-driven, a re-skin is primarily a content
and label swap — the resolution math is shared.

---

## Appendix A — Key tuning defaults (M1)
See [EDITING_GUIDE.md](EDITING_GUIDE.md) for where to change each of these.

| Setting | Default | Field |
|---------|---------|-------|
| Round duration | 180 s | `roundDuration` |
| Failure limit | 4 | `failureLimit` |
| Spawn interval | 3–6 s | `missionSpawnIntervalMin/Max` |
| Max active missions | 4 | `maxActiveMissions` |
| Mission expiry | 20 s | `missionExpiryDuration` |
| Busy duration | 7 s | `missionBusyDuration` |
| Skill combine mode | Max | `skillCombineMode` |
| Streak threshold / bonus | 5 / +5 gold | `streakBonusThreshold` / `streakBonusGold` |
| Skill scale per round | ×1.05 | `skillScalePerRound` |

### Random events (M3 — §4.9)
| Setting | Default | Field |
|---------|---------|-------|
| OII enabled | on | `enableOII` |
| OII base chance | 25% | `startingOccurrenceOII` |
| OII per-round scaling | +5% | `roundScalingOII` |
| OII cap | 75% | `maxOccurrenceOII` |
| Incapacitation enabled | on | `enableIncapacitation` |
| Incap check interval | 5 s | `incapOccurrenceRate` |
| Incap chance per check | 20% | `incapOccurrenceChance` |
| Incap safe time (round start) | 10 s | `incapSafeTime` |
| Incap duration range | 3–7 s | `incapDurationMin/Max` |

---

## Appendix B — M4 visual-polish asset list
Images to generate for M4. Transparent PNG for icons/logo; 2560×1440 for backgrounds
(covers QHD, scales down cleanly). Essentials first; *italic* = nice-to-have.

| Asset | Description | Size |
|-------|-------------|------|
| Dispatch background | Sci-fi ops-center backdrop, dark/muted so UI stays readable | 2560×1440 |
| Premise screen art | Evocative "chaotic space station" scene for the intro | 2560×1440 |
| How-to-play panels ×3 | Illustrations: (1) missions appear, (2) pick agents, (3) dice/resolution | ~700×450 each |
| Game logo | "Agentz" title treatment, transparent | ~1000×400 |
| Skill icons ×5 | ENG / DIP / NAV / SSM / RES glyphs, transparent | 128×128 each |
| Gold / credits icon | Currency symbol, transparent | 96×96 |
| Success / Failure stamps | Stylized ✓ and ✗ for the result popup, transparent | 256×256 each |
| *Mission-category icons ×~6* | Fire/repair, diplomacy, navigation, security, medical, generic | 128×128 each |
| *Incap status icons ×3* | Injured / emergency / paperwork | 64×64 each |
