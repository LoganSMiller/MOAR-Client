using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace MOAR.AI.Optimization
{
    public class BotAIOptimization
    {
        private readonly Dictionary<string, bool> _optimizationApplied = new();

        public void Optimize(BotOwner botOwner)
        {
            if (botOwner?.Profile == null || botOwner.Memory == null)
                return;

            string botId = botOwner.Profile.Id;

            if (_optimizationApplied.TryGetValue(botId, out var alreadyOptimized) && alreadyOptimized)
                return;

            ApplyVisionOptimization(botOwner);
            ApplyHearingOptimization(botOwner);
            ApplyAggressionTweaks(botOwner);

            _optimizationApplied[botId] = true;
        }

        private void ApplyVisionOptimization(BotOwner bot)
        {
            var mindSettings = bot.Settings?.FileSettings?.Mind;
            var lookData = bot.Settings?.FileSettings?.Look;

            if (mindSettings == null || lookData == null)
                return;

            float range = GetVisionRange(bot);

            lookData.MaxDist = range;
            lookData.MinDist = Mathf.Min(lookData.MinDist, range * 0.5f);
            lookData.SprintDist = range;
        }

        private void ApplyHearingOptimization(BotOwner bot)
        {
            var mindSettings = bot.Settings?.FileSettings?.Mind;
            if (mindSettings == null)
                return;

            float hearing = GetHearingRange(bot);
            mindSettings.HearingSenseDistance = hearing;
        }

        private void ApplyAggressionTweaks(BotOwner bot)
        {
            var mindSettings = bot.Settings?.FileSettings?.Mind;
            if (mindSettings == null)
                return;

            float aggression = GetAggressionLevel(bot);
            mindSettings.AggressionReact = aggression;
        }

        private float GetVisionRange(BotOwner bot)
        {
            if (IsBoss(bot))
                return 200f;

            return 120f;
        }

        private float GetHearingRange(BotOwner bot)
        {
            return IsBoss(bot) ? 55f : 40f;
        }

        private float GetAggressionLevel(BotOwner bot)
        {
            var role = bot.Profile.Info.Settings.Role;

            if (role.IsBoss())
                return 0.9f;
            if (role.IsFollower())
                return 0.5f;

            return 0.65f;
        }

        public void ResetOptimization(BotOwner botOwner)
        {
            if (botOwner?.Profile == null)
                return;

            _optimizationApplied[botOwner.Profile.Id] = false;
        }

        private bool IsBoss(BotOwner bot)
        {
            var role = bot.Profile.Info.Settings.Role;
            return role.IsBoss();
        }
    }
}
