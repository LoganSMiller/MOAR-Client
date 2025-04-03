using LiteNetLib.Utils;

namespace MOAR.Packets
{
    /// <summary>
    /// Packet used to synchronize the selected preset from server to client via FIKA networking.
    /// </summary>
    public class PresetSyncPacket : INetSerializable
    {
        /// <summary>
        /// Internal identifier of the preset (e.g., "live-like").
        /// </summary>
        public string PresetName { get; set; } = string.Empty;

        /// <summary>
        /// User-facing label of the preset (e.g., "Live-Like").
        /// </summary>
        public string PresetLabel { get; set; } = string.Empty;

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public PresetSyncPacket() { }

        /// <summary>
        /// Constructs a new sync packet with preset name and label.
        /// </summary>
        /// <param name="presetName">Internal name of the preset.</param>
        /// <param name="presetLabel">Human-readable display name.</param>
        public PresetSyncPacket(string presetName, string presetLabel)
        {
            PresetName = presetName ?? string.Empty;
            PresetLabel = presetLabel ?? string.Empty;
        }

        /// <inheritdoc />
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PresetName);
            writer.Put(PresetLabel);
        }

        /// <inheritdoc />
        public void Deserialize(NetDataReader reader)
        {
            PresetName = reader.GetString();
            PresetLabel = reader.GetString();
        }
    }
}
