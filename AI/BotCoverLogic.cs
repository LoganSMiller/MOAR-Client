using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

namespace MOAR.AI
{
    /// <summary>
    /// Controls bot behavior under suppressive fire and panic, including cover-seeking and fallback logic.
    /// </summary>
    public class BotCoverLogic
    {
        private readonly BotOwner _botOwner;
        private readonly MOARBotOwner _moarBot;

        private float _lastCheckTime;
        private const float CHECK_INTERVAL = 1.5f;
        private const float SUPPRESSION_RADIUS = 6.5f;
        private const float COVER_SCAN_DISTANCE = 5f;

        public BotCoverLogic(BotOwner botOwner)
        {
            _botOwner = botOwner;
            _moarBot = botOwner?.GetPlayer?.GetComponent<MOARBotOwner>();
        }

        public void Update()
        {
            if (_botOwner == null || _botOwner.IsDead || _botOwner.AIData == null || _moarBot?.PersonalityProfile == null)
                return;

            if (Time.time - _lastCheckTime < CHECK_INTERVAL)
                return;

            _lastCheckTime = Time.time;

            if (!IsUnderSuppression(out Vector3 suppressDir) && !IsPanicTriggered())
                return;

            TriggerSuppressionEffects();

            if (!TryMoveToCover(suppressDir))
            {
                _botOwner?.Memory?.Spotted(true, null, null); // No cover found, alert stance
            }
        }

        /// <summary>
        /// Determines if bot is being suppressed.
        /// </summary>
        private bool IsUnderSuppression(out Vector3 suppressDirection)
        {
            suppressDirection = Vector3.zero;

            if (_botOwner?.Memory?.GoalEnemy is not EnemyInfo enemy)
                return false;

            if (!enemy.IsVisible || !enemy.CanShoot)
                return false;

            float distance = Vector3.Distance(_botOwner.Position, enemy.CurrPosition);
            if (distance < SUPPRESSION_RADIUS)
            {
                suppressDirection = (_botOwner.Position - enemy.CurrPosition).normalized;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Uses health and suppression to determine panic fallback state.
        /// </summary>
        private bool IsPanicTriggered()
        {
            if (!_botOwner.Memory.IsUnderFire) return false;

            var hc = _botOwner.GetPlayer?.HealthController;
            if (hc == null) return false;

            float current = 0f;
            float max = 0f;
            foreach (EBodyPart part in Enum.GetValues(typeof(EBodyPart)))
            {
                var hp = hc.GetBodyPartHealth(part);
                current += hp.Current;
                max += hp.Maximum;
            }

            float healthRatio = Mathf.Clamp01(current / max);
            return healthRatio < _moarBot.PersonalityProfile.RetreatThreshold;
        }

        /// <summary>
        /// Sends suppression alert to bot brain.
        /// </summary>
        private void TriggerSuppressionEffects()
        {
            _botOwner.Memory?.Spotted(true, null, null);
            
        }

        /// <summary>
        /// Attempts to reposition the bot behind cover relative to threat.
        /// </summary>
        private bool TryMoveToCover(Vector3 awayFromThreat)
        {
            Vector3 lateral = Vector3.Cross(Vector3.up, awayFromThreat).normalized * UnityEngine.Random.Range(-1.5f, 1.5f);
            Vector3 offset = awayFromThreat.normalized * COVER_SCAN_DISTANCE + lateral;
            Vector3 fallback = _botOwner.Position + offset;

            if (NavMesh.SamplePosition(fallback, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                var nav = new NavPoint(0, hit.position);
                var groupPoint = new GroupPoint(
                    0,
                    nav,
                    hit.position,
                    null,
                    null,
                    (CoverLevel)0,
                    false,
                    Vector3.up,
                    Vector3.forward,
                    (PointWithNeighborType)0
                );
                var navPoint = new CustomNavigationPoint(groupPoint);
                _botOwner.Mover?.GoToPoint(navPoint, false);
                return true;
            }

            return false;
        }
    }
}
