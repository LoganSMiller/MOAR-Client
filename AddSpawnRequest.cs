using MOAR.Data;
using System;

namespace MOAR
{
    /// <summary>
    /// Represents a spawn request sent from the client to the server,
    /// including the target map and 3D world position.
    /// </summary>
    public class AddSpawnRequest
    {
        /// <summary>
        /// The internal map name (e.g., "factory4_day", "bigmap").
        /// </summary>
        public string Map { get; set; }

        /// <summary>
        /// The spawn position (x, y, z) for the bot/player/sniper.
        /// </summary>
        public Ixyz Position { get; set; }

        /// <summary>
        /// Default constructor for deserialization.
        /// </summary>
        public AddSpawnRequest() { }

        /// <summary>
        /// Constructs a new AddSpawnRequest with map and position.
        /// </summary>
        public AddSpawnRequest(string map, Ixyz position)
        {
            Map = map;
            Position = position;
        }

        /// <summary>
        /// Returns a readable string representation of the request.
        /// </summary>
        public override string ToString() => $"Map: {Map}, Position: {Position}";
    }
}
