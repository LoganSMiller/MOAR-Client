using System;
using Newtonsoft.Json;

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
        /// The internal name or label of the preset to activate.
        /// Must match a preset defined in the server configuration.
        /// </summary>
        [JsonProperty("preset")]
        public string Preset { get; set; } = string.Empty;

        /// <summary>
        /// Parameterless constructor for JSON deserialization.
        /// </summary>
        [JsonConstructor]
        public SetPresetRequest()
        {
            Preset = string.Empty;
        }

        /// <summary>
        /// Constructs a new request with the specified preset name or label.
        /// </summary>
        /// <param name="preset">The name or label of the preset to activate.</param>
        public SetPresetRequest(string? preset)
        {
            Preset = string.IsNullOrWhiteSpace(preset) ? string.Empty : preset.Trim();
        }

        /// <summary>
        /// Normalizes the input for safe comparison or storage.
        /// </summary>
        public void Normalize()
        {
            Preset = Preset?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Returns a debug-friendly string representation.
        /// </summary>
        public override string ToString()
        {
            return $"Preset = \"{Preset ?? "null"}\"";
        }
    }
}
