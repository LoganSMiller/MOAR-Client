using MOAR.AI;
using System.Collections.Generic;

public static class BotPersonalityPresets
{
    public static readonly Dictionary<PersonalityType, BotPersonalityProfile> Presets = new()
    {
        [PersonalityType.Adaptive] = new BotPersonalityProfile
        {
            EngagementRange = 90f,
            Accuracy = 0.75f,
            RepositionPriority = 0.9f,
            RiskTolerance = 0.4f,
            Cohesion = 0.7f,
            AggressionLevel = 0.6f,
            FlankBias = 0.7f,
            RetreatThreshold = 0.4f,
            SuppressiveFireBias = 0.4f,
            CommunicationLevel = 0.8f,
            ChaosFactor = 0.2f,
            AccuracyUnderFire = 0.5f
        },

        [PersonalityType.Aggressive] = new BotPersonalityProfile
        {
            EngagementRange = 70f,
            Accuracy = 0.65f,
            RepositionPriority = 0.3f,
            RiskTolerance = 0.8f,
            Cohesion = 0.5f,
            AggressionLevel = 1.0f,
            FlankBias = 0.4f,
            RetreatThreshold = 0.2f,
            SuppressiveFireBias = 0.6f,
            CommunicationLevel = 0.3f,
            ChaosFactor = 0.3f,
            AccuracyUnderFire = 0.4f
        },

        [PersonalityType.Camper] = new BotPersonalityProfile
        {
            EngagementRange = 85f,
            Accuracy = 0.75f,
            RepositionPriority = 0.1f,
            RiskTolerance = 0.2f,
            Cohesion = 0.4f,
            AggressionLevel = 0.4f,
            FlankBias = 0.1f,
            RetreatThreshold = 0.2f,
            SuppressiveFireBias = 0.3f,
            CommunicationLevel = 0.2f,
            ChaosFactor = 0.1f,
            AccuracyUnderFire = 0.6f,
            IsCamper = true
        },

        [PersonalityType.Cautious] = new BotPersonalityProfile
        {
            EngagementRange = 65f,
            Accuracy = 0.8f,
            RepositionPriority = 0.85f,
            RiskTolerance = 0.2f,
            Cohesion = 0.8f,
            AggressionLevel = 0.4f,
            FlankBias = 0.3f,
            RetreatThreshold = 0.6f,
            SuppressiveFireBias = 0.1f,
            CommunicationLevel = 0.7f,
            ChaosFactor = 0.1f,
            AccuracyUnderFire = 0.6f
        },

        [PersonalityType.ColdBlooded] = new BotPersonalityProfile
        {
            EngagementRange = 75f,
            Accuracy = 0.85f,
            RepositionPriority = 0.5f,
            RiskTolerance = 0.3f,
            Cohesion = 0.6f,
            AggressionLevel = 0.3f,
            FlankBias = 0.2f,
            RetreatThreshold = 0.1f,
            SuppressiveFireBias = 0.1f,
            CommunicationLevel = 0.3f,
            ChaosFactor = 0.0f,
            AccuracyUnderFire = 0.7f
        },

        [PersonalityType.Defensive] = new BotPersonalityProfile
        {
            EngagementRange = 60f,
            Accuracy = 0.7f,
            RepositionPriority = 0.6f,
            RiskTolerance = 0.3f,
            Cohesion = 0.8f,
            AggressionLevel = 0.3f,
            FlankBias = 0.2f,
            RetreatThreshold = 0.5f,
            SuppressiveFireBias = 0.3f,
            CommunicationLevel = 0.6f,
            ChaosFactor = 0.1f,
            AccuracyUnderFire = 0.5f
        },

        [PersonalityType.Dumb] = new BotPersonalityProfile
        {
            EngagementRange = 120f,
            Accuracy = 0.4f,
            RepositionPriority = 0.1f,
            RiskTolerance = 1.0f,
            Cohesion = 0.3f,
            AggressionLevel = 0.7f,
            FlankBias = 0.0f,
            RetreatThreshold = 0.1f,
            SuppressiveFireBias = 0.1f,
            CommunicationLevel = 0.2f,
            ChaosFactor = 0.5f,
            AccuracyUnderFire = 0.3f,
            IsDumb = true
        },

        [PersonalityType.Explorer] = new BotPersonalityProfile
        {
            EngagementRange = 70f,
            Accuracy = 0.65f,
            RepositionPriority = 0.9f,
            RiskTolerance = 0.4f,
            Cohesion = 0.4f,
            AggressionLevel = 0.5f,
            FlankBias = 0.3f,
            RetreatThreshold = 0.4f,
            SuppressiveFireBias = 0.2f,
            CommunicationLevel = 0.3f,
            ChaosFactor = 0.4f,
            AccuracyUnderFire = 0.5f
        },

        [PersonalityType.Fearful] = new BotPersonalityProfile
        {
            EngagementRange = 50f,
            Accuracy = 0.6f,
            RepositionPriority = 0.9f,
            RiskTolerance = 0.1f,
            Cohesion = 0.7f,
            AggressionLevel = 0.2f,
            FlankBias = 0.1f,
            RetreatThreshold = 0.7f,
            SuppressiveFireBias = 0.1f,
            CommunicationLevel = 0.6f,
            ChaosFactor = 0.2f,
            AccuracyUnderFire = 0.3f,
            IsFearful = true
        },

        [PersonalityType.Frenzied] = new BotPersonalityProfile
        {
            EngagementRange = 100f,
            Accuracy = 0.55f,
            RepositionPriority = 0.2f,
            RiskTolerance = 1.0f,
            Cohesion = 0.5f,
            AggressionLevel = 1.0f,
            FlankBias = 0.2f,
            RetreatThreshold = 0.1f,
            SuppressiveFireBias = 0.6f,
            CommunicationLevel = 0.2f,
            ChaosFactor = 0.8f,
            AccuracyUnderFire = 0.3f,
            IsFrenzied = true
        },
        [PersonalityType.Loner] = new BotPersonalityProfile
        {
            EngagementRange = 75f,
            Accuracy = 0.7f,
            RepositionPriority = 0.6f,
            RiskTolerance = 0.6f,
            Cohesion = 0.2f,
            AggressionLevel = 0.5f,
            FlankBias = 0.4f,
            RetreatThreshold = 0.4f,
            SuppressiveFireBias = 0.3f,
            CommunicationLevel = 0.2f,
            ChaosFactor = 0.3f,
            AccuracyUnderFire = 0.4f
        },

        [PersonalityType.Patient] = new BotPersonalityProfile
        {
            EngagementRange = 90f,
            Accuracy = 0.85f,
            RepositionPriority = 0.7f,
            RiskTolerance = 0.2f,
            Cohesion = 0.5f,
            AggressionLevel = 0.3f,
            FlankBias = 0.2f,
            RetreatThreshold = 0.6f,
            SuppressiveFireBias = 0.1f,
            CommunicationLevel = 0.4f,
            ChaosFactor = 0.1f,
            AccuracyUnderFire = 0.5f
        },

        [PersonalityType.Reckless] = new BotPersonalityProfile
        {
            EngagementRange = 100f,
            Accuracy = 0.6f,
            RepositionPriority = 0.3f,
            RiskTolerance = 1.0f,
            Cohesion = 0.4f,
            AggressionLevel = 0.95f,
            FlankBias = 0.2f,
            RetreatThreshold = 0.1f,
            SuppressiveFireBias = 0.5f,
            CommunicationLevel = 0.3f,
            ChaosFactor = 0.7f,
            AccuracyUnderFire = 0.3f,
            IsStubborn = true
        },

        [PersonalityType.RiskTaker] = new BotPersonalityProfile
        {
            EngagementRange = 85f,
            Accuracy = 0.65f,
            RepositionPriority = 0.4f,
            RiskTolerance = 0.9f,
            Cohesion = 0.5f,
            AggressionLevel = 0.8f,
            FlankBias = 0.6f,
            RetreatThreshold = 0.2f,
            SuppressiveFireBias = 0.5f,
            CommunicationLevel = 0.4f,
            ChaosFactor = 0.5f,
            AccuracyUnderFire = 0.3f
        },

        [PersonalityType.SilentHunter] = new BotPersonalityProfile
        {
            EngagementRange = 100f,
            Accuracy = 0.9f,
            RepositionPriority = 0.9f,
            RiskTolerance = 0.2f,
            Cohesion = 0.6f,
            AggressionLevel = 0.4f,
            FlankBias = 0.4f,
            RetreatThreshold = 0.3f,
            SuppressiveFireBias = 0.0f,
            CommunicationLevel = 0.3f,
            ChaosFactor = 0.1f,
            AccuracyUnderFire = 0.6f,
            IsSilentHunter = true
        },

        [PersonalityType.Sniper] = new BotPersonalityProfile
        {
            EngagementRange = 120f,
            Accuracy = 0.95f,
            RepositionPriority = 0.5f,
            RiskTolerance = 0.3f,
            Cohesion = 0.5f,
            AggressionLevel = 0.4f,
            FlankBias = 0.1f,
            RetreatThreshold = 0.2f,
            SuppressiveFireBias = 0.0f,
            CommunicationLevel = 0.3f,
            ChaosFactor = 0.0f,
            AccuracyUnderFire = 0.7f
        },

        [PersonalityType.Strategic] = new BotPersonalityProfile
        {
            EngagementRange = 85f,
            Accuracy = 0.8f,
            RepositionPriority = 0.85f,
            RiskTolerance = 0.3f,
            Cohesion = 0.9f,
            AggressionLevel = 0.5f,
            FlankBias = 0.7f,
            RetreatThreshold = 0.4f,
            SuppressiveFireBias = 0.3f,
            CommunicationLevel = 0.9f,
            ChaosFactor = 0.2f,
            AccuracyUnderFire = 0.6f
        },

        [PersonalityType.Stubborn] = new BotPersonalityProfile
        {
            EngagementRange = 70f,
            Accuracy = 0.65f,
            RepositionPriority = 0.2f,
            RiskTolerance = 0.6f,
            Cohesion = 0.6f,
            AggressionLevel = 0.7f,
            FlankBias = 0.2f,
            RetreatThreshold = 0.05f,
            SuppressiveFireBias = 0.4f,
            CommunicationLevel = 0.3f,
            ChaosFactor = 0.3f,
            AccuracyUnderFire = 0.4f,
            IsStubborn = true
        },

        [PersonalityType.Tactical] = new BotPersonalityProfile
        {
            EngagementRange = 80f,
            Accuracy = 0.8f,
            RepositionPriority = 0.9f,
            RiskTolerance = 0.3f,
            Cohesion = 1.0f,
            AggressionLevel = 0.5f,
            FlankBias = 0.5f,
            RetreatThreshold = 0.4f,
            SuppressiveFireBias = 0.3f,
            CommunicationLevel = 0.95f,
            ChaosFactor = 0.1f,
            AccuracyUnderFire = 0.6f,
            IsTeamPlayer = true
        },

        [PersonalityType.TeamPlayer] = new BotPersonalityProfile
        {
            EngagementRange = 75f,
            Accuracy = 0.75f,
            RepositionPriority = 0.7f,
            RiskTolerance = 0.4f,
            Cohesion = 1.0f,
            AggressionLevel = 0.5f,
            FlankBias = 0.4f,
            RetreatThreshold = 0.4f,
            SuppressiveFireBias = 0.4f,
            CommunicationLevel = 1.0f,
            ChaosFactor = 0.2f,
            AccuracyUnderFire = 0.5f,
            IsTeamPlayer = true
        },

        [PersonalityType.Unpredictable] = new BotPersonalityProfile
        {
            EngagementRange = 90f,
            Accuracy = 0.65f,
            RepositionPriority = 0.4f,
            RiskTolerance = 0.8f,
            Cohesion = 0.5f,
            AggressionLevel = 0.6f,
            FlankBias = 0.5f,
            RetreatThreshold = 0.3f,
            SuppressiveFireBias = 0.5f,
            CommunicationLevel = 0.4f,
            ChaosFactor = 1.0f,
            AccuracyUnderFire = 0.4f
        },

        [PersonalityType.Vengeful] = new BotPersonalityProfile
        {
            EngagementRange = 80f,
            Accuracy = 0.7f,
            RepositionPriority = 0.6f,
            RiskTolerance = 0.6f,
            Cohesion = 0.6f,
            AggressionLevel = 0.8f,
            FlankBias = 0.3f,
            RetreatThreshold = 0.2f,
            SuppressiveFireBias = 0.5f,
            CommunicationLevel = 0.5f,
            ChaosFactor = 0.3f,
            AccuracyUnderFire = 0.5f
        }
    };
}