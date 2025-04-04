﻿using System.Collections.Generic;
using System.Reflection;
using EFT;
using MOAR.Helpers;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Prevents friendly PMCs from targeting each other unless factionAggression is enabled.
    /// </summary>
    public class AddEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(BotsGroup).GetMethod("AddEnemy", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        protected static bool PatchPrefix(BotsGroup __instance, IPlayer person, EBotEnemyCause cause)
        {
            // Null checks and ensure target is an AI
            if (__instance == null || person == null || !person.IsAI)
                return true;

            // Always allow if target is Savage or opposite faction
            if (__instance.Side != person.Side || person.Side == EPlayerSide.Savage || __instance.Side == EPlayerSide.Savage)
                return true;

            List<BotOwner> groupMembers = __instance.GetAllMembers();
            bool isSolo = groupMembers.Count <= 1;

            // Only allow aggression if enabled or if bot is solo
            bool allowAggression = Settings.factionAggression.Value || isSolo;

            return allowAggression;
        }
    }
}
