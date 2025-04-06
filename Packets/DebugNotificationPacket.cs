using System;
using EFT.Communications;
using Fika.Core.Networking;
using LiteNetLib.Utils;

namespace MOAR.Packets
{
    /// <summary>
    /// Syncs debug notification messages across FIKA clients.
    /// </summary>
    [Serializable]
    public sealed class DebugNotificationPacket : INetSerializable
    {
        /// <summary>
        /// Message to be displayed in the notification.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Notification icon type (quest, alert, etc.).
        /// </summary>
        public ENotificationIconType Icon { get; set; } = ENotificationIconType.Default;

        /// <summary>
        /// Default constructor for deserialization.
        /// </summary>
        public DebugNotificationPacket() { }

        /// <summary>
        /// Constructs a packet with message and optional icon.
        /// </summary>
        public DebugNotificationPacket(string message, ENotificationIconType icon = ENotificationIconType.Default)
        {
            Message = message?.Trim() ?? string.Empty;
            Icon = icon;
        }

        /// <summary>
        /// Serialize this packet to network.
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Message ?? string.Empty);
            writer.Put((int)Icon);
        }

        /// <summary>
        /// Deserialize this packet from network.
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            Message = reader.GetString();
            Icon = (ENotificationIconType)reader.GetInt();
            Plugin.LogSource?.LogDebug($"[DebugNotificationPacket] Deserialized: {this}");
        }

        /// <summary>
        /// Whether this packet contains a valid, non-empty message.
        /// </summary>
        public bool IsValid() => !string.IsNullOrWhiteSpace(Message);

        /// <summary>
        /// Factory method for simple creation.
        /// </summary>
        public static DebugNotificationPacket Create(string message, ENotificationIconType icon = ENotificationIconType.Default) =>
            new DebugNotificationPacket(message, icon);

        public override string ToString() =>
            $"[DebugNotificationPacket] \"{Message}\" ({Icon})";
    }
}
