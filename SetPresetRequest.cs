using System;

namespace MOAR
{
    /// <summary>
    /// Request payload sent from client to server to change the active spawn preset.
    /// </summary>
    public class SetPresetRequest
    {
        /// <summary>
        /// The internal name or label of the preset to activate.
        /// </summary>
        public string Preset { get; set; }

        /// <summary>
        /// Default constructor for deserialization.
        /// </summary>
        public SetPresetRequest() { }

        /// <summary>
        /// Constructs a new SetPresetRequest with a specified preset name.
        /// </summary>
        /// <param name="preset">The internal name or label of the preset.</param>
        public SetPresetRequest(string preset)
        {
            Preset = preset ?? throw new ArgumentNullException(nameof(preset));
        }

        public override string ToString() => $"Preset Request: {Preset}";
    }
}
