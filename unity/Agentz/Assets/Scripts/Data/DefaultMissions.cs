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
            "Two freighter crews are blocking the airlock and refusing to budge. [DIP, SSM]",
            eng:1, dip:7, nav:2, ss:4, res:2, gold:10, tier:1),

        new MissionDef(
            "Lost Supply Drone",
            "A cargo drone went dark somewhere in the maintenance corridors. [NAV, ENG]",
            eng:3, dip:1, nav:7, ss:3, res:2, gold:10, tier:1),

        new MissionDef(
            "Black Market Tip",
            "Intel suggests a smuggling drop in the lower decks. Investigate quietly. [SSM,NAV]",
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
            "Detained suspect won't talk. Someone fluent in leverage is needed. [SSM,DIP]",
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
            "Unknown agent has bypassed security and is heading for the control hub. [SSM,NAV]",
            eng:4, dip:2, nav:5, ss:8, res:5, gold:25, tier:3),

        new MissionDef(
            "Trade Summit Breakdown",
            "Three species reps are about to walk. Talks collapse in 10 minutes. [DIP, SSM]",
            eng:1, dip:9, nav:1, ss:5, res:3, gold:22, tier:3),

        new MissionDef(
            "Hull Breach — Outer Ring",
            "Micro-meteorite punctured an outer panel. EVA repair required. [ENG, RES]",
            eng:7, dip:1, nav:6, ss:2, res:7, gold:25, tier:3),

        // ── Tier 1 — additional ─────────────────────────────────────────────
        new MissionDef(
            "Oxygen Recycler Fault",
            "Bay 3 recycler is throwing errors. Trace the fault before CO2 levels spike. [ENG, NAV]",
            eng:6, dip:1, nav:3, ss:1, res:2, gold:10, tier:1),

        new MissionDef(
            "Stowaway Detected",
            "Someone is hiding in the cargo holds. Find them before they cause trouble. [SSM,RES]",
            eng:1, dip:2, nav:2, ss:7, res:4, gold:10, tier:1),

        new MissionDef(
            "Fuel Line Rupture",
            "A pressurised line burst near docking arm 2. Contain it before it ignites. [ENG, RES]",
            eng:7, dip:1, nav:1, ss:1, res:3, gold:12, tier:1),

        new MissionDef(
            "Medical Emergency",
            "A dockworker collapsed in the lower ring. Someone needs to stabilise them until the medic arrives. [RES, DIP]",
            eng:2, dip:3, nav:1, ss:2, res:7, gold:10, tier:1),

        new MissionDef(
            "Missing Passenger",
            "A transit passenger hasn't boarded and their ship leaves in 20 minutes. [NAV, DIP]",
            eng:1, dip:4, nav:6, ss:2, res:1, gold:10, tier:1),

        // ── Tier 2 — additional ─────────────────────────────────────────────
        new MissionDef(
            "Weapons Cache Discovery",
            "A routine inspection found a hidden arms stash. Someone needs to extract it quietly. [SSM,ENG]",
            eng:3, dip:2, nav:2, ss:7, res:4, gold:18, tier:2),

        new MissionDef(
            "Quarantine Breach",
            "An unregistered ship docked without clearance. Lock it down before anything spreads. [RES, NAV]",
            eng:2, dip:3, nav:5, ss:2, res:6, gold:15, tier:2),

        new MissionDef(
            "Power Grid Overload",
            "Surge in sector 4 is cascading. Kill the feed manually before it hits life support. [ENG, SSM]",
            eng:7, dip:1, nav:3, ss:4, res:2, gold:15, tier:2),

        new MissionDef(
            "Diplomatic Insult",
            "A senior Vellian delegate was publicly insulted by station staff. Smooth it over fast. [DIP, RES]",
            eng:1, dip:7, nav:2, ss:3, res:5, gold:15, tier:2),

        new MissionDef(
            "Stolen Shuttle",
            "A docked shuttle went dark and is now moving. Track it down and bring it back. [NAV, SSM]",
            eng:3, dip:1, nav:7, ss:5, res:2, gold:18, tier:2),

        // ── Tier 3 — additional ─────────────────────────────────────────────
        new MissionDef(
            "Station Lockdown",
            "Someone triggered a manual security lockdown from an unknown terminal. Override and locate the source. [SSM,ENG]",
            eng:6, dip:2, nav:4, ss:7, res:6, gold:25, tier:3),

        new MissionDef(
            "Assassination Plot",
            "Credible threat against a visiting dignitary. Extract them through the station without incident. [SSM,RES]",
            eng:2, dip:5, nav:4, ss:8, res:7, gold:28, tier:3),

        new MissionDef(
            "Sabotaged Life Support",
            "Primary life support has been deliberately tampered with. Find the fault and fix it — now. [ENG, RES]",
            eng:8, dip:1, nav:4, ss:3, res:7, gold:25, tier:3),

        new MissionDef(
            "Galactic Council Hearing",
            "The station is under review. A council observer is watching everything. Keep the peace and the image. [DIP, RES]",
            eng:1, dip:9, nav:3, ss:4, res:6, gold:22, tier:3),

        new MissionDef(
            "Rescue — Sector 7",
            "A distress beacon from an unmapped corridor. Reach it before life signs go dark. [NAV, RES]",
            eng:5, dip:2, nav:8, ss:3, res:7, gold:25, tier:3),
    };
}
