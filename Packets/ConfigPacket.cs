using System;
using LiteNetLib.Utils;

namespace MOAR.Packets
{
    /// <summary>
    /// Synchronizes config settings between server and client (no version metadata).
    /// </summary>
    [Serializable]
    public struct ConfigPacket : INetSerializable
    {
        /// <summary>
        /// Serialized config values in string format.
        /// </summary>
        public string[] Settings;

        /// <summary>
        /// Constructor for creating a packet with settings.
        /// </summary>
        public ConfigPacket(string[] settings)
        {
            Settings = settings ?? Array.Empty<string>();
        }

        /// <summary>
        /// Deserialize from NetDataReader.
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            Settings = reader.GetStringArray();
        }

        /// <summary>
        /// Serialize to NetDataWriter.
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(Settings ?? Array.Empty<string>());
        }

        public override string ToString() =>
            $"[ConfigPacket] Settings Count: {Settings?.Length ?? 0}";
    }
}
