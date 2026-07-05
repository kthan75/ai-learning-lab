# Agentz — Editing Guide (where the tunable data lives)

This is the "I just want to change a number" reference: every agent, mission, timer, and
balance value, and **how to edit it** — with a clear split between what you can change in a
**plain text file** and what currently needs the **Unity Editor**.

> **TL;DR — all three are now plain-text editable:**
> - ✅ **Missions** — `StreamingAssets/missions.csv` (used when the scene's Mission Pool is empty).
> - ✅ **Agents** — `StreamingAssets/agents.csv` (wins over the Inspector roster if present).
> - ✅ **Timers / rules / economy** — `StreamingAssets/config.json` (overrides the `GameConfig`
>   asset if present; you only need to include the fields you want to change).
>
> **Precedence ("Model B"):** a text file, **if it exists**, wins at Play time; if it's
> absent, the Unity asset is used exactly as before. Nothing syncs automatically both ways —
> use the **Agentz** menu in Unity to Export (assets → text) or Import (text → assets) on
> demand. See [§4](#4-syncing-between-text-and-unity-the-agentz-menu).
>
> **First time:** the `agents.csv` / `config.json` files don't exist yet. Create them safely
> from your *current tuned values* with **Agentz → Export Agents / Export Config** (do this
> rather than hand-writing them, so you start from exactly what's in your assets).

---

## 1. Missions — ✅ plain text (CSV)

**File:** `unity/Agentz/Assets/StreamingAssets/missions.csv`
Edit it in any text editor or spreadsheet. Changes take effect on the next Play — **no Unity
recompile needed**.

### Format
First row is a header (ignored). One mission per row, 9 columns:

```
Title,Description,ENG,DIP,NAV,SSM,RES,Gold,Tier
"Reactor Coolant Leak","Pressure is dropping in bay 7... [ENG, RES]",7,1,2,2,4,10,1
```

| Column | Meaning | Range / notes |
|--------|---------|---------------|
| `Title` | Mission name | Wrap in `"..."` if it contains a comma |
| `Description` | Shown in the popup; convention is to end with skill hints like `[ENG, RES]` | Wrap in `"..."`; use `""` for a literal quote |
| `ENG` `DIP` `NAV` `SSM` `RES` | Required skill per axis | 0–10 (decimals allowed) |
| `Gold` | Reward on success | integer ≥ 0 |
| `Tier` | Difficulty tier (currently informational / weighting) | integer 1–3 in the built-in set |

### Rules & gotchas
- **The CSV is only used when the scene's mission pool is empty.** In the scene,
  `GameBootstrapper` has an optional **Mission Pool** slot. If any `MissionTemplate` assets
  are assigned there, **those win and the CSV is ignored**. Leave that slot empty to use the
  CSV. *(This is the #1 "why aren't my edits showing up" cause.)*
- Any row that fails to parse (wrong column count, non-numeric skill) is **skipped with a
  warning** in the Unity Console; the rest still load.
- If the file is missing or every row is invalid, the game falls back to the **30 built-in
  missions** in `DefaultMissions.cs`.
- Fields with commas **must** be quoted. Escape a literal `"` inside a quoted field by
  doubling it (`""`).
- Watch out: spreadsheet apps may "smart-quote" or change the em-dash `—`. Save as plain
  **UTF-8 CSV**.

### The built-in fallback set
`unity/Agentz/Assets/Scripts/Data/DefaultMissions.cs` — 30 missions, same fields as the CSV.
This is the safety net if the CSV is absent. Keeping it in sync with the CSV is optional; the
CSV is the intended edit surface.

---

## 2. Agents — ✅ plain text (CSV) or Unity

**File:** `unity/Agentz/Assets/StreamingAssets/agents.csv`
If this file exists, it **wins** over the Inspector roster at Play (Model B). If it's absent,
the scene's `AgentData` assets are used as before.

### Format
First row is a header (ignored). One agent per row, 7 columns:

```
Name,Bio,ENG,DIP,NAV,SSM,RES
"Zara 'Sparks' Vex","Former station mechanic turned field operative...",5,1,2,1,3
```

| Column | Meaning | Range / notes |
|--------|---------|---------------|
| `Name` | Agent name | Wrap in `"..."` if it contains a comma |
| `Bio` | Flavor text shown on the card | Wrap in `"..."`; `""` for a literal quote |
| `ENG` `DIP` `NAV` `SSM` `RES` | Skill per axis | 0–10 (decimals allowed) |

### How to start & edit
1. **Create the file from your current agents:** in Unity, **Agentz → Export Agents to
   agents.csv**. This reads your actual tuned `AgentData` assets, so you start from the real
   values (don't hand-write it from scratch — you'd risk diverging from your tuning).
2. Edit `agents.csv` in any text editor / spreadsheet. Changes apply on the next Play.
3. Optional: to also update the Unity assets to match the file, use **Agentz → Import
   agents.csv into Agent assets** (matches rows to assets by **Name**).

### Rules & gotchas
- **Portraits** aren't in the CSV. At Play, a CSV agent whose `Name` matches an existing
  `AgentData` asset keeps that asset's portrait; an all-new name has no portrait (fine in MVP).
- Rows that fail to parse (wrong column count, non-numeric skill) are **skipped with a
  warning**; the rest still load. If every row is invalid, it falls back to the Inspector
  roster, then to `DefaultAgents.cs`.
- Save as plain **UTF-8 CSV**; watch for smart-quotes / changed em-dashes.

### The built-in fallback set (`DefaultAgents.cs`)
`unity/Agentz/Assets/Scripts/Data/DefaultAgents.cs` — the code-level roster, used only if
there's no CSV **and** no Inspector roster. Order is always **eng, dip, nav, ss (Street
Smarts), res**. Current tuned roster:

| Agent | ENG | DIP | NAV | SSM | RES |
|-------|-----|-----|-----|-----|-----|
| Zara 'Sparks' Vex | 5 | 1 | 2 | 1 | 3 |
| Ambassador Rell'ik | 1 | 4 | 2 | 3 | 2 |
| Captain Mira Orvath | 2 | 2 | 5 | 2 | 2 |
| 'Ghost' Sable | 1 | 2 | 3 | 5 | 1 |
| Sergeant Brutus Kael | 2 | 2 | 1 | 3 | 5 |
| Renn Duskfall | 3 | 1 | 3 | 1 | 3 |

---

## 3. Timers, rules & economy — ✅ plain text (JSON) or Unity

**File:** `unity/Agentz/Assets/StreamingAssets/config.json`
If present, its values override the **`GameConfig`** asset at Play (Model B). You only need to
include the fields you want to change — anything omitted keeps the asset's value.

### How to start & edit
1. **Create it from your current config:** **Agentz → Export Config to config.json** (writes
   the full asset as pretty JSON).
2. Edit the values in a text editor. Or keep a **minimal** file with just the fields you're
   tuning, e.g.:
   ```json
   { "roundDuration": 240, "failureLimit": 5, "skillCombineMode": 2 }
   ```
3. Optional: push a hand-edited file back into the Unity asset with **Agentz → Import
   config.json into GameConfig asset**.

> ⚠️ **Enum quirk:** `skillCombineMode` is written as a **number**, not a word —
> `0 = Average, 1 = Additive, 2 = Max`. (Unity's JSON writer can't emit the enum name.)

The field defaults below also live in
`unity/Agentz/Assets/Scripts/Config/GameConfig.cs`; editing that file changes the defaults
for any *newly created* asset (and needs a recompile). For a running game, edit `config.json`
or the asset.

| Setting | Field | Default | What it does |
|---------|-------|---------|--------------|
| **Round** | | | |
| Round duration (s) | `roundDuration` | 180 | Length of each round |
| Failure limit | `failureLimit` | 4 | Failures that end the round |
| **Spawning** | | | |
| Spawn interval min (s) | `missionSpawnIntervalMin` | 3 | Fastest gap between spawns |
| Spawn interval max (s) | `missionSpawnIntervalMax` | 6 | Slowest gap between spawns |
| Max active missions | `maxActiveMissions` | 4 | Cap on missions on the board |
| Mission expiry (s) | `missionExpiryDuration` | 20 | Time a mission waits before expiring |
| **Resolution** | | | |
| Busy duration (s) | `missionBusyDuration` | 7 | "In progress" time after assigning |
| Skill combine mode | `skillCombineMode` | Max | How team skills combine: Average / Additive / Max |
| **Economy** | | | |
| Base gold reward | `baseGoldReward` | 10 | ⚠️ defined but **not currently used** |
| Round completion bonus | `roundCompletionBonus` | 25 | ⚠️ defined but **not currently used** |
| Streak threshold | `streakBonusThreshold` | 5 | Consecutive successes before bonus kicks in |
| Streak bonus gold | `streakBonusGold` | 5 | Extra gold per success past the threshold |
| **Difficulty scaling** | | | |
| Skill scale / round | `skillScalePerRound` | 1.05 | Requirements ×= this each round (cumulative) |
| Spawn reduction / round | `spawnIntervalReductionPerRound` | 0.03 | Spawn gap shrinks by this fraction per round |
| Min spawn interval | `minSpawnInterval` | 1.5 | Floor for the shrinking spawn gap |
| **Upgrade shop (M3, unbuilt)** | | | |
| Skill upgrade base cost | `skillUpgradeCostBase` | 15 | ⚠️ future — no upgrade UI yet |
| Skill upgrade cost ramp | `skillUpgradeCostRamp` | 5 | ⚠️ future — no upgrade UI yet |

> Fields marked ⚠️ exist in the config but nothing reads them yet — changing them has no
> effect today. See GDD §4.5.

### Skill combine mode — the one that changes feel most
`skillCombineMode` decides how a multi-agent team's skills combine per axis:
- **Average** — mean of the assigned agents (a weak agent can *lower* the team score).
- **Additive** — sum, capped at 10 (more agents always helps).
- **Max** *(default)* — best value per axis (each agent contributes their strongest skills).

---

## 4. Syncing between text and Unity (the Agentz menu)

There is **no automatic two-way sync** — that's deliberate, so the two sides never silently
disagree. Instead, Unity's menu bar has an **Agentz** menu with four explicit, one-way
actions:

| Menu item | Direction | Use when… |
|-----------|-----------|-----------|
| **Export Agents to agents.csv** | assets → text | Seeding/refreshing the CSV from current assets |
| **Import agents.csv into Agent assets** | text → assets | You hand-edited the CSV and want the assets to match (matches by **Name**; creates an Undo step) |
| **Export Config to config.json** | assets → text | Seeding/refreshing the JSON from the current `GameConfig` |
| **Import config.json into GameConfig asset** | text → assets | You hand-edited the JSON and want the asset to match |

### The mental model
- **Day-to-day:** edit the **text files** — they win at Play, no Unity restart needed.
- **At Play time:** a text file (if present) always wins over its asset. That's the only
  "sync" that's automatic, and it's one-way (text → game), read fresh each Play.
- **When you edit in the Inspector** and want that reflected in the text file, click the
  matching **Export**. When you edit the text and want the asset updated, click **Import**.
- Deleting a text file cleanly reverts that data to the Inspector asset.

> Because Export overwrites the text file and Import overwrites the asset, pick a primary
> place to edit (recommended: the text files) and use the opposite action only to
> resynchronize. Don't edit both sides expecting them to merge.

---

## Quick reference — "I want to change X"
| I want to… | Edit | Needs Unity? |
|------------|------|--------------|
| Add / change a mission | `StreamingAssets/missions.csv` | No |
| …and have the CSV actually load | Ensure the scene's Mission Pool slot is **empty** | Yes (one-time) |
| Change an agent's skills/bio | `StreamingAssets/agents.csv` | No |
| Change round length, failures, spawn rate, difficulty | `StreamingAssets/config.json` | No |
| Change how team skills combine | `config.json` → `skillCombineMode` (0/1/2) | No |
| Create the agents.csv / config.json files the first time | **Agentz → Export …** | Yes (one-time) |
| Push a hand-edited text file back into the assets | **Agentz → Import …** | Yes |
| Change the built-in fallback data | `DefaultMissions.cs` / `DefaultAgents.cs` | Recompile |

## Precedence at a glance
For each kind of data, the first source that exists wins at Play:

- **Missions:** Inspector Mission Pool (if assigned) → `missions.csv` → `DefaultMissions.cs`
- **Agents:** `agents.csv` → Inspector roster → `DefaultAgents.cs`
- **Config:** `config.json` (per-field) → `GameConfig` asset → `GameConfig.cs` defaults

*(Missions are the one exception where an assigned Inspector value beats the CSV — so leave
the Mission Pool empty to drive missions from the CSV. Agents and config put the text file
first.)*
