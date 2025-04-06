using System;
using EFT.Communications;
using Fika.Core.Networking;
using LiteNetLib.Utils;

namespace MOAR.Packets
{
    /// <summary>
    /// Used to sync debug notification messages across clients in a FIKA session.
    /// </summary>
    [Serializable]
    public sealed class DebugNotificationPacket : INetSerializable
    {
        /// <summary>
        /// The text to display in the notification.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The icon type shown with the notification.
        /// </summary>
        public ENotificationIconType Icon { get; set; } = ENotificationIconType.Default;

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public DebugNotificationPacket() { }

        /// <summary>
        /// Constructs a new debug packet with a message and optional icon.
        /// </summary>
        public DebugNotificationPacket(string message, ENotificationIconType icon)
        {
            Message = message?.Trim() ?? string.Empty;
            Icon = icon;
        }

        /// <summary>
        /// Serializes the packet into a network writer.
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Message ?? string.Empty);
            writer.Put((int)Icon);
        }

        /// <summary>
        /// Deserializes the packet from a network reader.
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            Message = reader.GetString();
            Icon = (ENotificationIconType)reader.GetInt();

            Plugin.LogSource?.LogDebug($"[DebugNotificationPacket] Deserialized: {this}");
        }

        /// <summary>
        /// Checks whether this packet contains valid data.
        /// </summary>
        public bool IsValid() => !string.IsNullOrWhiteSpace(Message);

        /// <summary>
        /// Factory method for simplified packet creation.
        /// </summary>
        public static DebugNotificationPacket Create(string message, ENotificationIconType icon = ENotificationIconType.Default) =>
            new DebugNotificationPacket(message, icon);

        public override string ToString() =>
            $"[DebugNotificationPacket] \"{Message}\" ({Icon})";
    }
}
