using System;
using System.Collections.Generic;

namespace MOAR.Helpers
{
    [Serializable]
    public class Preset
    {
        public string Name { get; set; } = "live-like";
        public string Label { get; set; } = "Live-Like";

        public ConfigSettings Settings { get; set; } = new ConfigSettings();
    }
}
