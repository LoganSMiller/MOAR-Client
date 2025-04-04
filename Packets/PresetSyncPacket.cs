using LiteNetLib.Utils;

namespace MOAR.Networking
{
    /// <summary>
    /// Represents a FIKA network packet for syncing preset state across Coop, server, and headless clients.
    /// </summary>
    public class PresetSyncPacket : INetSerializable
    {
        /// <summary>
        /// Internal identifier of the preset (used in config and routing).
        /// </summary>
        public string PresetName { get; private set; } = "live-like";

        /// <summary>
        /// Friendly display label used for UI and logging.
        /// </summary>
        public string PresetLabel { get; private set; } = "Live Preset";

        /// <summary>
        /// Parameterless constructor for network deserialization.
        /// </summary>
        public PresetSyncPacket() { }

        /// <summary>
        /// Constructs a packet with specified preset name and optional label.
        /// </summary>
        /// <param name="name">Unique config-safe preset name.</param>
        /// <param name="label">Optional UI label. Falls back to <paramref name="name"/>.</param>
        public PresetSyncPacket(string name, string label)
        {
            PresetName = string.IsNullOrWhiteSpace(name) ? "live-like" : name.Trim();
            PresetLabel = string.IsNullOrWhiteSpace(label) ? PresetName : label.Trim();
        }

        /// <summary>
        /// Serializes the packet data to a binary network stream.
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PresetName ?? "live-like");
            writer.Put(PresetLabel ?? PresetName ?? "Live Preset");
        }

        /// <summary>
        /// Deserializes the packet data from a binary network stream.
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            PresetName = reader.GetString()?.Trim() ?? "live-like";
            PresetLabel = reader.GetString()?.Trim() ?? PresetName;
        }

        /// <summary>
        /// Returns a readable debug representation of the packet.
        /// </summary>
        public override string ToString() => $"{PresetLabel} ({PresetName})";
    }
}
