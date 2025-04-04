using System;
using System.Collections.Generic;

namespace MOAR.Helpers
{
    /// <summary>
    /// Represents the server response containing all available preset definitions.
    /// Used by the client to populate dropdowns, apply configurations, or validate current state.
    /// </summary>
    [Serializable]
    public class GetPresetsListResponse
    {
        /// <summary>
        /// Collection of all presets returned by the server.
        /// Each includes a unique name, label, and optional settings payload.
        /// </summary>
        public List<Preset> Data { get; set; } = new();

        /// <summary>
        /// Indicates whether the request was successful.
        /// Used for fallback handling and status checks.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Optional error or status message returned by the server.
        /// Empty or null if successful.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
