using System;
using EFT.Communications;
using Fika.Core.Networking;
using LiteNetLib.Utils;

namespace MOAR.Packets
{
    /// <summary>
    /// Syncs a selected MOAR preset from server to all FIKA clients.
    /// Clients apply it automatically on receipt.
    /// </summary>
    [Serializable]
    public sealed class PresetSyncPacket : INetSerializable
    {
        /// <summary>
        /// The internal config name of the preset.
        /// </summary>
        public string PresetName { get; set; } = string.Empty;

        /// <summary>
        /// The display label for the preset.
        /// </summary>
        public string PresetLabel { get; set; } = string.Empty;

        /// <summary>
        /// Version string for validation.
        /// </summary>
        public static readonly string CurrentVersion = Plugin.Version;
        public string Version { get; set; } = CurrentVersion;

        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public PresetSyncPacket() { }

        /// <summary>
        /// Constructs a new sync packet with the given name and label.
        /// </summary>
        public PresetSyncPacket(string name, string label)
        {
            PresetName = name?.Trim() ?? string.Empty;
            PresetLabel = label?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Writes packet data to the network stream.
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PresetName ?? string.Empty);
            writer.Put(PresetLabel ?? string.Empty);
            writer.Put(Version ?? string.Empty);
        }

        /// <summary>
        /// Reads packet data from the network stream.
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            PresetName = reader.GetString();
            PresetLabel = reader.GetString();
            Version = reader.GetString();

            Plugin.LogSource?.LogDebug($"[PresetSyncPacket] Deserialized: {this}");
        }

        /// <summary>
        /// Validates whether the packet contains usable data.
        /// </summary>
        public bool IsValid() =>
            !string.IsNullOrWhiteSpace(PresetName) &&
            !string.IsNullOrWhiteSpace(PresetLabel);

        public override string ToString() =>
            $"[PresetSyncPacket] \"{PresetLabel}\" ({PresetName}) v{Version}";
    }
}
