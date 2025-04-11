#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Communications;
using Fika.Core.Coop.Utils;
using HarmonyLib;
using MOAR.Components.Notifications;
using MOAR.Helpers;
using MOAR.Patches;
using MOAR.AI;
using MOAR.AI.Optimization;
using UnityEngine;

namespace MOAR
{
    [BepInPlugin("MOAR.settings", "MOAR-Refactored", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin? Instance { get; private set; }
        public static ManualLogSource LogSource = null!;
        private static readonly System.Random _rng = new();
        private static bool _initialized;
        private static bool _patched;

        private GameObject? _aiRoot;
        private HotspotSystem? _hotspotSystem;

        private async void Awake()
        {
            if (_initialized)
            {
                Logger.LogWarning("[MOAR] Already initialized. Skipping duplicate Awake.");
                return;
            }

            _initialized = true;
            Instance = this;
            LogSource = Logger;

            Logger.LogInfo("[MOAR] Awake — Starting async initialization");

            try
            {
                await Settings.InitAsync(Config);
                new Harmony("com.moar.patches").PatchAll();

                if (Settings.IsFika)
                {
                    DebugNotification.RegisterNetworkHandler();
                }

                BotOwnerAIController.Initialize(Logger); 

                Logger.LogInfo($"[MOAR] Initialization complete. Preset: {Settings.GetCurrentPresetName()}");
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
                InitAIBehaviors();
                Logger.LogInfo("[MOAR] Start complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[MOAR] Start failed: {ex}");
            }
        }

        private void InitAIBehaviors()
        {
            _aiRoot = new GameObject("MOAR_HostAI");
            DontDestroyOnLoad(_aiRoot);

            _hotspotSystem = _aiRoot.AddComponent<HotspotSystem>(); 
        }

        private void Update()
        {
            if (ShouldHandleInput())
            {
                HandleInput();
            }

            BotOwnerAIController.Update(); 

            if (_hotspotSystem != null)
            {
                // Future: Could expose reloader hotkey or debug inspector
            }
        }

        private static bool ShouldHandleInput()
        {
            return Settings.IsFika
                && FikaBackendUtils.IsServer
                && Settings.AreHotkeysReady()
                && Singleton<GameWorld>.Instantiated;
        }

        private static void HandleInput()
        {
            if (ConfigEntryExtensions.BetterIsDown(Settings.DeleteBotOwnerSpawn!.Value))
                AnnounceResult(Routers.DeleteBotOwnerSpawn(), "Deleted 1 BotOwner spawn point");

            if (ConfigEntryExtensions.BetterIsDown(Settings.AddBotOwnerSpawn!.Value))
                AnnounceResult(Routers.AddBotOwnerSpawn(), "Added 1 BotOwner spawn point");

            if (ConfigEntryExtensions.BetterIsDown(Settings.AddSniperSpawn!.Value))
                AnnounceResult(Routers.AddSniperSpawn(), "Added 1 sniper spawn point");

            if (ConfigEntryExtensions.BetterIsDown(Settings.AddPlayerSpawn!.Value))
                AnnounceResult(Routers.AddPlayerSpawn(), "Added 1 player spawn point");

            if (ConfigEntryExtensions.BetterIsDown(Settings.AnnounceKey!.Value))
                Settings.AnnounceManually();
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

        private static void EnablePatches()
        {
            if (_patched) return;
            _patched = true;

            TryPatch("SniperPatch", () => new SniperPatch().Enable());
            TryPatch("AddEnemyPatch", () => new AddEnemyPatch().Enable());
            TryPatch("NotificationPatch", () => new NotificationPatch().Enable());

            if (Settings.enablePointOverlay?.Value == true)
            {
                TryPatch("OnGameStartedPatch", () => new OnGameStartedPatch().Enable());
            }
        }

        private static void TryPatch(string name, Action patchAction)
        {
            try
            {
                patchAction();
            }
            catch (Exception ex)
            {
                LogSource.LogWarning($"[MOAR] {name} failed: {ex.Message}");
            }
        }

        public static string GetFlairMessage()
        {
            var suffixes = new List<string>
            {
                ", good luck!",
                ", may the BotOwners ever be in your favour.",
                ", you're probably screwed.",
                ", enjoy the dumpster fire.",
                ", hope you brought snacks.",
                ", prepare to be crushed.",
                ", try not to rage-quit.",
                ", it's going to be a long day for you.",
                ", let the feelings of dread pass over you."
            };

            return suffixes[_rng.Next(suffixes.Count)];
        }
    }
}
