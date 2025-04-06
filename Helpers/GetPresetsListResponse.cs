using System.Collections.Generic;
using MOAR.Helpers; 

namespace MOAR
{
    /// <summary>
    /// Represents the response returned from the /moar/getPresets endpoint.
    /// Contains the list of available spawn configuration presets.
    /// </summary>
    public class GetPresetsListResponse
    {
        public List<Preset>? Data { get; set; }
    }
}
