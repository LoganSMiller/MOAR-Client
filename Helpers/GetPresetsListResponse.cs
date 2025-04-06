using System;
using System.Collections.Generic;
using MOAR.Helpers;

namespace MOAR
{
    /// <summary>
    /// Represents the response from the /moar/getPresets endpoint.
    /// Contains available preset configurations with metadata and optional errors.
    /// </summary>
    [Serializable]
    public class GetPresetsListResponse
    {
        /// <summary>
        /// True if the request succeeded; false if it failed.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// List of available presets.
        /// </summary>
        public List<Preset> Data { get; set; } = new();

        /// <summary>
        /// The default preset name to fall back to.
        /// </summary>
        public string? DefaultPreset { get; set; }

        /// <summary>
        /// Informational or debugging message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Error message, if the response indicates failure.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Timestamp (Unix seconds) when the response was generated.
        /// </summary>
        public long ServerTimestamp { get; set; }

        /// <summary>
        /// Creates a success response with presets.
        /// </summary>
        public static GetPresetsListResponse CreateSuccess(List<Preset> presets, string? defaultPreset = null, string? message = null)
        {
            return new GetPresetsListResponse
            {
                Success = true,
                Data = presets ?? new List<Preset>(),
                DefaultPreset = defaultPreset,
                Message = message,
                ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        /// <summary>
        /// Creates a failure response with an error message.
        /// </summary>
        public static GetPresetsListResponse CreateFailure(string error)
        {
            return new GetPresetsListResponse
            {
                Success = false,
                Error = error,
                Data = new List<Preset>(),
                ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }
}
