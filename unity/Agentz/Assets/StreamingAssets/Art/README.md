# M4 art assets

Drop the M4 images here. They'll be loaded at runtime (same approach as agent portraits) and
matched by **exact filename** — so use the names below precisely (all lowercase).

- **Format:** PNG. Use **transparent** PNG for icons, the logo, and stamps; backgrounds can be
  opaque PNG (or JPG).
- **Missing files are fine** — anything absent just isn't shown; nothing breaks.
- Sizes mirror [GDD Appendix B](../../docs/GDD.md). Backgrounds at 2560×1440 cover QHD and
  scale down cleanly.

## Essentials
| Filename | What it is | Size |
|----------|-----------|------|
| `dispatch_bg.png` | Main dispatch-screen background (dark/muted so UI stays readable) | 2560×1440 |
| `premise_bg.png` | Premise / intro screen art | 2560×1440 |
| `howto_1.png` | How-to-play panel 1 (missions appear on the board) | ~700×450 |
| `howto_2.png` | How-to-play panel 2 (pick agents) | ~700×450 |
| `howto_3.png` | How-to-play panel 3 (dice / resolution) | ~700×450 |
| `logo.png` | "Agentz" logo, transparent | ~1000×400 |
| `skill_eng.png` | Engineering skill icon, transparent | 128×128 |
| `skill_dip.png` | Diplomacy skill icon, transparent | 128×128 |
| `skill_nav.png` | Navigation skill icon, transparent | 128×128 |
| `skill_ssm.png` | Street Smarts skill icon, transparent | 128×128 |
| `skill_res.png` | Resilience skill icon, transparent | 128×128 |
| `gold.png` | Gold / credits icon, transparent | 96×96 |
| `stamp_success.png` | Success stamp (✓) for the result popup, transparent | 256×256 |
| `stamp_fail.png` | Failure stamp (✗) for the result popup, transparent | 256×256 |

## Optional (nice-to-have)
| Filename | What it is | Size |
|----------|-----------|------|
| `mission_engineering.png` | Mission-category icon: repairs/fire | 128×128 |
| `mission_diplomacy.png` | Mission-category icon: negotiation | 128×128 |
| `mission_navigation.png` | Mission-category icon: piloting/search | 128×128 |
| `mission_security.png` | Mission-category icon: security/street | 128×128 |
| `mission_medical.png` | Mission-category icon: medical/resilience | 128×128 |
| `mission_generic.png` | Mission-category icon: fallback | 128×128 |
| `incap_injured.png` | Incapacitation icon: injured | 64×64 |
| `incap_emergency.png` | Incapacitation icon: personal emergency | 64×64 |
| `incap_paperwork.png` | Incapacitation icon: paperwork | 64×64 |

When you've dropped in whatever's ready, tell me and we'll wire them up during M4 (I'll write
the loader + place each one).
