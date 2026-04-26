/// <summary>
/// Built-in mission templates used when no MissionTemplate assets are assigned.
/// Skills: Engineering, Diplomacy, Navigation, Street Smarts, Resilience
/// </summary>
public static class DefaultMissions
{
    public struct MissionDef
    {
        public string   Title, Description;
        public SkillSet Skills;
        public int      GoldReward, Tier;

        public MissionDef(string title, string desc,
                          float eng, float dip, float nav, float ss, float res,
                          int gold = 10, int tier = 1)
        {
            Title       = title;
            Description = desc;
            Skills      = new SkillSet(eng, dip, nav, ss, res);
            GoldReward  = gold;
            Tier        = tier;
        }
    }

    public static readonly MissionDef[] All = new MissionDef[]
    {
        // ── Tier 1 — easy, balanced ─────────────────────────────────────────
        new MissionDef(
            "Reactor Coolant Leak",
            "Pressure is dropping in bay 7. Patch it before the core overheats. [ENG, RES]",
            eng:7, dip:1, nav:2, ss:2, res:4, gold:10, tier:1),

        new MissionDef(
            "Docking Bay Dispute",
            "Two freighter crews are blocking the airlock and refusing to budge. [DIP, SS]",
            eng:1, dip:7, nav:2, ss:4, res:2, gold:10, tier:1),

        new MissionDef(
            "Lost Supply Drone",
            "A cargo drone went dark somewhere in the maintenance corridors. [NAV, ENG]",
            eng:3, dip:1, nav:7, ss:3, res:2, gold:10, tier:1),

        new MissionDef(
            "Black Market Tip",
            "Intel suggests a smuggling drop in the lower decks. Investigate quietly. [SS, NAV]",
            eng:1, dip:3, nav:3, ss:8, res:1, gold:12, tier:1),

        new MissionDef(
            "Brawl in the Cantina",
            "Three Kreth workers and a Vellian merchant need separating — fast. [RES, DIP]",
            eng:1, dip:4, nav:1, ss:3, res:8, gold:10, tier:1),

        // ── Tier 2 — moderate, dual-skill ───────────────────────────────────
        new MissionDef(
            "Malfunctioning Airlock",
            "The outer seal won't cycle. Manual override needed during a docking. [ENG, NAV]",
            eng:6, dip:1, nav:5, ss:2, res:3, gold:15, tier:2),

        new MissionDef(
            "Ambassador Escort",
            "The Yrev delegation needs an escort through the station's lower ring. [DIP, NAV]",
            eng:1, dip:6, nav:4, ss:3, res:3, gold:15, tier:2),

        new MissionDef(
            "Smuggler Interrogation",
            "Detained suspect won't talk. Someone fluent in leverage is needed. [SS, DIP]",
            eng:1, dip:5, nav:1, ss:7, res:3, gold:18, tier:2),

        new MissionDef(
            "Engine Room Fire",
            "Electrical fault triggered a suppression failure. Get in and fix it. [ENG, RES]",
            eng:7, dip:1, nav:2, ss:2, res:6, gold:15, tier:2),

        new MissionDef(
            "Navigation Array Offline",
            "Three incoming ships can't dock — the beacon is down. [NAV, ENG]",
            eng:5, dip:2, nav:7, ss:1, res:2, gold:15, tier:2),

        // ── Tier 3 — hard, high demands ─────────────────────────────────────
        new MissionDef(
            "Hostage Situation",
            "A disgruntled mechanic has locked down deck 9 with six civilians inside. [RES, DIP]",
            eng:3, dip:6, nav:2, ss:5, res:7, gold:25, tier:3),

        new MissionDef(
            "Reactor Meltdown Warning",
            "The failsafe triggered a full lockdown. Override it before evacuation. [ENG, RES]",
            eng:9, dip:1, nav:3, ss:2, res:6, gold:25, tier:3),

        new MissionDef(
            "Intruder in the Core",
            "Unknown agent has bypassed security and is heading for the control hub. [SS, NAV]",
            eng:4, dip:2, nav:5, ss:8, res:5, gold:25, tier:3),

        new MissionDef(
            "Trade Summit Breakdown",
            "Three species reps are about to walk. Talks collapse in 10 minutes. [DIP, SS]",
            eng:1, dip:9, nav:1, ss:5, res:3, gold:22, tier:3),

        new MissionDef(
            "Hull Breach — Outer Ring",
            "Micro-meteorite punctured an outer panel. EVA repair required. [ENG, RES]",
            eng:7, dip:1, nav:6, ss:2, res:7, gold:25, tier:3),
    };
}
