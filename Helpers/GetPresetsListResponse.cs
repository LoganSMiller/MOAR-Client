using System;
using System.Collections.Generic;

namespace MOAR.Helpers
{
    /// <summary>
    /// Represents the deserialized response from the server containing
    /// the list of available AI spawn presets.
    /// This is consumed by the client UI to populate the dropdown and apply selected presets.
    /// </summary>
    [Serializable]
    public class GetPresetsListResponse
    {
        /// <summary>
        /// The collection of all available presets returned by the server.
        /// Each item contains both a label and internal name.
        /// </summary>
        public List<Preset> Data { get; set; } = new();

        /// <summary>
        /// Indicates whether the server responded successfully (optional).
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Optional server message or error string (null if success).
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
