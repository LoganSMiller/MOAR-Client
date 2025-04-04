using System.Collections.Generic;

namespace MOAR.Helpers
{
    /// <summary>
    /// Represents a server response payload containing all available AI spawn presets.
    /// </summary>
    public class GetPresetsListResponse
    {
        /// <summary>
        /// The list of server-defined spawn configuration presets.
        /// </summary>
        public List<Preset> Data { get; set; } = new();
    }
}
