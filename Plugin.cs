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
using MOAR.Helpers;
using MOAR.Packets;
using MOAR.Patches;
using MOAR.Networking;
using MOAR.Components.Notifications;
using Fika.Core.Coop;
using Fika.Core.Coop.GameMode;
using Fika.Core.Coop.Utils;
using Fika.Core.Networking;

namespace MOAR
{
    [BepInPlugin("MOAR.settings", "MOAR-Refactored", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static Plugin Instance { get; private set; }

        private static bool _initialized;
        private static readonly Random _rng = new();
        private static string _hostPresetLabel = "Unknown";

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

                if (Settings.IsFika && !FikaBackendUtils.IsServer)
                {
                    DebugNotification.RegisterNetworkHandler();
                    MOARCoopPacketRouter.Register();
                }

                Logger.LogInfo("[MOAR] Awake complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[MOAR] Plugin Awake failed: {ex}");
            }
        }

        private void Start()
        {
            if (!_initialized)
            {
                Logger.LogError("[MOAR] Start called without proper Awake initialization.");
                return;
            }

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

        private void EnablePatches()
        {
            try
            {
                new SniperPatch().Enable();
                new AddEnemyPatch().Enable();
                new NotificationPatch().Enable();

                if (Settings.enablePointOverlay.Value)
                    new OnGameStartedPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"[MOAR] EnablePatches failed: {ex}");
            }
        }

        private void Update()
        {
            if (!_initialized) return;

            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null) return;

            if (TryPress(Settings.DeleteBotSpawn.Value))
                AnnounceResult(Routers.DeleteBotSpawn(), "Deleted 1 bot spawn point", player.Location);

            if (TryPress(Settings.AddBotSpawn.Value))
                AnnounceResult(Routers.AddBotSpawn(), "Added 1 bot spawn point", player.Location);

            if (TryPress(Settings.AddSniperSpawn.Value))
                AnnounceResult(Routers.AddSniperSpawn(), "Added 1 sniper spawn point", player.Location);

            if (TryPress(Settings.AddPlayerSpawn.Value))
                AnnounceResult(Routers.AddPlayerSpawn(), "Added 1 player spawn point", player.Location);

            if (Settings.AnnounceKey.Value.BetterIsDown())
                AnnouncePresetManually(player.Location);
        }

        private static bool TryPress(KeyboardShortcut shortcut) =>
            shortcut.BetterIsDown();

        private static void AnnounceResult(string result, string fallbackMessage, string location = "Unknown")
        {
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

        private static void BroadcastPresetToClients(string presetName, string presetLabel)
        {
            if (Singleton<FikaServer>.Instantiated && Singleton<FikaServer>.Instance != null)
            {
                var packet = new PresetSyncPacket(presetName, presetLabel);
                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered, null);
                LogSource.LogInfo($"[MOAR] Broadcasted PresetSyncPacket: {presetName} / {presetLabel}");
            }
            else
            {
                LogSource.LogError("[MOAR] Failed to broadcast preset: FikaServer not instantiated.");
            }
        }

        public static string GetFlairMessage()
        {
            var suffixes = new List<string>
            {
                ", good luck!", ", may the bots ever be in your favour.", ", you're probably screwed.",
                ", enjoy the dumpster fire.", ", hope you brought snacks.", ", prepare to be crushed.",
                ", try not to rage-quit.", ", it's going to be a long day for you.", ", let the feelings of dread pass over you."
            };

            return suffixes[_rng.Next(suffixes.Count)];
        }
    }
}
