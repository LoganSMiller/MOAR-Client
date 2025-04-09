using System;

namespace MOAR.Data
{
    /// <summary>
    /// Represents a spawn placement request sent to the MOAR server.
    /// Used for injecting new spawn points via live in-game commands.
    /// </summary>
    [Serializable]
    public class AddSpawnRequest
    {
        /// <summary>
        /// Map identifier (e.g., "Factory", "Woods", "Customs").
        /// Used to associate the spawn with the correct level.
        /// </summary>
        public string Map { get; set; } = "Unknown";

        /// <summary>
        /// World-space coordinates for the new spawn point.
        /// </summary>
        public Ixyz Position { get; set; } = new Ixyz();

        /// <summary>
        /// Debug-friendly output.
        /// </summary>
        public override string ToString() => $"Map: {Map}, Position: {Position}";

        /// <summary>
        /// Normalizes input values (trims, null checks).
        /// </summary>
        public void Normalize()
        {
            Map = string.IsNullOrWhiteSpace(Map) ? "Unknown" : Map.Trim();
            Position ??= new Ixyz();
        }

        /// <summary>
        /// Validates the request format.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Map)
                   && Position is { X: not 0, Y: not 0, Z: not 0 };
        }
    }
}
