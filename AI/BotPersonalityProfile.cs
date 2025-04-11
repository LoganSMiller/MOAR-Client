namespace MOAR.AI
{
    public enum PersonalityType
    {
        Adaptive,
        Aggressive,
        Camper,
        Cautious,
        ColdBlooded,
        Defensive,
        Dumb,
        Explorer,
        Fearful,
        Frenzied,
        Loner,
        Patient,
        Reckless,
        RiskTaker,
        SilentHunter,
        Sniper,
        Strategic,
        Stubborn,
        Tactical,
        TeamPlayer,
        Unpredictable,
        Vengeful
    }

    /// <summary>
    /// Holds bot personality traits that influence their combat and engagement behavior.
    /// This system is client-only and purely behavioral — no gear or loadout control.
    /// </summary>
    public class BotPersonalityProfile
    {
        public PersonalityType Personality { get; set; }

        // Core tactical behavior
        public float EngagementRange { get; set; } = 80f;        // Max distance the bot will prefer to engage from
        public float Accuracy { get; set; } = 0.7f;              // Base combat accuracy (1.0 = perfect)
        public float RepositionPriority { get; set; } = 0.8f;    // Tendency to reposition into better range
        public float RiskTolerance { get; set; } = 0.5f;         // Willingness to engage outside optimal range
        public float Cohesion { get; set; } = 0.75f;             // Tendency to stick with squadmates
        public float AggressionLevel { get; set; } = 0.6f;       // Likelihood to push, chase, or take initiative

        // Advanced behavior flags
        public float FlankBias { get; set; } = 0.5f;             // Preference for flanking vs head-on combat
        public float RetreatThreshold { get; set; } = 0.3f;      // HP percent at which to consider retreat
        public float SuppressiveFireBias { get; set; } = 0.2f;   // Likelihood to use suppressive fire over aimed shots
        public float CommunicationLevel { get; set; } = 0.6f;    // Tendency to broadcast team information or sync
        public float ChaosFactor { get; set; } = 0.0f;           // Randomization factor in combat choices
        public float AccuracyUnderFire { get; set; } = 0.4f;     // Drop in accuracy when under suppression or stress

        // Simplified toggles for key roles
        public bool IsDumb { get; set; } = false;
        public bool IsFearful { get; set; } = false;
        public bool IsCamper { get; set; } = false;
        public bool IsFrenzied { get; set; } = false;
        public bool IsSilentHunter { get; set; } = false;
        public bool IsTeamPlayer { get; set; } = false;
        public bool IsSadistic { get; set; } = false;
        public bool IsStubborn { get; set; } = false;
    }
}
