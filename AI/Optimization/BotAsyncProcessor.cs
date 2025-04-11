using System.Threading.Tasks;
using UnityEngine;
using EFT;

namespace MOAR.AI.Optimization
{
    /// <summary>
    /// Performs async-safe, low-frequency updates to BotOwner AI values.
    /// Intended for staggered initialization or deferred logic outside the main frame.
    /// </summary>
    public class BotOwnerAsyncProcessor
    {
        public async Task ProcessBotOwnerAsync(BotOwner botOwner)
        {
            if (botOwner == null || botOwner.Settings?.FileSettings?.Mind == null)
                return;

            await Task.Delay(30); // Yield to avoid main thread strain

            ApplyPersonalityModifiers(botOwner);
        }

        private void ApplyPersonalityModifiers(BotOwner botOwner)
        {
            var mind = botOwner.Settings.FileSettings.Mind;
            var personality = botOwner.GetComponent<MOARBotOwner>()?.PersonalityProfile;

            if (personality == null)
                return;

            // Risk-based panic tuning
            mind.PANIC_RUN_WEIGHT = Mathf.Lerp(0.5f, 2f, personality.RiskTolerance);
            mind.PANIC_SIT_WEIGHT = Mathf.Lerp(10f, 80f, 1 - personality.RiskTolerance);

            // Coordination tuning
            mind.DIST_TO_FOUND_SQRT = Mathf.Lerp(200f, 600f, 1f - personality.Cohesion);

            // Aggression
            mind.FRIEND_AGR_KILL = Mathf.Lerp(0f, 0.4f, personality.AggressionLevel);
        }
    }
}
