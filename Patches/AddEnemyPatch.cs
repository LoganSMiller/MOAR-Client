using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using MOAR.Helpers;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;
using Fika.Core.Coop.Utils;

namespace MOAR.Patches
{
    /// <summary>
    /// Prevents same-faction PMC bots from attacking each other unless:
    /// - 'factionAggression' is enabled
    /// - OR they are solo (only 1 bot in group)
    /// Always allows aggression across factions or against scavs.
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
                        Plugin.LogSource.LogDebug("[AddEnemyPatch] Skipped null, non-AI, or missing BotsGroup.");
                    return true; // Let default logic run
                }

                var ctx = FikaBackendUtils.IsServer ? "[FIKA-Server]" :
                          FikaBackendUtils.IsClient ? "[FIKA-Client]" : "[SPT-Solo]";

                var groupSide = __instance.Side;
                var targetSide = person.Side;

                // Always allow if cross-faction or either is scav
                if (groupSide != targetSide || groupSide == EPlayerSide.Savage || targetSide == EPlayerSide.Savage)
                {
                    if (Settings.debug.Value)
                        Plugin.LogSource.LogDebug($"{ctx} [AddEnemyPatch] ALLOW — Different faction or scav: {groupSide} → {targetSide}");
                    return true;
                }

                bool isSoloGroup = (__instance.GetAllMembers()?.Count ?? 0) <= 1;
                bool shouldAggress = Settings.factionAggression.Value || isSoloGroup;

                if (!shouldAggress)
                {
                    if (Settings.debug.Value)
                        Plugin.LogSource.LogDebug($"{ctx} [AddEnemyPatch] BLOCK — Same side: {groupSide}, solo: {isSoloGroup}, factionAggression: {Settings.factionAggression.Value}");
                }

                return shouldAggress;
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[AddEnemyPatch] Exception during AddEnemy check: {ex}");
                return true; // Fail-safe: allow aggression if logic fails
            }
        }
    }
}
