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
    /// Prevents same-faction PMC bots from attacking each other unless:
    /// - 'factionAggression' is enabled
    /// - OR the bot is in a solo group (1 member)
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
                        Plugin.LogSource.LogDebug("[AddEnemyPatch] Skipped — null BotsGroup, null target, or target is not AI.");
                    return true;
                }

                string ctx = FikaBackendUtils.IsServer ? "[FIKA-Server]" :
                             FikaBackendUtils.IsClient ? "[FIKA-Client]" :
                             "[SPT-Solo]";

                var groupSide = __instance.Side;
                var targetSide = person.Side;

                //  Allow aggression if different faction or one is a Scav
                if (groupSide != targetSide || groupSide == EPlayerSide.Savage || targetSide == EPlayerSide.Savage)
                {
                    if (Settings.debug.Value)
                        Plugin.LogSource.LogDebug($"{ctx} [AddEnemyPatch] ALLOW — Faction mismatch or Scav involved: {groupSide} → {targetSide}");
                    return true;
                }

                //  Same faction — evaluate aggression logic
                bool isSoloGroup = (__instance.GetAllMembers()?.Count ?? 0) <= 1;
                bool shouldAggress = Settings.factionAggression.Value || isSoloGroup;

                if (!shouldAggress && Settings.debug.Value)
                {
                    Plugin.LogSource.LogDebug($"{ctx} [AddEnemyPatch] BLOCK — Same side {groupSide}, Solo: {isSoloGroup}, factionAggression: {Settings.factionAggression.Value}");
                }

                return shouldAggress;
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[AddEnemyPatch] Exception occurred — allowing fallback aggression:\n{ex}");
                return true; // Fail-safe
            }
        }
    }
}
