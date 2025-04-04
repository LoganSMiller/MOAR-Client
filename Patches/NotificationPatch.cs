using System.Linq;
using System.Reflection;
using EFT;
using EFT.Communications;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using Fika.Core.Coop.Utils;

namespace MOAR.Patches
{
    /// <summary>
    /// Displays the current MOAR preset with a randomized flair message when a raid starts.
    /// Automatically avoids duplication in FIKA multiplayer and respects headless host logic.
    /// </summary>
    public sealed class NotificationPatch : ModulePatch
    {
        /// <summary>
        /// Hooks into GameWorld.OnGameStarted to broadcast preset label.
        /// </summary>
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        /// <summary>
        /// Prefix logic to announce preset with flair if enabled.
        /// Skips FIKA clients and non-hosts to avoid duplicate messaging.
        /// </summary>
        [PatchPrefix]
        private static void Prefix()
        {
            if (!Settings.ShowPresetOnRaidStart.Value)
                return;

            // Prevent redundant notifications if part of FIKA multiplayer
            if (Settings.IsFika && !FikaBackendUtils.IsServer)
            {
                Plugin.LogSource.LogDebug("[NotificationPatch] Skipped client-side preset announcement (FIKA active).");
                return;
            }

            var selected = Settings.PresetList.FirstOrDefault(p => p.Name == Settings.currentPreset.Value);
            var label = selected?.Label ?? Settings.currentPreset.Value ?? "Unknown";
            var flair = Plugin.GetFlairMessage();

            Methods.DisplayMessage($"Current preset is {label}{flair}", ENotificationIconType.EntryPoint);
        }
    }
}
