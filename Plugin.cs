using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Communications;
using Fika.Core;
using Fika.Core.Coop.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using HarmonyLib;
using LiteNetLib;
using MOAR.Components.Notifications;
using MOAR.Helpers;
using MOAR.Packets;
using MOAR.Patches;
using UnityEngine;
using Random = System.Random;

namespace MOAR
{
    [BepInPlugin("MOAR.settings", "MOAR-Refactored", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource LogSource;
        public static readonly string Version = "1.0.0";

        private static readonly Random _rng = new();
        private static bool _initialized;

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

            Logger.LogInfo("[MOAR] Awake — Starting initialization");

            try
            {
                Settings.Init(Config);
                Routers.Init(Config);

                new Harmony("com.moar.patches").PatchAll();

                if (Settings.IsFika)
                {
                    DebugNotification.RegisterNetworkHandler();
                    RegisterFikaEventListeners();
                }

                Logger.LogInfo("[MOAR] Initialization complete.");
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
            if (!Settings.IsFika || !FikaBackendUtils.IsServer)
                return;

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
            return shortcut.BetterIsDown() && Singleton<GameWorld>.Instantiated;
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

        private static void RegisterFikaEventListeners()
        {
            // Subscribing to the correct event class `PeerConnectedEvent`
            FikaEventDispatcher.SubscribeEvent<PeerConnectedEvent>(OnPeerConnected);
        }

        private static void OnPeerConnected(PeerConnectedEvent ev)
        {
            if (!FikaBackendUtils.IsServer || ev?.Peer == null)
                return;

            try
            {
                // Getting current preset name and label
                string presetName = Settings.GetCurrentPresetName();
                string presetLabel = Settings.GetCurrentPresetLabel();

                // Creating a packet to sync the preset
                var packet = new PresetSyncPacket(presetName, presetLabel);

                // Sending the packet to the peer using FikaServer
                if (Singleton<FikaServer>.Instantiated)
                {
                    Singleton<FikaServer>.Instance.SendDataToPeer(ev.Peer, ref packet, DeliveryMethod.ReliableUnordered);
                    LogSource.LogInfo($"[MOAR] [SYNC] Sent PresetSyncPacket to peer {ev.Peer.Id}: {presetLabel} ({presetName})");
                }
                else
                {
                    LogSource.LogWarning("[MOAR] [SYNC] FikaServer not instantiated, unable to send preset.");
                }
            }
            catch (Exception ex)
            {
                LogSource.LogError($"[MOAR] [SYNC] Failed to send preset to peer: {ex}");
            }
        }

        public static string GetFlairMessage()
        {
            var suffixes = new List<string>
            {
                ", good luck!", ", may the bots ever be in your favour.", ", you're probably screwed.",
                ", enjoy the dumpster fire.", ", hope you brought snacks.", ", prepare to be crushed.",
                ", try not to rage-quit.", ", it's going to be a long day for you.",
                ", let the feelings of dread pass over you."
            };

            return suffixes[_rng.Next(suffixes.Count)];
        }
    }
}
