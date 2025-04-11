using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace MOAR.AI.Optimization
{
    public class BotOwnerGroupOptimization
    {
        public void OptimizeGroupAI(List<BotOwner> botOwners)
        {
            foreach (var botOwner in botOwners)
            {
                if (botOwner?.Settings?.FileSettings?.Mind == null)
                    continue;

                var mind = botOwner.Settings.FileSettings.Mind;
                var moarOwner = botOwner.GetComponent<MOARBotOwner>();

                if (moarOwner == null)
                    continue;

                var profile = moarOwner.PersonalityProfile;

                // Simulate team cohesion affecting panic or coordination range
                mind.DIST_TO_FOUND_SQRT = Mathf.Lerp(300f, 600f, 1f - profile.Cohesion);

                // Simulate aggression scaling in team context
                mind.FRIEND_AGR_KILL = Mathf.Clamp(mind.FRIEND_AGR_KILL + profile.AggressionLevel * 0.15f, 0f, 1f);

                // Simulate better threat detection in groups
                mind.ENEMY_LOOK_AT_ME_ANG = Mathf.Clamp(mind.ENEMY_LOOK_AT_ME_ANG - profile.Cohesion * 5f, 5f, 30f);
            }
        }
    }
}
