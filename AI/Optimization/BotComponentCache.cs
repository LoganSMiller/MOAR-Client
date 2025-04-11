using UnityEngine;
using EFT;

namespace MOAR.AI
{
    /// <summary>
    /// Caches all MOAR AI components and helpers for a bot.
    /// Automatically attached and populated at bot initialization.
    /// </summary>
    public class BotComponentCache : MonoBehaviour
    {
        public BotOwner Bot { get; set; }

        public FlashGrenadeComponent FlashGrenade { get; set; }

        public BotPanicHandler PanicHandler { get; set; }

        public BotSuppressionReactionComponent Suppression { get; set; }

        public BotOwnerZone Zone { get; set; }

        /// <summary>
        /// Optional: Whether this cache has been fully initialized
        /// </summary>
        public bool IsReady =>
            Bot != null &&
            FlashGrenade != null &&
            PanicHandler != null;
    }
}
