using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Communications;
using Fika.Core.Coop.PacketHandlers;
using Fika.Core.Coop.Utils;
using Fika.Core;
using HarmonyLib;
using MOAR.Components.Notifications;
using MOAR.Helpers;
using MOAR.Networking;
using MOAR.Packets;
using MOAR.Patches;

namespace MOAR
{
    [BepInPlugin("MOAR.settings", "MOAR-Refactored", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource LogSource;
        private static readonly Random _rng = new();
        private static bool _initialized = false;

        private void Awake()
        {
            if (_initialized)
            {
                Logger.LogWarning("[MOAR] Already initialized. Skipping duplicate Awake.");
                return;
            }

            _initialized = true;
            Instance = this;
            LogSource = Logger;

            Logger.LogInfo("[MOAR] Awake - Starting initialization");

            try
            {
                Settings.Init(Config);
                Routers.Init(Config);

                new Harmony("com.moar.patches").PatchAll();

                if (Settings.IsFika)
                {
                    DebugNotification.RegisterNetworkHandler();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[MOAR] Initialization error: {ex}");
            }
        }

        private void Start()
        {
            try
            {
                EnablePatches();

                // Broadcast preset at raid start if this instance is headless/server
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
            // Retry-safe registration of packet handlers (if we're the server/headless)
            if (Settings.IsFika)
                MOARCoopPacketRouter.TryRegister();

            // Dev tools
            if (TryPress(Settings.DeleteBotSpawn.Value))
                AnnounceResult(Routers.DeleteBotSpawn(), "Deleted 1 bot spawn point");

            if (TryPress(Settings.AddBotSpawn.Value))
                AnnounceResult(Routers.AddBotSpawn(), "Added 1 bot spawn point");

            if (TryPress(Settings.AddSniperSpawn.Value))
                AnnounceResult(Routers.AddSniperSpawn(), "Added 1 sniper spawn point");

            if (TryPress(Settings.AddPlayerSpawn.Value))
                AnnounceResult(Routers.AddPlayerSpawn(), "Added 1 player spawn point");

            if (Settings.AnnounceKey.Value.BetterIsDown())
                AnnouncePresetManually();
        }

        private static bool TryPress(KeyboardShortcut shortcut)
        {
            return shortcut.BetterIsDown() && Singleton<GameWorld>.Instance?.MainPlayer != null;
        }

        private static void AnnouncePresetManually()
        {
            var label = Routers.GetAnnouncePresetLabel();

            var notification = new DebugNotification
            {
                Notification = $"Current preset is {label}",
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
            if (!Settings.IsFika || !FikaBackendUtils.IsServer)
                return;

            try
            {
                // Use existing sync system
                var packet = new PresetSyncPacket(presetName, presetLabel);
                MOARPresetSyncHandler.OnClientReceivedPresetPacket(packet); // Apply locally

                // Use FIKA's registered routing logic to broadcast
                DebugNotification notification = new DebugNotification
                {
                    Notification = $"Preset synced from host: {presetLabel}",
                    NotificationIcon = ENotificationIconType.EntryPoint
                };

                notification.BroadcastToClients(); // Uses working wrapper
                LogSource.LogInfo($"[MOAR] Broadcasted preset sync: {presetLabel} ({presetName})");
            }
            catch (Exception ex)
            {
                LogSource.LogError($"[MOAR] BroadcastPresetToClients failed: {ex.Message}");
            }
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
