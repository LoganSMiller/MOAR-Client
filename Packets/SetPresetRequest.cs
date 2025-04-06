using System;
using Newtonsoft.Json;

namespace MOAR.Packets
{
    /// <summary>
    /// Represents a request from a FIKA client to update the active spawn preset.
    /// Accepts either the internal name or user-facing label of the preset.
    /// </summary>
    [Serializable]
    public sealed class SetPresetRequest
    {
        /// <summary>
        /// The name or label of the preset to apply.
        /// This must match a server-side preset.
        /// </summary>
        [JsonProperty("preset")]
        public string Preset { get; set; } = string.Empty;

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        [JsonConstructor]
        public SetPresetRequest() { }

        /// <summary>
        /// Creates a new request using the specified preset name or label.
        /// </summary>
        public SetPresetRequest(string? preset)
        {
            Preset = string.IsNullOrWhiteSpace(preset) ? string.Empty : preset.Trim();
        }

        /// <summary>
        /// Trims and sanitizes the preset value.
        /// </summary>
        public void Normalize()
        {
            Preset = Preset?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Returns whether this request contains a valid, non-empty preset.
        /// </summary>
        public bool IsValid() => !string.IsNullOrWhiteSpace(Preset);

        /// <summary>
        /// String representation used in debug logs or console output.
        /// </summary>
        public override string ToString() => $"SetPresetRequest: \"{Preset}\"";

        /// <summary>
        /// Equality helper for comparing requests (optional).
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is SetPresetRequest other && string.Equals(Preset, other.Preset, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => Preset?.ToLowerInvariant().GetHashCode() ?? 0;
    }
}
