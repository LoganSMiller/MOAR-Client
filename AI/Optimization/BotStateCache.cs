using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace MOAR.AI.Optimization
{
    public class BotOwnerStateCache
    {
        private readonly Dictionary<string, BotOwnerState> _cache = new();

        public void CacheBotOwnerState(BotOwner botOwner)
        {
            string id = botOwner?.Profile?.Id ?? "";
            if (!_cache.ContainsKey(id))
            {
                _cache[id] = new BotOwnerState(botOwner);
            }
        }

        public void UpdateBotOwnerStateIfNeeded(BotOwner botOwner)
        {
            string id = botOwner?.Profile?.Id ?? "";
            var newState = new BotOwnerState(botOwner);

            if (_cache.TryGetValue(id, out var lastState) && !lastState.Equals(newState))
            {
                _cache[id] = newState;
                UpdateBotOwnerAI(botOwner);
            }
        }

        private void UpdateBotOwnerAI(BotOwner botOwner)
        {
            if (botOwner == null || botOwner.gameObject == null)
                return;

            var moar = botOwner.GetComponent<MOARBotOwner>();
            if (moar == null)
                return;

            var profile = moar.PersonalityProfile;
            float aggression = profile.AggressionLevel;
            float caution = 1f - aggression;

            // Personality-driven behavior tuning
            if (aggression > 0.7f)
            {
                botOwner.Memory?.SetCombatAggressionMode(); // Custom extension/hook method
                ReassignZoneBehavior(botOwner, preferForward: true);
            }
            else if (caution > 0.7f)
            {
                botOwner.Memory?.SetCautiousSearchMode(); // Custom extension/hook method
                ReassignZoneBehavior(botOwner, preferForward: false);
            }
            else
            {
                ReassignZoneBehavior(botOwner, preferForward: null); // Use defaults
            }
        }

        private void ReassignZoneBehavior(BotOwner botOwner, bool? preferForward)
        {
            // Add your tactical logic here. Example:
            Vector3 fallback = botOwner.Position + Vector3.back * 5f;
            Vector3 advance = botOwner.Position + Vector3.forward * 8f;

            if (preferForward == true)
            {
                botOwner.Memory?.ForceMoveTo(advance);
            }
            else if (preferForward == false)
            {
                botOwner.Memory?.FallbackTo(fallback);
            }
            else
            {
                botOwner.Memory?.ReevaluateCurrentCover();
            }
        }
    }
}
