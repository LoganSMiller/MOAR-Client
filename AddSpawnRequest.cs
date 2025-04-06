using System;

namespace MOAR.Data
{
    /// <summary>
    /// Represents a spawn placement request sent to the MOAR server.
    /// Contains the map name and position to create the spawn point.
    /// </summary>
    [Serializable]
    public class AddSpawnRequest
    {
        public string Map { get; set; } = "Unknown";
        public Ixyz Position { get; set; } = new Ixyz();

        public override string ToString() => $"Map: {Map}, Position: {Position}";
    }
}
