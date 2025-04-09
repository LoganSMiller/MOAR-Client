using System;
using System.Reflection;
using EFT;
using Fika.Core.Coop.Utils;
using MOAR.Helpers;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Prevents friendly PMCs from attacking each other unless:
    /// - factionAggression is enabled
    /// - OR the bot is alone in its group
    /// Always allows aggression between different factions or when scavs are involved.
    /// Safe for all SPT and FIKA game modes (host/client/headless).
    /// </summary>
    public class AddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(BotsGroup).GetMethod(nameof(BotsGroup.AddEnemy), BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        private static bool PatchPrefix(BotsGroup __instance, IPlayer person, EBotEnemyCause cause)
        {
            try
            {
                if (__instance == null || person == null || !person.IsAI)
                {
                    if (Settings.debug.Value)
                        Plugin.LogSource.LogDebug("[AddEnemyPatch] Skipped: null group, null target, or target is not AI.");
                    return true;
                }

                string ctx = FikaBackendUtils.IsServer ? "[FIKA-Server]"
                          : FikaBackendUtils.IsClient ? "[FIKA-Client]"
                          : "[SPT-Solo]";

                var groupSide = __instance.Side;
                var targetSide = person.Side;

                // Always allow if different factions or either side is a Scav
                if (groupSide != targetSide || groupSide == EPlayerSide.Savage || targetSide == EPlayerSide.Savage)
                {
                    if (Settings.debug.Value)
                        Plugin.LogSource.LogDebug($"{ctx} [AddEnemyPatch] ALLOW: {groupSide} → {targetSide} (faction mismatch or scav)");
                    return true;
                }

                // Same side (PMC) — check if they're solo or aggression is enabled
                bool isSoloGroup = (__instance.GetAllMembers()?.Count ?? 0) <= 1;
                bool shouldAggress = Settings.factionAggression.Value || isSoloGroup;

                if (!shouldAggress && Settings.debug.Value)
                {
                    Plugin.LogSource.LogDebug($"{ctx} [AddEnemyPatch] BLOCK: Same side {groupSide}, Solo: {isSoloGroup}, factionAggression: {Settings.factionAggression.Value}");
                }

                return shouldAggress;
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[AddEnemyPatch] Exception occurred — fallback to default behavior:\n{ex}");
                return true; // Safe fallback: allow aggression to avoid blocking logic
            }
        }
    }
}
