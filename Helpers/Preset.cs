using System;

namespace MOAR.Helpers
{
    /// <summary>
    /// Represents a named spawn or config preset used in MOAR.
    /// Presets are synced from server to client and control bot spawn behavior and settings.
    /// </summary>
    [Serializable]
    public class Preset
    {
        /// <summary>
        /// Unique internal name of the preset (used for logic and server sync).
        /// Example: "live-like", "hardcore", "sandbox".
        /// </summary>
        public string Name { get; set; } = "live-like";

        /// <summary>
        /// Human-readable label for UI display, announcements, or notifications.
        /// Example: "Live-Like", "Hardcore Mode", "Free Roam".
        /// </summary>
        public string Label { get; set; } = "Live-Like";

        /// <summary>
        /// Full configuration payload defining behavior for this preset.
        /// Includes spawn values, boss tuning, bot settings, etc.
        /// </summary>
        public ConfigSettings Settings { get; set; } = new ConfigSettings();

        /// <summary>
        /// Returns a debug string for logging purposes.
        /// </summary>
        public override string ToString()
        {
            return $"[Preset] \"{Label}\" ({Name}) — Settings: {(Settings != null ? "Loaded" : "None")}";
        }

        /// <summary>
        /// Checks whether this preset is valid and has a usable configuration.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Label) &&
                   Settings != null;
        }
    }
}
