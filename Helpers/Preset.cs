using System;

namespace MOAR.Helpers
{
    /// <summary>
    /// Represents a named configuration preset used to control spawn behavior and AI settings.
    /// Presets contain a unique name, a display label, and optional serialized settings payload.
    /// </summary>
    public class Preset
    {
        /// <summary>
        /// Internal identifier used for referencing the preset (e.g. in configs or network sync).
        /// </summary>
        public string Name { get; set; } = "Unnamed";

        /// <summary>
        /// User-facing display label shown in UI and logs. Falls back to <see cref="Name"/> if null or empty.
        /// </summary>
        public string Label { get; set; } = "Unnamed";

        /// <summary>
        /// Optional configuration payload for the preset.
        /// Can be any serializable object (e.g. POCO, Dictionary, JSON).
        /// </summary>
        public object Settings { get; set; } = new();

        /// <summary>
        /// Default constructor for serialization and fallback.
        /// </summary>
        public Preset() { }

        /// <summary>
        /// Creates a new preset with the given name, label, and optional payload.
        /// </summary>
        /// <param name="name">Unique internal identifier (required).</param>
        /// <param name="label">User-friendly label. Falls back to <paramref name="name"/> if null or empty.</param>
        /// <param name="settings">Custom payload object (can be null).</param>
        public Preset(string name, string? label, object? settings)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Preset name cannot be null or empty.");

            Name = name.Trim();
            Label = string.IsNullOrWhiteSpace(label) ? Name : label.Trim();
            Settings = settings ?? new object();
        }

        /// <summary>
        /// Returns the display label if set, otherwise falls back to the internal name.
        /// </summary>
        public override string ToString() => string.IsNullOrWhiteSpace(Label) ? Name : Label;
    }
}
