namespace MOAR
{
    /// <summary>
    /// Represents a basic 3D position used in spawn-related data exchanges.
    /// </summary>
    public class Ixyz
    {
        /// <summary>
        /// X-coordinate in world space.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y-coordinate in world space.
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Z-coordinate in world space.
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Default constructor for serialization.
        /// </summary>
        public Ixyz() { }

        /// <summary>
        /// Constructs an Ixyz position from X, Y, Z components.
        /// </summary>
        public Ixyz(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Returns a string representation of the position.
        /// </summary>
        public override string ToString() => $"({X:F1}, {Y:F1}, {Z:F1})";
    }
}
