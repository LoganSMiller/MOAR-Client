using System;

namespace MOAR.Helpers
{
    /// <summary>
    /// Represents a named configuration preset used to control spawn behavior and AI settings.
    /// Presets contain a unique name, a display label, and serialized spawn/config settings.
    /// </summary>
    public class Preset
    {
        /// <summary>
        /// Internal identifier used for referencing the preset (e.g., in configs or sync).
        /// </summary>
        public string Name { get; set; } = "Unnamed";

        /// <summary>
        /// Display label for UI/logs. Falls back to <see cref="Name"/> if not set.
        /// </summary>
        public string Label { get; set; } = "Unnamed";

        /// <summary>
        /// Spawn configuration values associated with the preset.
        /// </summary>
        public ConfigSettings Settings { get; set; } = new ConfigSettings();

        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public Preset() { }

        /// <summary>
        /// Creates a new preset instance.
        /// </summary>
        public Preset(string name, string? label, ConfigSettings? settings = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Preset name cannot be null or empty.");

            Name = name.Trim();
            Label = string.IsNullOrWhiteSpace(label) ? Name : label.Trim();
            Settings = settings ?? new ConfigSettings();
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Label) ? Name : Label;
    }
}
