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

        public AddSpawnRequest() { }

        public AddSpawnRequest(string map, Ixyz position)
        {
            this.map = map;
            this.position = position;
        }

        public override string ToString() => $"Map: {map}, Position: {position}";
    }
}
