using System;
using MOAR.Helpers;

namespace MOAR.Helpers
{
    [Serializable]
    public class Preset
    {
        // Unique identifier for internal logic and server-client sync
        public string Name { get; set; } = "live-like";

        // User-facing label shown in UI or notifications
        public string Label { get; set; } = "Live-Like";

        // Full payload of config settings used in this preset
        public ConfigSettings Settings { get; set; } = new ConfigSettings();
    }
}
