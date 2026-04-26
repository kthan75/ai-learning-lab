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
        // 1. Engineering specialist
        new AgentDef(
            "Zara 'Sparks' Vex",
            "Former station mechanic turned field operative. Can rewire anything in under a minute.",
            eng: 5, dip: 1, nav: 2, ss: 1, res: 3),

        // 2. Diplomacy specialist
        new AgentDef(
            "Ambassador Rell'ik",
            "Multi-lingual attaché fluent in 14 dialects. Prefers words over fists.",
            eng: 1, dip: 4, nav: 2, ss: 3, res: 2),

        // 3. Navigation specialist
        new AgentDef(
            "Captain Mira Orvath",
            "Decorated shuttle pilot with an uncanny ability to find routes others miss.",
            eng: 2, dip: 2, nav: 5, ss: 2, res: 2),

        // 4. Street Smarts specialist
        new AgentDef(
            "'Ghost' Sable",
            "Background unknown. Somehow knows everyone on every level of the station.",
            eng: 1, dip: 2, nav: 3, ss: 5, res: 1),

        // 5. Resilience specialist
        new AgentDef(
            "Sergeant Brutus Kael",
            "Twenty-year security veteran. Hard to rattle, hard to stop.",
            eng: 2, dip: 2, nav: 1, ss: 3, res: 5),

        // 6. Generalist — fills gaps, no standout weakness
        new AgentDef(
            "Renn Duskfall",
            "Ex-military drone technician. Equally comfortable at a console or in the field.",
            eng: 3, dip: 1, nav: 3, ss: 1, res: 3),
    };
}
