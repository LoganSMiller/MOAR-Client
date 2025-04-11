// ----------------------------------
// BotRegistry.cs (final unified version)
// ----------------------------------
using EFT;
using System.Collections.Generic;
using System.Diagnostics;
using UDebug = UnityEngine.Debug;

namespace MOAR.AI
{
    /// <summary>
    /// Centralized registry for tracking all AI bots in the scene.
    /// Supports side-based lookup and debug-safe logging.
    /// </summary>
    public static class BotRegistry
    {
        private static readonly HashSet<Player> _bots = new();

        /// <summary>
        /// All currently tracked AI bots.
        /// </summary>
        public static IEnumerable<Player> Bots => _bots;

        /// <summary>
        /// Total number of registered bots.
        /// </summary>
        public static int Count => _bots.Count;

        /// <summary>
        /// Registers an AI bot if not already tracked.
        /// </summary>
        public static void Register(Player bot)
        {
            if (bot == null || !bot.IsAI || _bots.Contains(bot))
                return;

            _bots.Add(bot);
            LogDebug($"Bot registered: {bot.Profile?.Nickname ?? bot.name}");
        }

        /// <summary>
        /// Unregisters a bot if present.
        /// </summary>
        public static void Unregister(Player bot)
        {
            if (bot != null && _bots.Remove(bot))
                LogDebug($"Bot unregistered: {bot.Profile?.Nickname ?? bot.name}");
        }

        /// <summary>
        /// Clears all tracked bots.
        /// </summary>
        public static void Clear() => _bots.Clear();

        /// <summary>
        /// Checks if a bot is already in the registry.
        /// </summary>
        public static bool IsRegistered(Player bot) => bot != null && _bots.Contains(bot);

        /// <summary>
        /// Returns bots matching a given side (e.g., "usec", "bear", "savage").
        /// Matches against Profile.Info.Side.ToString() for consistency with FIKA/SPT.
        /// </summary>
        public static IEnumerable<Player> GetBotsBySide(string side)
        {
            side = side.ToLowerInvariant();

            foreach (var bot in _bots)
            {
                var sideEnum = bot?.Profile?.Info?.Side;
                if (!sideEnum.HasValue)
                    continue;

                string normalized = sideEnum.Value.ToString().ToLowerInvariant();
                if (normalized == side)
                    yield return bot;
            }
        }

        [Conditional("DEBUG"), Conditional("UNITY_EDITOR")]
        private static void LogDebug(string msg)
        {
            UDebug.Log($"[RLO] {msg}");
        }
    }
}
