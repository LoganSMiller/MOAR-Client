using System.Linq;
using System.Reflection;
using EFT;
using EFT.Communications;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Displays the current MOAR preset with a randomized flair message when a raid starts.
    /// Prevents duplicate announcements when using FIKA Coop.
    /// </summary>
    public sealed class NotificationPatch : ModulePatch
    {
        /// <summary>
        /// Hooks into GameWorld.OnGameStarted to broadcast preset label.
        /// </summary>
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        /// <summary>
        /// Prefix logic to announce preset with flair if enabled and not in FIKA multiplayer.
        /// </summary>
        [PatchPrefix]
        private static void Prefix()
        {
            if (!Settings.ShowPresetOnRaidStart.Value)
                return;

            // Skip FIKA to avoid duplication — host handles it
            if (Settings.IsFika)
                return;

            var selected = Settings.PresetList.FirstOrDefault(p => p.Name == Settings.currentPreset.Value);
            var label = selected?.Label ?? Settings.currentPreset.Value ?? "Unknown";
            var flair = Plugin.GetFlairMessage();

            Methods.DisplayMessage($"Current preset is {label}{flair}", ENotificationIconType.EntryPoint);
        }
    }
}
