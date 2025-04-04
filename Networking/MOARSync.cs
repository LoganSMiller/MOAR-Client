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
        private static bool _configReceived = false;
        private static readonly Dictionary<string, string> _clientOverrides = new();
        private static bool _ignoreClientSettingChange = false;

        public static void RegisterFikaEventListeners()
        {
            Plugin.LogSource.LogInfo("[MOARSync] Registering FIKA event listeners...");

            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkCreated);
            FikaEventDispatcher.SubscribeEvent<PeerConnectedEvent>(OnPeerConnected);
            FikaEventDispatcher.SubscribeEvent<FikaRaidStartedEvent>(OnRaidStarted);
            FikaEventDispatcher.SubscribeEvent<FikaGameEndedEvent>(OnRaidEnded);
        }

        private static void OnNetworkCreated(FikaNetworkManagerCreatedEvent ev)
        {
            Plugin.LogSource.LogDebug("[MOARSync] FikaNetworkManager created");

            if (ev.Manager is FikaServer)
            {
                Plugin.Instance.Config.SettingChanged += OnServerSettingChanged;
            }
            else if (ev.Manager is FikaClient client)
            {
                Plugin.Instance.Config.SettingChanged += OnClientSettingChanged;
                client.RegisterPacket<PresetSyncPacket>(HandleClientPacket);
            }
        }

        private static void OnPeerConnected(PeerConnectedEvent ev)
        {
            if (!FikaBackendUtils.IsServer)
                return;

            var currentPreset = Settings.currentPreset?.Value ?? "live-like";
            var label = Routers.GetAnnouncePresetLabel();
            var packet = new PresetSyncPacket(currentPreset, label);

            Plugin.LogSource.LogInfo($"[MOARSync] Peer connected, syncing preset '{label}' to peer {ev.Peer.Id}");
            Singleton<FikaServer>.Instance.SendDataToPeer(ev.Peer, ref packet, DeliveryMethod.ReliableUnordered);
        }

        private static void OnServerSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (!FikaBackendUtils.IsServer)
                return;

            if (args.ChangedSetting == Settings.currentPreset)
            {
                Plugin.LogSource.LogInfo("[MOARSync] Preset changed, broadcasting sync");

                var packet = new PresetSyncPacket(
                    Settings.currentPreset?.Value ?? "live-like",
                    Routers.GetAnnouncePresetLabel()
                );

                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableUnordered);
            }
        }

        private static void OnClientSettingChanged(object sender, SettingChangedEventArgs args)
        {
            if (_ignoreClientSettingChange || args.ChangedSetting != Settings.currentPreset)
                return;

            _ignoreClientSettingChange = true;

            if (_clientOverrides.TryGetValue("preset", out var presetOverride))
            {
                Settings.currentPreset.SetSerializedValue(presetOverride);
                Plugin.LogSource.LogInfo($"[MOARSync] Restored synced preset: {presetOverride}");
            }

            _ignoreClientSettingChange = false;
        }

        private static void OnRaidStarted(FikaRaidStartedEvent ev)
        {
            if (!ev.IsServer && !_configReceived)
            {
                Plugin.LogSource.LogError("[MOARSync] Preset sync failed! MOAR missing on host?");
                NotificationManagerClass.DisplayWarningNotification(
                    "MOAR preset sync failed — Host is missing MOAR?",
                    ENotificationDurationType.Long
                );
            }
        }

        private static void OnRaidEnded(FikaGameEndedEvent ev)
        {
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

        private static void HandleClientPacket(PresetSyncPacket packet)
        {
            Plugin.LogSource.LogInfo($"[MOARSync] Received preset from host: {packet.PresetLabel} ({packet.PresetName})");
            _configReceived = true;

            _ignoreClientSettingChange = true;
            _clientOverrides["preset"] = packet.PresetName;

            var match = Settings.PresetList?.FirstOrDefault(p =>
                string.Equals(p.Name, packet.PresetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Label, packet.PresetLabel, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                Settings.currentPreset.SetSerializedValue(match.Name);
                Routers.SetHostPresetLabel(match.Label);
                Plugin.LogSource.LogInfo($"[MOARSync] Applied synced preset: {match.Label}");
            }
            else
            {
                Plugin.LogSource.LogWarning($"[MOARSync] Host preset not found locally: {packet.PresetName}");
            }

            _ignoreClientSettingChange = false;
        }
    }
}
