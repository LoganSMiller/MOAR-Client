using EFT;
using UnityEngine;
using MOAR.Helpers;
using System.Diagnostics;

namespace MOAR.AI
{
    /// <summary>
    /// Triggers temporary panic behavior when bots are exposed to intense light.
    /// </summary>
    public class BotFlashReactionComponent : MonoBehaviour
    {
        private Player bot = null!;
        private BotOwner? owner;

        private void Awake()
        {
            bot = GetComponent<Player>();
            owner = BotSuppressionHelper.GetBotOwner(bot);
        }

        /// <summary>
        /// Triggers a flash-based panic response using the panic controller.
        /// </summary>
        public void TriggerFlashReaction(float intensity, string source)
        {
            if (owner == null || bot == null)
                return;

            if (BotPanicUtility.TryGet(bot, out var panic))
            {
                panic.TriggerPanic("Flash", bot.Transform.position);
                LogDebug($"Flash reaction triggered → Source={source}, Intensity={intensity:F2}");
            }
        }

        [Conditional("DEBUG"), Conditional("UNITY_EDITOR")]
        private void LogDebug(string msg)
        {
            UnityEngine.Debug.Log($"[RLO] {msg}");
        }
    }
}
