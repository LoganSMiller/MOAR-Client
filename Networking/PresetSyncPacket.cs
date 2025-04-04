using LiteNetLib.Utils;

namespace MOAR.Networking
{
    /// <summary>
    /// Represents a network packet used to synchronize the active preset between Coop clients and server.
    /// </summary>
    public class PresetSyncPacket : INetSerializable
    {
        /// <summary>
        /// Internal name of the preset to apply (used as key).
        /// </summary>
        public string PresetName { get; private set; }

        /// <summary>
        /// Display label of the preset to show in UI/logs.
        /// </summary>
        public string PresetLabel { get; private set; }

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public PresetSyncPacket() { }

        /// <summary>
        /// Constructs a new sync packet with the given preset name and label.
        /// </summary>
        public PresetSyncPacket(string name, string label)
        {
            PresetName = name ?? "live-like";
            PresetLabel = label ?? name ?? "Live Preset";
        }

        /// <summary>
        /// Serializes the preset sync packet for sending.
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PresetName ?? string.Empty);
            writer.Put(PresetLabel ?? string.Empty);
        }

        /// <summary>
        /// Deserializes the preset sync packet after receiving.
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            PresetName = reader.GetString();
            PresetLabel = reader.GetString();
        }

        public override string ToString() => $"{PresetLabel} ({PresetName})";
    }
}
