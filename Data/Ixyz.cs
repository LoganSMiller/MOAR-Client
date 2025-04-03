using System;

namespace MOAR.Data
{
    /// <summary>
    /// Represents a 3D vector position (x, y, z) used for spawn-related data exchanges in MOAR.
    /// </summary>
    [Serializable]
    public class Ixyz
    {
        /// <summary>
        /// The X-axis coordinate.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// The Y-axis coordinate.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// The Z-axis coordinate.
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public Ixyz() { }

        /// <summary>
        /// Constructs a new Ixyz with the given coordinates.
        /// </summary>
        public Ixyz(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Returns a string representation of the position, formatted as (x, y, z).
        /// </summary>
        public override string ToString() => $"({X:F1}, {Y:F1}, {Z:F1})";

        /// <summary>
        /// Converts this Ixyz to a Unity-style Vector3 (if applicable).
        /// </summary>
        public UnityEngine.Vector3 ToVector3() => new(X, Y, Z);

        /// <summary>
        /// Sets this Ixyz from a Unity-style Vector3 (if applicable).
        /// </summary>
        public void FromVector3(UnityEngine.Vector3 vec)
        {
            X = vec.x;
            Y = vec.y;
            Z = vec.z;
        }
    }
}
