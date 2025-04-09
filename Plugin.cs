#nullable enable
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

namespace MOAR
{
    [BepInPlugin("MOAR.settings", "MOAR-Refactored", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin? Instance { get; private set; }
        public static ManualLogSource LogSource = null!;
        private static readonly Random _rng = new();
        private static bool _initialized;
        private static bool _patched;

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
                Logger.LogInfo("[MOAR] Start complete.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[MOAR] Start failed: {ex}");
            }
        }

        private void Update()
        {
            if (ShouldHandleInput())
            {
                HandleInput();
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
            if (ConfigEntryExtensions.BetterIsDown(Settings.DeleteBotSpawn!.Value))
                AnnounceResult(Routers.DeleteBotSpawn(), "Deleted 1 bot spawn point");

            if (ConfigEntryExtensions.BetterIsDown(Settings.AddBotSpawn!.Value))
                AnnounceResult(Routers.AddBotSpawn(), "Added 1 bot spawn point");

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
                ", may the bots ever be in your favour.",
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
