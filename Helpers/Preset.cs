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
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The display label shown to users in the UI. Falls back to Name if not specified.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The optional configuration payload for this preset.
        /// This can be any serializable structure (e.g. Dictionary, POCO, JSON).
        /// </summary>
        public object Settings { get; set; } = new();

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public Preset()
        {
            Name = "Unnamed";
            Label = "Unnamed";
            Settings = new object();
        }

        /// <summary>
        /// Creates a new preset with a name, label, and custom settings payload.
        /// </summary>
        public Preset(string name, string label, object settings)
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
        public override string ToString() => Label ?? Name ?? "Unnamed Preset";
    }
}
