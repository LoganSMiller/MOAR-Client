using LiteNetLib.Utils;

namespace MOAR.Networking
{
    /// <summary>
    /// Sync packet sent from server to clients to broadcast the active MOAR preset.
    /// Implements INetSerializable for FIKA Coop support.
    /// </summary>
    public struct PresetSyncPacket : INetSerializable
    {
        public string Version;
        public string PresetName;
        public string PresetLabel;

        // Static version constant for validation (optional)
        public static readonly string CurrentVersion = "1.0.0"; // Match this to your MOAR version

        public PresetSyncPacket(string presetName, string presetLabel)
        {
            Version = CurrentVersion;
            PresetName = presetName;
            PresetLabel = presetLabel;
        }

        public void Deserialize(NetDataReader reader)
        {
            Version = reader.GetString();
            PresetName = reader.GetString();
            PresetLabel = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Version);
            writer.Put(PresetName);
            writer.Put(PresetLabel);
        }
    }
}
