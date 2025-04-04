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
    /// - Configured via 'factionAggression'
    /// - Or they are solo/isolated
    /// Always allows aggression across factions or against scavs.
    /// </summary>
    public class AddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(BotsGroup).GetMethod(nameof(BotsGroup.AddEnemy), BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Evaluates whether a bot should be marked as an enemy before adding.
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotsGroup __instance, IPlayer person, EBotEnemyCause cause)
        {
            if (__instance == null || person == null || !person.IsAI)
            {
                if (Settings.debug.Value)
                    Plugin.LogSource.LogDebug("[AddEnemyPatch] Skipped null, non-AI, or invalid target.");
                return true;
            }

            EPlayerSide groupSide = __instance.Side;
            EPlayerSide targetSide = person.Side;

            // Always allow opposing factions or scav involvement
            if (groupSide != targetSide || groupSide == EPlayerSide.Savage || targetSide == EPlayerSide.Savage)
                return true;

            // Evaluate solo bot fallback or configured aggression
            bool isSoloBot = (__instance.GetAllMembers()?.Count ?? 0) <= 1;
            bool allowSameFactionAggression = Settings.factionAggression.Value || isSoloBot;

            if (!allowSameFactionAggression && Settings.debug.Value)
            {
                string context = FikaBackendUtils.IsServer ? "[Headless]" : "[Client]";
                Plugin.LogSource.LogDebug($"{context} [AddEnemyPatch] Prevented {groupSide} bot from targeting {targetSide} (solo: {isSoloBot}, aggression: {Settings.factionAggression.Value})");
            }

            return allowSameFactionAggression;
        }
    }
}
