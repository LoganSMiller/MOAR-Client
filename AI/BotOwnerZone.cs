using UnityEngine;
using EFT;

namespace MOAR.AI
{
    /// <summary>
    /// Tracks the current zone or area the bot is in for logic like fallback, group cohesion, or hotspot targeting.
    /// </summary>
    public class BotOwnerZone : MonoBehaviour
    {
        public BotOwner Bot { get; private set; }

        /// <summary>
        /// Name or identifier of the current zone the bot is considered to be in.
        /// </summary>
        public string CurrentZone { get; private set; } = "unknown";

        /// <summary>
        /// The last fallback zone the bot transitioned from.
        /// </summary>
        public string LastFallbackZone { get; private set; } = "none";

        /// <summary>
        /// If true, the bot is moving between zones.
        /// </summary>
        public bool IsTransitingZones { get; private set; } = false;

        void Awake()
        {
            Bot = GetComponent<BotOwner>();
        }

        /// <summary>
        /// Assign the bot to a new zone and mark as transiting.
        /// </summary>
        public void AssignZone(string zoneName)
        {
            if (!string.IsNullOrEmpty(zoneName) && zoneName != CurrentZone)
            {
                LastFallbackZone = CurrentZone;
                CurrentZone = zoneName;
                IsTransitingZones = true;

                Debug.Log($"[MOAR] Bot {Bot?.Profile?.Info?.Nickname} assigned to zone: {zoneName}");
            }
        }

        /// <summary>
        /// Call this once the bot arrives in its assigned zone.
        /// </summary>
        public void ConfirmZoneArrival()
        {
            IsTransitingZones = false;
        }

        /// <summary>
        /// Clear the current zone assignment.
        /// </summary>
        public void ResetZone()
        {
            CurrentZone = "unknown";
            IsTransitingZones = false;
        }
    }
}
