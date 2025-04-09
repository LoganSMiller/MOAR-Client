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

        public DebugNotificationPacket() { }

        public DebugNotificationPacket(string message, ENotificationIconType icon = ENotificationIconType.Default)
        {
            Message = message?.Trim() ?? string.Empty;
            Icon = icon;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Message ?? string.Empty);
            writer.Put((int)Icon);

#if DEBUG
            Plugin.LogSource?.LogDebug($"[{nameof(DebugNotificationPacket)}] Serialized: {this}");
#endif
        }

        public void Deserialize(NetDataReader reader)
        {
            Message = reader.GetString();

            int iconValue = reader.GetInt();
            if (!Enum.IsDefined(typeof(ENotificationIconType), iconValue))
            {
                Icon = ENotificationIconType.Default;
                Plugin.LogSource?.LogWarning($"[{nameof(DebugNotificationPacket)}] Unknown icon enum value: {iconValue}");
            }
            else
            {
                Icon = (ENotificationIconType)iconValue;
            }

            Plugin.LogSource?.LogDebug($"[{nameof(DebugNotificationPacket)}] Deserialized: {this}");
        }

        public bool IsValid() => !string.IsNullOrWhiteSpace(Message);

        public static DebugNotificationPacket Create(string message, ENotificationIconType icon = ENotificationIconType.Default) =>
            new DebugNotificationPacket(message, icon);

        public override string ToString() =>
            $"[{nameof(DebugNotificationPacket)}] \"{Message}\" ({Icon})";
    }
}
