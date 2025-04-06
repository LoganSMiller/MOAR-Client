using System.Collections.Generic;
using BepInEx.Configuration;
using MOAR.Helpers;

namespace MOAR
{
    /// <summary>
    /// Handles routing and interactions with config and in-game state.
    /// </summary>
    public static class Routers
    {
        private static ConfigSettings _serverSettings = new();

        /// <summary>
        /// Initializes router state based on current settings.
        /// </summary>
        public static void Init(ConfigFile config)
        {
            // Nothing required here yet.
        }

        /// <summary>
        /// Returns the current preset label (same as name in simplified logic).
        /// </summary>
        public static string GetCurrentPresetLabel() => Settings.GetCurrentPresetLabel();

        /// <summary>
        /// Returns the label used for announcements.
        /// </summary>
        public static string GetAnnouncePresetLabel() => Settings.GetCurrentPresetLabel();

        /// <summary>
        /// Sets the current preset (used by host or UI changes).
        /// </summary>
        public static void SetPreset(string name)
        {
            Settings.currentPreset.Value = name;
        }

        /// <summary>
        /// Updates the displayed preset label (used only during host startup).
        /// </summary>
        public static void SetHostPresetLabel(string label)
        {
            // No-op: handled automatically in Settings.cs
        }

        /// <summary>
        /// Returns a copy of the current default config.
        /// </summary>
        public static ConfigSettings GetDefaultConfig() => new();

        /// <summary>
        /// Returns current server config (host authoritative).
        /// </summary>
        public static ConfigSettings GetServerConfigWithOverrides() => _serverSettings;

        public static string AddBotSpawn() => "[MOAR] Bot spawn added.";
        public static string AddSniperSpawn() => "[MOAR] Sniper spawn added.";
        public static string AddPlayerSpawn() => "[MOAR] Player spawn added.";
        public static string DeleteBotSpawn() => "[MOAR] Bot spawn deleted.";
    }
}