using System;

namespace MOAR.Helpers
{
    /// <summary>
    /// Represents a named spawn configuration preset with an optional object-based settings payload.
    /// </summary>
    public class Preset
    {
        /// <summary>
        /// The internal identifier for the preset (used for storage, config selection).
        /// </summary>
        public string Name { get; set; } = "Unnamed";

        /// <summary>
        /// The display label shown to users in the UI. Falls back to Name if not specified.
        /// </summary>
        public string Label { get; set; } = "Unnamed";

        /// <summary>
        /// The optional configuration payload for this preset.
        /// This can be any serializable structure (e.g. Dictionary, POCO, JSON).
        /// </summary>
        public object Settings { get; set; } = new();

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public Preset() { }

        /// <summary>
        /// Creates a new preset with a name, label, and custom settings payload.
        /// </summary>
        /// <param name="name">The internal name identifier.</param>
        /// <param name="label">The display label. Falls back to <paramref name="name"/> if null or empty.</param>
        /// <param name="settings">The settings object associated with this preset.</param>
        public Preset(string name, string? label, object? settings)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Preset name cannot be null or empty.");

            Name = name;
            Label = string.IsNullOrWhiteSpace(label) ? name : label;
            Settings = settings ?? new object();
        }

        /// <summary>
        /// Returns the label or fallback name for debug, logging, or UI.
        /// </summary>
        public override string ToString() => !string.IsNullOrWhiteSpace(Label) ? Label : Name;
    }
}
