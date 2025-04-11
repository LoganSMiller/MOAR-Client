using EFT;
using UnityEngine;
using MOAR.AI;
using MOAR.Helpers;


namespace MOAR.AI
{
    /// <summary>
    /// Utility to safely attach or retrieve a BotPanicController from an AI bot.
    /// Automatically applies role-specific panic behavior.
    /// </summary>
    public static class BotPanicUtility
    {
        /// <summary>
        /// Ensures the bot has a panic controller attached and configured.
        /// </summary>
        /// <param name="bot">The AI Player instance</param>
        /// <param name="panic">The resulting panic controller (if valid)</param>
        /// <returns>True if panic controller is present and ready</returns>
        public static bool TryGet(Player bot, out BotPanicController panic)
        {
            panic = bot.GetComponent<BotPanicController>();

            if (panic == null)
            {
                panic = bot.gameObject.AddComponent<BotPanicController>();
                ApplyProfileConfig(bot, panic);
            }

            if (!BotCacheUtility.TryGet(bot, out _))
                return false;

            return panic != null && panic.enabled;
        }

        /// <summary>
        /// Applies profile-specific behavior tuning to the BotPanicController.
        /// </summary>
        private static void ApplyProfileConfig(Player bot, BotPanicController panic)
        {
            var profile = BotVisionProfiles.Get(bot);

            // 1. Role-based duration scaling (less adaptable = longer panic)
            panic.panicDuration = Mathf.Lerp(2f, 5f, 1f - Mathf.Clamp01(profile.AdaptationSpeed / 2f));

            // 2. Vision reduction based on light sensitivity
            panic.visionMultiplier = Mathf.Clamp01(1f - (profile.LightSensitivity * 0.3f));

            // 3. Aggression-based scatter probability
            panic.scatterChance = Mathf.Clamp01(profile.AggressionResponse * 0.25f);

            // 4. Movement slowdown during panic (aggression = less frozen)
            panic.panicMovementSpeed = Mathf.Clamp(1f - profile.AggressionResponse * 0.35f, 0.1f, 0.8f);

            // 5. Optional: Hook for bark/audio/stimulus (stubbed for expansion)
            panic.panicVoiceLine = GetDefaultBark(bot);

#if UNITY_EDITOR || DEBUG
            if (RLOConfig.DebugEnabled.Value)
            {
                Debug.Log($"[RLO] Panic config → {bot.Profile?.Info?.Settings?.Role}: " +
                          $"Dur={panic.panicDuration:F1}s | Vision={panic.visionMultiplier:F2} | " +
                          $"Scatter={panic.scatterChance:P0} | Speed={panic.panicMovementSpeed:F2}");
            }
#endif
        }

        /// <summary>
        /// Placeholder for triggering bot audio bark, voice, or animation cue.
        /// </summary>
        private static string GetDefaultBark(Player bot)
        {
            return bot.Profile?.Info?.Settings?.Role switch
            {
                WildSpawnType.bossBully => "shout_take_cover",
                WildSpawnType.assault => "panic_moan",
                WildSpawnType.pmcBot => "cuss_lightflash",
                WildSpawnType.bossKilla => "rage_charge",
                WildSpawnType.gifter => "laugh_glitch",
                _ => "panic_generic"
            };
        }
    }
}
