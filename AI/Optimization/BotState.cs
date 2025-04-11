using System;
using EFT;

namespace MOAR.AI.Optimization
{
    public class BotOwnerState
    {
        public float Aggression { get; }
        public float PerceptionRange { get; }

        public BotOwnerState(BotOwner botOwner)
        {
            if (botOwner?.Settings?.FileSettings?.Mind == null)
            {
                Aggression = 0f;
                PerceptionRange = 0f;
                return;
            }

            // Closest valid aggression reference
            Aggression = botOwner.Settings.FileSettings.Mind.MIN_START_AGGRESION_COEF;

            // Using DIST_TO_ENEMY_SPOTTED_ON_HIT as fallback "sight" range value
            PerceptionRange = botOwner.Settings.FileSettings.Mind.DIST_TO_ENEMY_SPOTTED_ON_HIT;
        }

        public override bool Equals(object obj)
        {
            return obj is BotOwnerState other &&
                   Math.Abs(Aggression - other.Aggression) < 0.01f &&
                   Math.Abs(PerceptionRange - other.PerceptionRange) < 0.1f;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Aggression.GetHashCode();
                hash = hash * 23 + PerceptionRange.GetHashCode();
                return hash;
            }
        }
    }
}
