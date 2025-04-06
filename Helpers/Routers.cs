using System.Collections.Generic;
using System.Linq;
using MOAR.Helpers;
using MOAR.Packets;

namespace MOAR
{
    /// <summary>
    /// Handles routing and interactions with config, presets, and in-game state.
    /// </summary>
    public static class Routers
    {
        private static ConfigSettings _serverSettings = new();
        private static string _activePresetName = "live-like";
        private static string _activePresetLabel = "Live-Like";

        /// <summary>
        /// Initializes router state based on current settings.
        /// </summary>
        public static void Init(BepInEx.Configuration.ConfigFile config)
        {
            _activePresetName = Settings.currentPreset?.Value ?? "live-like";
            _activePresetLabel = Settings.GetCurrentPresetLabel();
        }

        /// <summary>
        /// Returns the current active preset's label.
        /// </summary>
        public static string GetCurrentPresetLabel() => _activePresetLabel;

        /// <summary>
        /// Returns the label used for announcements.
        /// </summary>
        public static string GetAnnouncePresetLabel() => _activePresetLabel;

        /// <summary>
        /// Sets the active preset name and label.
        /// </summary>
        public static void SetPreset(string name)
        {
            var preset = Settings.PresetList.FirstOrDefault(p => p.Name == name);
            if (preset != null)
            {
                _activePresetName = preset.Name;
                _activePresetLabel = preset.Label;
            }
        }

        public static void SetHostPresetLabel(string label)
        {
            _activePresetLabel = string.IsNullOrWhiteSpace(label) ? "Live-Like" : label.Trim();
        }

        /// <summary>
        /// Returns a copy of the current default config.
        /// </summary>
        public static ConfigSettings GetDefaultConfig() => new();

        /// <summary>
        /// Returns current server config with overrides.
        /// </summary>
        public static ConfigSettings GetServerConfigWithOverrides() => _serverSettings;

        /// <summary>
        /// Returns the list of available presets.
        /// </summary>
        public static List<Preset> GetPresetsList() => Settings.PresetList;

        public static string AddBotSpawn() => "[MOAR] Bot spawn added.";
        public static string AddSniperSpawn() => "[MOAR] Sniper spawn added.";
        public static string AddPlayerSpawn() => "[MOAR] Player spawn added.";
        public static string DeleteBotSpawn() => "[MOAR] Bot spawn deleted.";
    }
}
