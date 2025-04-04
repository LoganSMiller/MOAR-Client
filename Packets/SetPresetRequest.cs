using System;

namespace MOAR.Packets
{
    /// <summary>
    /// Represents a request sent from the client to the server to change the active spawn preset.
    /// Supports both internal names and human-readable labels.
    /// </summary>
    [Serializable]
    public sealed class SetPresetRequest
    {
        /// <summary>
        /// Gets or sets the internal name or kebab-case label of the preset to activate.
        /// Must match a preset defined in the server configuration.
        /// </summary>
        public string Preset { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetPresetRequest"/> class.
        /// Required for JSON deserialization.
        /// </summary>
        public SetPresetRequest() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetPresetRequest"/> class with a specified preset name or label.
        /// </summary>
        /// <param name="preset">The name or label of the preset to apply.</param>
        public SetPresetRequest(string preset)
        {
            Preset = string.IsNullOrWhiteSpace(preset) ? string.Empty : preset.Trim();
        }

        /// <summary>
        /// Returns a readable representation of the preset request for debugging.
        /// </summary>
        public override string ToString() => $"Preset = {Preset}";
    }
}
