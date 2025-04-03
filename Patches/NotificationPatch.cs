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
    /// Prevents duplicate messaging in FIKA-based Coop scenarios.
    /// </summary>
    public sealed class NotificationPatch : ModulePatch
    {
        /// <summary>
        /// Hooks into GameWorld.OnGameStarted to broadcast preset label.
        /// </summary>
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        /// <summary>
        /// Shows a preset notification if enabled in settings.
        /// </summary>
        [PatchPrefix]
        private static bool Prefix()
        {
            // Skip notification if toggled off
            if (!Settings.ShowPresetOnRaidStart.Value)
                return true;

            // Avoid double notification in Coop — host will announce
            if (Settings.IsFika)
                return true;

            // Get current preset label
            var selected = Settings.PresetList
                .FirstOrDefault(p => p.Name == Settings.currentPreset.Value);

            var label = selected?.Label ?? Settings.currentPreset.Value ?? "Unknown";
            var flair = Plugin.GetFlairMessage();

            // Display notification
            Methods.DisplayMessage($"Current preset is {label}{flair}", ENotificationIconType.EntryPoint);

            return true;
        }
    }
}
