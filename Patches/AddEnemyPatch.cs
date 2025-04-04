using System.Collections.Generic;
using System.Reflection;
using EFT;
using MOAR.Helpers;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Prevents same-faction PMC AI from targeting each other unless faction-based aggression is enabled,
    /// or if the bot is solo and not part of a coordinated squad.
    /// </summary>
    public class AddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(BotsGroup).GetMethod(nameof(BotsGroup.AddEnemy), BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Filters out friendly-fire bot targeting unless allowed by settings or solo.
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(BotsGroup __instance, IPlayer person, EBotEnemyCause cause)
        {
            if (__instance == null || person == null || !person.IsAI)
            {
                if (Settings.debug.Value)
                    Plugin.LogSource.LogDebug("[AddEnemyPatch] Skipped null or non-AI target.");
                return true;
            }

            var groupSide = __instance.Side;
            var targetSide = person.Side;

            // Allow aggression if different factions or scavs are involved
            if (groupSide != targetSide || groupSide == EPlayerSide.Savage || targetSide == EPlayerSide.Savage)
                return true;

            // Solo bots or factionAggression setting overrides default friendly-fire prevention
            bool isSolo = (__instance.GetAllMembers()?.Count ?? 0) <= 1;
            bool allowAggression = Settings.factionAggression.Value || isSolo;

            if (!allowAggression && Settings.debug.Value)
            {
                Plugin.LogSource.LogDebug($"[AddEnemyPatch] Blocked same-faction aggression between {groupSide} bots.");
            }

            return allowAggression;
        }
    }
}
