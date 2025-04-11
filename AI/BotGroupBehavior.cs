using EFT;
using UnityEngine;

namespace MOAR.AI.Optimization
{
    /// <summary>
    /// Handles squad-level awareness: regrouping, group panic propagation, and spatial cohesion.
    /// </summary>
    public class BotGroupBehavior
    {
        private readonly BotOwner _bot;
        private readonly BotPerceptionSystem _perception;
        private readonly MOARBotOwner _moar;

        private float _nextUpdate;
        private const float UPDATE_INTERVAL = 1.0f;
        private const float MAX_BASE_SPREAD = 25f;

        public BotGroupBehavior(BotOwner bot, BotPerceptionSystem perception)
        {
            _bot = bot;
            _perception = perception;
            _moar = _bot?.GetPlayer?.GetComponent<MOARBotOwner>();
        }

        public void Update()
        {
            if (Time.time < _nextUpdate || _bot == null || _bot.IsDead || _bot.BotsGroup == null)
                return;

            _nextUpdate = Time.time + UPDATE_INTERVAL;

            HandleRegrouping();
            SharePanic();
        }

        private void HandleRegrouping()
        {
            if (_bot.BotsGroup.MembersCount <= 1 || _moar?.PersonalityProfile == null)
                return;

            Vector3 center = _bot.Position;
            int count = 1;

            for (int i = 0; i < _bot.BotsGroup.MembersCount; i++)
            {
                BotOwner other = _bot.BotsGroup.Member(i);
                if (other != null && other != _bot)
                {
                    center += other.Position;
                    count++;
                }
            }

            center /= count;

            float cohesion = Mathf.Clamp01(_moar.PersonalityProfile.Cohesion);
            float maxSpread = Mathf.Lerp(35f, 10f, cohesion); // High cohesion = tighter grouping

            float distance = Vector3.Distance(_bot.Position, center);
            if (distance > maxSpread && !_perception.IsBlinded)
            {
                _bot.GoToPoint(center, false);
            }
        }

        private void SharePanic()
        {
            if (!_perception.ShouldPanic)
                return;

            _bot.BotsGroup?.AddPointToSearch(_bot.Position, 100f, _bot);

            // Optionally: force allies near this bot to react (fallback, seek cover, etc.)
            for (int i = 0; i < _bot.BotsGroup.MembersCount; i++)
            {
                BotOwner member = _bot.BotsGroup.Member(i);
                if (member != null && member != _bot && Vector3.Distance(member.Position, _bot.Position) < 15f)
                {
                    member.Memory?.SetPanicPoint(new PlaceForCheck(_bot.Position, PlaceForCheckType.danger), true);
                }
            }
        }
    }
}
