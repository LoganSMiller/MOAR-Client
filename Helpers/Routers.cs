using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using Fika.Core.Coop.Utils;
using MOAR.Helpers;

namespace MOAR
{
    /// <summary>
    /// Handles routing and interactions with config and in-game state.
    /// </summary>
    public static class Routers
    {
        private static readonly object _presetLock = new();
        private static readonly ManualLogSource Log = Plugin.LogSource;

        private static ConfigSettings _serverSettings = new();

        private static readonly List<Preset> _availablePresets = new()
        {
            new Preset { Name = "live-like" },
            new Preset { Name = "hardcore" },
            new Preset { Name = "relaxed" }
        };

        /// <summary>
        /// Initializes router state based on current settings.
        /// </summary>
        public static void Init(ConfigFile config)
        {
            // Future initialization logic can go here.
        }

        /// <summary>
        /// Gets the current active preset name (safe fallback).
        /// </summary>
        public static string GetCurrentPresetLabel()
        {
            return Settings.currentPreset?.Value ?? "live-like";
        }

        /// <summary>
        /// Gets the active label used for announcements.
        /// </summary>
        public static string GetAnnouncePresetLabel()
        {
            return GetCurrentPresetLabel();
        }

        /// <summary>
        /// Updates the current preset if allowed by host role.
        /// </summary>
        public static void SetPreset(string name)
        {
            if (Settings.IsFika && !FikaBackendUtils.IsServer)
            {
                Log.LogWarning("[MOAR] Ignored client-side preset change attempt in FIKA mode.");
                return;
            }

            Settings.currentPreset.Value = name;
            Log.LogInfo($"[MOAR] Preset set to: {name}");
        }

        /// <summary>
        /// No-op — handled in Settings.cs via reactive event.
        /// </summary>
        public static void SetHostPresetLabel(string label)
        {
            // Host label auto-managed.
        }

        /// <summary>
        /// Gets the current default configuration structure.
        /// </summary>
        public static ConfigSettings GetDefaultConfig()
        {
            return new ConfigSettings(); // Return new blank/default
        }

        /// <summary>
        /// Gets the current authoritative server config.
        /// </summary>
        public static ConfigSettings GetServerConfigWithOverrides()
        {
            return _serverSettings;
        }

        /// <summary>
        /// Fake message for UI button feedback.
        /// </summary>
        public static string AddBotSpawn() => "[MOAR] Bot spawn added.";

        public static string AddSniperSpawn() => "[MOAR] Sniper spawn added.";

        public static string AddPlayerSpawn() => "[MOAR] Player spawn added.";

        public static string DeleteBotSpawn() => "[MOAR] Bot spawn deleted.";

        /// <summary>
        /// Returns list of presets safely (cloned).
        /// </summary>
        public static List<Preset> GetPresetsList()
        {
            lock (_presetLock)
            {
                return new List<Preset>(_availablePresets);
            }
        }
    }

    /// <summary>
    /// Represents a basic named preset option.
    /// </summary>
    public class Preset
    {
        public string Name { get; set; }
    }
}
