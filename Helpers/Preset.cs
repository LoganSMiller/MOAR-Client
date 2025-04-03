using System;

namespace MOAR.Helpers
{
    /// <summary>
    /// Represents a named spawn configuration preset with an optional object-based settings payload.
    /// </summary>
    public class Preset
    {
        /// <summary>
        /// The internal name identifier for the preset (used as key).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The user-facing display label for the preset.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The arbitrary settings object applied when the preset is selected.
        /// </summary>
        public object Settings { get; set; } = new();

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public Preset() { }

        /// <summary>
        /// Creates a new preset with the specified name, label, and settings.
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
        /// Returns the label or fallback name for debug or UI display.
        /// </summary>
        public override string ToString() => !string.IsNullOrWhiteSpace(Label) ? Label : Name;
    }
}
