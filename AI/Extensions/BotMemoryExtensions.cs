using UnityEngine;
using EFT;

namespace MOAR.AI.Extensions
{
    public static class BotMemoryExtensions
    {
        /// <summary>
        /// Simulates a fallback movement by commanding the bot to move to a new position.
        /// Must be called with a valid BotOwner.
        /// </summary>
        public static void FallbackTo(this BotOwner bot, Vector3 fallbackPosition)
        {
            if (bot == null || fallbackPosition == Vector3.zero)
                return;

            bot.GoToPoint(fallbackPosition, true); // uses real EFT method to request nav movement

            Debug.Log($"[MOAR] Bot {bot.Profile?.Info?.Nickname} performing fallback to {fallbackPosition}");
        }

        /// <summary>
        /// Forces the bot to move immediately to a position (ignores combat state).
        /// </summary>
        public static void ForceMoveTo(this BotOwner bot, Vector3 position)
        {
            if (bot == null)
                return;

            bot.GoToPoint(position, true); // optionally toggle auto-sprint/urgency
            Debug.Log($"[MOAR] Bot {bot.Profile?.Info?.Nickname} forced move to {position}");
        }

        /// <summary>
        /// Signals a soft reset of threat evaluation. Does not modify any internal state directly.
        /// </summary>
        public static void ReevaluateCurrentCover(this BotOwner bot)
        {
            // Placeholder for triggering cover re-check logic (e.g., assign a fallback or remove memory)
            Debug.Log($"[MOAR] Bot {bot.Profile?.Info?.Nickname} reevaluating cover behavior");
        }

        /// <summary>
        /// Flags the bot to use a cautious movement/search profile.
        /// </summary>
        public static void SetCautiousSearchMode(this BotOwner bot)
        {
            Debug.Log($"[MOAR] Bot {bot.Profile?.Info?.Nickname} using cautious search mode");
            // Optional: modify group or brain behavior flags here
        }

        /// <summary>
        /// Flags the bot to operate in aggressive engagement mode.
        /// </summary>
        public static void SetCombatAggressionMode(this BotOwner bot)
        {
            Debug.Log($"[MOAR] Bot {bot.Profile?.Info?.Nickname} using combat aggression mode");
            // Optional: modify group or brain behavior flags here
        }
    }
}
