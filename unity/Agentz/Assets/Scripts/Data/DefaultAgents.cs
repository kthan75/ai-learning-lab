/// <summary>
/// Hard-coded starting roster of 6 agents with distinct skill distributions.
/// Used to populate AgentData ScriptableObjects on first setup, or as
/// a runtime fallback if no assets are found.
///
/// Skills order: Engineering, Diplomacy, Navigation, Street Smarts, Resilience
/// </summary>
public static class DefaultAgents
{
    public struct AgentDef
    {
        public string Name;
        public string Bio;
        public SkillSet Skills;

        public AgentDef(string name, string bio,
                        float eng, float dip, float nav, float ss, float res)
        {
            Name   = name;
            Bio    = bio;
            Skills = new SkillSet(eng, dip, nav, ss, res);
        }
    }

    public static readonly AgentDef[] All = new AgentDef[]
    {
        // 1. Engineering specialist — best at fixing things, average elsewhere
        new AgentDef(
            "Zara 'Sparks' Vex",
            "Former station mechanic turned field operative. Can rewire anything in under a minute.",
            eng: 9, dip: 2, nav: 4, ss: 3, res: 6),

        // 2. Diplomacy specialist — smooths over inter-species incidents
        new AgentDef(
            "Ambassador Rell'ik",
            "Multi-lingual attaché fluent in 14 dialects. Prefers words over fists.",
            eng: 2, dip: 9, nav: 3, ss: 5, res: 5),

        // 3. Navigation specialist — best pilot and spatial thinker on the roster
        new AgentDef(
            "Captain Mira Orvath",
            "Decorated shuttle pilot with an uncanny ability to find routes others miss.",
            eng: 4, dip: 3, nav: 9, ss: 4, res: 4),

        // 4. Street Smarts specialist — thrives in shady situations
        new AgentDef(
            "'Ghost' Sable",
            "Background unknown. Somehow knows everyone on every level of the station.",
            eng: 3, dip: 4, nav: 5, ss: 9, res: 3),

        // 5. Resilience specialist — tanks failures, keeps going
        new AgentDef(
            "Sergeant Brutus Kael",
            "Twenty-year security veteran. Hard to rattle, hard to stop.",
            eng: 3, dip: 5, nav: 3, ss: 6, res: 9),

        // 6. Hybrid (Engineering + Navigation) — technical pilot archetype
        new AgentDef(
            "Renn Duskfall",
            "Ex-military drone technician. Equally comfortable at a console or in the field.",
            eng: 7, dip: 2, nav: 7, ss: 2, res: 6),
    };
}
