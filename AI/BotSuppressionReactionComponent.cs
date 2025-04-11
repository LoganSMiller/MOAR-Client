using UnityEngine;
using EFT;

namespace MOAR.AI
{
    /// <summary>
    /// Simulates bot suppression reaction, such as flinching or evasive response under fire.
    /// May later connect to external damage, threat, or sound detection systems.
    /// </summary>
    public class BotSuppressionReactionComponent : MonoBehaviour
    {
        public BotOwner Bot { get; private set; }

        private float suppressionStartTime;
        private const float suppressionDuration = 2.0f;

        private bool isSuppressed = false;

        void Awake()
        {
            Bot = GetComponent<BotOwner>();
        }

        void Update()
        {
            if (isSuppressed && Time.time - suppressionStartTime > suppressionDuration)
            {
                isSuppressed = false;
            }
        }

        public void TriggerSuppression()
        {
            suppressionStartTime = Time.time;
            isSuppressed = true;

            // Optional: trigger movement or animation
            Debug.Log($"[MOAR] Bot {Bot?.Profile?.Info?.Nickname} is reacting to suppression.");
        }

        public bool IsSuppressed()
        {
            return isSuppressed;
        }
    }
}
