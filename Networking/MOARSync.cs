using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Communications;
using Fika.Core.Coop.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LiteNetLib;
using MOAR.Helpers;
using MOAR.Packets;
using SPT.Common.Utils;

namespace MOAR.Networking
{
    internal static class MOARSync
    {
        private static readonly Dictionary<ConfigEntryBase, SyncedValues> SettingOverrides = new();
        private static bool ConfigReceived = false;
        private static bool IgnoreClientChanges = false;

        public static void RegisterFikaEventListeners()
        {
            Plugin.LogSource.LogInfo("[MOARSync] Registering FIKA sync lifecycle handlers...");
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetworkManagerCreated);
            FikaEventDispatcher.SubscribeEvent<PeerConnectedEvent>(OnPeerConnected);
            FikaEventDispatcher.SubscribeEvent<FikaRaidStartedEvent>(OnRaidStarted);
            FikaEventDispatcher.SubscribeEvent<FikaGameEndedEvent>(OnGameEnded);
        }

        private static void OnFikaNetworkManagerCreated(FikaNetworkManagerCreatedEvent ev)
        {
            if (ev.Manager is FikaServer)
            {
                Plugin.Instance.Config.SettingChanged += OnServerSettingChanged;
            }
            else if (ev.Manager is FikaClient client)
            {
                Plugin.Instance.Config.SettingChanged += OnClientSettingChanged;
                client.RegisterPacket<PresetSyncPacket>(HandlePresetPacketClient);
            }
        }

        private static void OnPeerConnected(PeerConnectedEvent ev)
        {
            if (!FikaBackendUtils.IsServer)
                return;

            var current = Settings.currentPreset?.Value ?? "live-like";
            var label = Routers.GetAnnouncePresetLabel();
            var packet = new PresetSyncPacket(current, label);

            Plugin.LogSource.LogInfo($"[MOARSync] Peer connected — syncing preset to peer {ev.Peer.Id}");
            Singleton<FikaServer>.Instance.SendDataToPeer(ev.Peer, ref packet, DeliveryMethod.ReliableUnordered);

        }

        private static void OnServerSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (!FikaBackendUtils.IsServer || args.ChangedSetting != Settings.currentPreset)
                return;

            var packet = new PresetSyncPacket(
                Settings.currentPreset?.Value ?? "live-like",
                Routers.GetAnnouncePresetLabel()
            );

            Plugin.LogSource.LogInfo("[MOARSync] Preset changed — broadcasting to all clients");
            Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableUnordered);
        }

        private static void OnClientSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (IgnoreClientChanges || args.ChangedSetting != Settings.currentPreset)
                return;

            IgnoreClientChanges = true;

            if (SettingOverrides.TryGetValue(Settings.currentPreset, out var values))
            {
                Settings.currentPreset.SetSerializedValue(values.Override);
                Plugin.LogSource.LogInfo($"[MOARSync] Restored synced preset override: {values.Override}");
            }

            IgnoreClientChanges = false;
        }

        private static void HandlePresetPacketClient(PresetSyncPacket packet)
        {
            Plugin.LogSource.LogInfo($"[MOARSync] Received preset from host: {packet.PresetLabel} ({packet.PresetName})");
            ConfigReceived = true;

            IgnoreClientChanges = true;

            var preset = Settings.PresetList?.FirstOrDefault(p =>
                string.Equals(p.Name, packet.PresetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Label, packet.PresetLabel, StringComparison.OrdinalIgnoreCase));

            if (preset != null)
            {
                if (!SettingOverrides.ContainsKey(Settings.currentPreset))
                {
                    SettingOverrides[Settings.currentPreset] = new SyncedValues
                    {
                        Original = Settings.currentPreset.GetSerializedValue(),
                        Override = preset.Name
                    };
                }
                else
                {
                    SettingOverrides[Settings.currentPreset] = new SyncedValues
                    {
                        Original = SettingOverrides[Settings.currentPreset].Original,
                        Override = preset.Name
                    };
                }

                Settings.currentPreset.SetSerializedValue(preset.Name);
                Routers.SetHostPresetLabel(preset.Label);

                Plugin.LogSource.LogInfo($"[MOARSync] Applied preset: {preset.Label}");
            }
            else
            {
                Plugin.LogSource.LogWarning($"[MOARSync] Preset not found locally: {packet.PresetName}");
            }

            IgnoreClientChanges = false;
        }

        private static void OnRaidStarted(FikaRaidStartedEvent ev)
        {
            if (!ev.IsServer && !ConfigReceived)
            {
                Plugin.LogSource.LogError("[MOARSync] Preset sync failed — Host is missing MOAR?");
                NotificationManagerClass.DisplayWarningNotification(
                    "MOAR preset sync failed! Host is missing MOAR?",
                    ENotificationDurationType.Long
                );
            }
        }

        private static void OnGameEnded(FikaGameEndedEvent ev)
        {
            if (ev.IsServer)
            {
                Plugin.Instance.Config.SettingChanged -= OnServerSettingChanged;
                return;
            }

            Plugin.Instance.Config.SettingChanged -= OnClientSettingChanged;

            foreach (var pair in SettingOverrides)
                pair.Key.SetSerializedValue(pair.Value.Original);

            SettingOverrides.Clear();
            ConfigReceived = false;

            Plugin.Instance.Config.Save();
            Plugin.Instance.Config.SaveOnConfigSet = true;

            Plugin.LogSource.LogDebug("[MOARSync] Reset sync state on game end");
        }

        private struct SyncedValues
        {
            public string Original;
            public string Override;
        }
    }
}
