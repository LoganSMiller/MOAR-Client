using System;

namespace MOAR.Data
{
    /// <summary>
    /// Represents a spawn placement request sent to the MOAR server.
    /// Contains the target map and the world position for creating a new spawn point.
    /// </summary>
    [Serializable]
    public class AddSpawnRequest
    {
        /// <summary>
        /// Map identifier (e.g., "Factory", "Woods", "Customs").
        /// Used to route the spawn placement on the correct level.
        /// </summary>
        public string Map { get; set; } = "Unknown";

        /// <summary>
        /// The 3D position where the bot or player spawn should be created.
        /// </summary>
        public Ixyz Position { get; set; } = new Ixyz();

        /// <summary>
        /// Returns a string representation for logging or debugging.
        /// </summary>
        public override string ToString() => $"Map: {Map}, Position: {Position}";

        /// <summary>
        /// Normalizes the request by trimming and validating fields.
        /// </summary>
        public void Normalize()
        {
            Map = string.IsNullOrWhiteSpace(Map) ? "Unknown" : Map.Trim();
            Position ??= new Ixyz();
        }

        /// <summary>
        /// Returns true if this request has a known map and a non-zero position.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Map) &&
                   Position is { X: not 0, Y: not 0, Z: not 0 };
        }
    }
}
