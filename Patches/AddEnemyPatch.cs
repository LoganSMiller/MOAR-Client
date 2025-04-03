using System.Collections.Generic;
using System.Reflection;
using EFT;
using MOAR.Helpers;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Prevents friendly PMCs from targeting each other unless <c>factionAggression</c> is enabled in the settings.
    /// Also allows solo bots to still retaliate when alone.
    /// </summary>
    public class AddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(BotsGroup).GetMethod(nameof(BotsGroup.AddEnemy), BindingFlags.Instance | BindingFlags.Public);

        /// <summary>
        /// Prefix patch that filters out invalid or friendly-against-friendly AI targeting unless allowed by settings.
        /// </summary>
        /// <param name="__instance">The AI group instance.</param>
        /// <param name="person">The potential enemy player.</param>
        /// <param name="cause">The reason for being marked as an enemy.</param>
        /// <returns><c>true</c> to allow adding as enemy; <c>false</c> to skip.</returns>
        [PatchPrefix]
        protected static bool PatchPrefix(BotsGroup __instance, IPlayer person, EBotEnemyCause cause)
        {
            if (__instance == null || person == null || !person.IsAI)
            {
                if (Settings.debug.Value)
                    Plugin.LogSource.LogDebug("[AddEnemyPatch] Skipped null or non-AI target.");
                return true;
            }

            // Always allow aggression against different factions or scavs
            bool isOpposingFaction = __instance.Side != person.Side;
            bool isScav = person.Side == EPlayerSide.Savage || __instance.Side == EPlayerSide.Savage;

            if (isOpposingFaction || isScav)
                return true;

            List<BotOwner> groupMembers = __instance.GetAllMembers() ?? new List<BotOwner>();
            bool isSolo = groupMembers.Count <= 1;

            bool allowAggression = Settings.factionAggression.Value || isSolo;

            if (!allowAggression && Settings.debug.Value)
            {
                Plugin.LogSource.LogDebug($"[AddEnemyPatch] Suppressed same-faction aggression: {person.Profile?.Id}");
            }

            return allowAggression;
        }
    }
}
