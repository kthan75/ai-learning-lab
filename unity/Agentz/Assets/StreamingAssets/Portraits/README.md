# Agent portraits

Drop agent portrait images in this folder. They're loaded at runtime by
`PortraitLoader` and matched to each agent by a **slug of the agent's name**
(lowercase; spaces → `_`; quotes/apostrophes dropped).

Supported formats: **.png**, .jpg, .jpeg. Square images are fine (aspect/crop TBD).

## Filenames for the current roster

| Agent | File to drop here |
|-------|-------------------|
| Sergeant Brutus Kael | `sergeant_brutus_kael.png` |
| Zara "Sparks" Vex | `zara_sparks_vex.png` |
| 'Ghost' Sable | `ghost_sable.png` |
| Ambassador Rell'ik | `ambassador_rellik.png` |
| Captain Mira Orvath | `captain_mira_orvath.png` |
| Renn Duskfall | `renn_duskfall.png` |

## Notes
- If a file is missing, that agent just renders without a portrait — nothing breaks.
- A portrait assigned directly on an `AgentData` asset in the Unity Inspector takes
  precedence; this folder only fills agents that don't already have one.
- For agents added via `agents.csv`, the same name-slug rule applies.
