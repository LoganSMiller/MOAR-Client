using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Communications;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using MOAR.Components.Notifications;
using MOAR.Helpers;
using MOAR.Networking;
using MOAR.Patches;
using Fika.Core.Networking;
using Fika.Core.Coop;
using Fika.Core.Coop.Utils;
using Fika.Core.Coop.GameMode;
using MOAR.Packets;

namespace MOAR
{
    [BepInPlugin("MOAR.settings", "MOAR-Refactored", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource LogSource;
        private static readonly Random _rng = new();
        private static string _hostPresetLabel = "Unknown";
        private static bool _initialized = false;

        private void Awake()
        {
            if (_initialized)
            {
                Logger.LogWarning("[MOAR] Plugin already initialized. Skipping duplicate Awake.");
                return;
            }

            _initialized = true;
            Instance = this;
            LogSource = Logger;

            Logger.LogInfo("[MOAR] Awake - Initializing plugin");

            try
            {
                Settings.Init(Config);
                Routers.Init(Config);

                new Harmony("com.moar.patches").PatchAll();

                if (Settings.IsFika && Singleton<FikaServer>.Instantiated && Singleton<FikaClient>.Instantiated)
                {
                    DebugNotification.RegisterNetworkHandler();

                    var networkManager = Singleton<IFikaNetworkManager>.Instance;
                    if (networkManager != null)
                    {
                        networkManager.RegisterPacket<PresetSyncPacket>(OnClientReceivedPresetPacket);
                        Logger.LogInfo("[MOAR] PresetSyncPacket handler registered.");
                    }
                    else
                    {
                        LogSource.LogError("[MOAR] FIKA NetworkManager not available for packet registration.");
                    }
                }
                else if (Settings.IsFika)
                {
                    LogSource.LogError("[MOAR] FIKA detected but networking components unavailable.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[MOAR] Initialization failed: {ex}");
            }
        }

        private void Start()
        {
            try
            {
                EnablePatches();

                if (Settings.IsFika && FikaBackendUtils.IsServer)
                {
                    BroadcastPresetToClients(Settings.currentPreset.Value, Routers.GetAnnouncePresetName());
                }

                Logger.LogInfo("[MOAR] Start complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[MOAR] Start failed: {ex}");
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private static void HandleInput()
        {
            if (Settings.IsFika)
                MOARCoopPacketRouter.TryRegister(); // ✅ Retry packet registration if needed

            if (TryPress(Settings.DeleteBotSpawn.Value))
                AnnounceResult(Routers.DeleteBotSpawn(), "Deleted 1 bot spawn point");

            if (TryPress(Settings.AddBotSpawn.Value))
                AnnounceResult(Routers.AddBotSpawn(), "Added 1 bot spawn point");

            if (TryPress(Settings.AddSniperSpawn.Value))
                AnnounceResult(Routers.AddSniperSpawn(), "Added 1 sniper spawn point");

            if (TryPress(Settings.AddPlayerSpawn.Value))
                AnnounceResult(Routers.AddPlayerSpawn(), "Added 1 player spawn point");

            if (Settings.AnnounceKey.Value.BetterIsDown())
            {
                AnnouncePresetManually();
            }
        }

        private static bool TryPress(KeyboardShortcut shortcut)
        {
            return shortcut.BetterIsDown() && Singleton<GameWorld>.Instance?.MainPlayer != null;
        }

        private static void AnnouncePresetManually(string location = "Unknown")
        {
            var presetName = Settings.IsFika ? _hostPresetLabel : Routers.GetAnnouncePresetName();

            var notification = new DebugNotification
            {
                Notification = $"Current preset is {presetName}",
                NotificationIcon = ENotificationIconType.EntryPoint
            };

            notification.Display();

            if (Settings.IsFika && FikaBackendUtils.IsServer)
                notification.BroadcastToClients();
        }

        private static void AnnounceResult(string result, string fallbackMessage)
        {
            var location = Singleton<GameWorld>.Instance?.MainPlayer?.Location ?? "Unknown";
            var message = string.IsNullOrWhiteSpace(result) ? fallbackMessage : result;

            var notification = new DebugNotification
            {
                Notification = $"{message} in {location}",
                NotificationIcon = ENotificationIconType.Default
            };

            notification.Display();

            if (Settings.IsFika && FikaBackendUtils.IsServer)
                notification.BroadcastToClients();
        }

        private static void BroadcastPresetToClients(string presetName, string presetLabel)
        {
            var packet = new PresetSyncPacket(presetName, presetLabel);
            Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered, null);
        }

        private static void OnClientReceivedPresetPacket(PresetSyncPacket packet)
        {
            _hostPresetLabel = packet.PresetLabel;
            Settings.currentPreset.Value = packet.PresetName;

            var notification = new DebugNotification
            {
                Notification = $"Preset synced from host: {_hostPresetLabel}",
                NotificationIcon = ENotificationIconType.EntryPoint
            };

            notification.Display();
            LogSource.LogInfo($"Preset synced from host: {_hostPresetLabel}");
        }

        private static void EnablePatches()
        {
            new SniperPatch().Enable();
            new AddEnemyPatch().Enable();
            new NotificationPatch().Enable();

            if (Settings.enablePointOverlay.Value)
                new OnGameStartedPatch().Enable();
        }

        public static string GetFlairMessage()
        {
            var suffixes = new List<string>
            {
                ", good luck!", ", may the bots ever be in your favour.", ", you're probably screwed.",
                ", enjoy the dumpster fire.", ", hope you brought snacks.", ", prepare to be crushed.",
                ", try not to rage-quit.", ", it's going to be a long day for you.", ", let the feelings of dread pass over you.",
            };

            return suffixes[_rng.Next(suffixes.Count)];
        }
    }
}
