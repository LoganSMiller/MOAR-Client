using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Communications;
using Fika.Core.Coop;
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
        private static bool _configReceived = false;
        private static bool _ignoreClientSettingChange = false;
        private static readonly Dictionary<string, string> _clientOverrides = new();

        public static bool ConfigReceived => _configReceived;

        public static void RegisterFikaEventListeners()
        {
            Plugin.LogSource.LogInfo("[MOARSync] Registering FIKA lifecycle events...");

            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkCreated);
            FikaEventDispatcher.SubscribeEvent<PeerConnectedEvent>(OnPeerConnected);
            FikaEventDispatcher.SubscribeEvent<FikaRaidStartedEvent>(OnRaidStarted);
            FikaEventDispatcher.SubscribeEvent<FikaGameEndedEvent>(OnRaidEnded);
        }

        private static void OnNetworkCreated(FikaNetworkManagerCreatedEvent ev)
        {
            Plugin.LogSource.LogDebug("[MOARSync] FikaNetworkManagerCreated");

            if (ev.Manager is FikaServer)
            {
                Plugin.Instance.Config.SettingChanged += OnServerSettingChanged;
            }
            else if (ev.Manager is FikaClient client)
            {
                Plugin.Instance.Config.SettingChanged += OnClientSettingChanged;

                client.RegisterPacket<PresetSyncPacket>(HandleClientPacket);
                Plugin.LogSource.LogInfo("[MOARSync] Registered PresetSyncPacket handler on client");
            }
        }

        private static void OnPeerConnected(PeerConnectedEvent ev)
        {
            if (!FikaBackendUtils.IsServer)
                return;

            try
            {
                var preset = Settings.currentPreset?.Value ?? "live-like";
                var label = Routers.GetAnnouncePresetLabel();
                var packet = new PresetSyncPacket(preset, label)
                {
                    Version = Plugin.Version
                };

                Plugin.LogSource.LogInfo($"[MOARSync] Peer {ev.Peer.Id} connected — syncing preset '{label}'");

                Singleton<FikaServer>.Instance.SendDataToPeer(ev.Peer, ref packet, DeliveryMethod.ReliableUnordered);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[MOARSync] Failed to send preset to peer {ev.Peer.Id}: {ex.Message}");
            }
        }

        private static void OnServerSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (!FikaBackendUtils.IsServer || args.ChangedSetting != Settings.currentPreset)
                return;

            try
            {
                var packet = new PresetSyncPacket(
                    Settings.currentPreset?.Value ?? "live-like",
                    Routers.GetAnnouncePresetLabel()
                )
                {
                    Version = Plugin.Version
                };

                Plugin.LogSource.LogInfo("[MOARSync] Preset changed — broadcasting to all clients");

                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableUnordered);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[MOARSync] Failed to broadcast preset sync: {ex.Message}");
            }
        }

        private static void OnClientSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (_ignoreClientSettingChange || args.ChangedSetting != Settings.currentPreset)
                return;

            if (_clientOverrides.TryGetValue("preset", out var overridden))
            {
                _ignoreClientSettingChange = true;
                Settings.currentPreset.SetSerializedValue(overridden);
                Plugin.LogSource.LogInfo($"[MOARSync] Overriding client preset with: {overridden}");
                _ignoreClientSettingChange = false;
            }
        }

        private static void HandleClientPacket(PresetSyncPacket packet)
        {
            Plugin.LogSource.LogInfo($"[MOARSync] Received preset from host: {packet.PresetLabel} ({packet.PresetName})");

            _configReceived = true;
            _clientOverrides["preset"] = packet.PresetName;
            _ignoreClientSettingChange = true;

            var match = Settings.PresetList?.FirstOrDefault(p =>
                string.Equals(p.Name, packet.PresetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Label, packet.PresetLabel, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                Settings.currentPreset.SetSerializedValue(match.Name);
                Routers.SetHostPresetLabel(match.Label);
                Plugin.LogSource.LogInfo($"[MOARSync] Synced preset applied: {match.Label}");

                NotificationManagerClass.DisplayMessageNotification(
                    $"MOAR preset synced: {match.Label}",
                    ENotificationDurationType.Default,
                    ENotificationIconType.EntryPoint
                );
            }
            else
            {
                Plugin.LogSource.LogWarning($"[MOARSync] No local match for host preset: {packet.PresetName}");
            }

            _ignoreClientSettingChange = false;
        }

        private static void OnRaidStarted(FikaRaidStartedEvent ev)
        {
            if (!ev.IsServer && !_configReceived)
            {
                Plugin.LogSource.LogError("[MOARSync] Preset sync failed — host may be missing MOAR.");
                NotificationManagerClass.DisplayWarningNotification(
                    "MOAR preset sync failed — Host may be missing MOAR!",
                    ENotificationDurationType.Long
                );
            }
        }

        private static void OnRaidEnded(FikaGameEndedEvent ev)
        {
            Plugin.LogSource.LogDebug("[MOARSync] Raid ended — cleaning up sync state");

            if (ev.IsServer)
            {
                Plugin.Instance.Config.SettingChanged -= OnServerSettingChanged;
                return;
            }

            Plugin.Instance.Config.SettingChanged -= OnClientSettingChanged;
            _clientOverrides.Clear();
            _configReceived = false;

            Plugin.Instance.Config.SaveOnConfigSet = true;
            Plugin.Instance.Config.Save();
        }
    }
}
