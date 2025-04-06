using System;
using System.Reflection;
using HarmonyLib;
using EFT.UI.Matchmaker;
using EFT.Communications;
using Fika.Core.Coop.Utils;
using MOAR.Helpers;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Displays the current preset on raid start if enabled in config.
    /// Safe for local, host, client, and headless environments.
    /// </summary>
    public class NotificationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Target: public override void Show(TimeHasComeScreenClass controller)
            return typeof(MatchmakerTimeHasCome).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                null,
                new Type[] { typeof(MatchmakerTimeHasCome.TimeHasComeScreenClass) },
                null
            );
        }

        [PatchPostfix]
        public static void Postfix()
        {
            try
            {
                // Only show preset if enabled
                if (!Settings.ShowPresetOnRaidStart.Value)
                    return;

                // Skip if FIKA is active and we are a headless host
                if (Settings.IsFika && FikaBackendUtils.IsHeadless)
                {
                    Plugin.LogSource.LogDebug("[NotificationPatch] Skipped preset display — FIKA headless mode.");
                    return;
                }

                string label = Routers.GetCurrentPresetLabel();
                if (!string.IsNullOrWhiteSpace(label))
                {
                    Methods.DisplayMessage($"Live preset: {label}", ENotificationIconType.Quest);
                    Plugin.LogSource.LogInfo($"[NotificationPatch] Preset displayed: {label}");
                }
                else
                {
                    Plugin.LogSource.LogWarning("[NotificationPatch] No preset label available to display.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[NotificationPatch] Exception during notification display: {ex}");
            }
        }
    }
}
