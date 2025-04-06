using System;

namespace MOAR.Data
{
    [Serializable]
    public class Ixyz
    {
        public float X;
        public float Y;
        public float Z;

        public override string ToString() => $"({X:0.00}, {Y:0.00}, {Z:0.00})";
    }
}
