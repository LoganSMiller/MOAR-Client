// --------------------------------------------------
// BotSuppressionHelper.cs (Panic Synced)
// --------------------------------------------------

using EFT;
using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using MOAR.AI;


using static UnityEngine.Object;

namespace MOAR.Helpers
{
    /// <summary>
    /// Provides helper methods for bot suppression behavior, AI state access, and runtime system validation.
    /// </summary>
    public static class BotSuppressionHelper
    {
        private static MethodInfo? _setUnderFireMethod;

        #region Bot Accessors

        /// <summary>
        /// Attempts to retrieve the BotOwner from a Player AI instance.
        /// </summary>
        public static BotOwner? GetBotOwner(Player bot)
        {
            return bot.IsAI && bot.AIData is BotOwner owner ? owner : null;
        }

        #endregion

        #region Suppression Logic

        /// <summary>
        /// Triggers the internal ShootData.SetUnderFire() method via reflection.
        /// </summary>
        public static void TrySetUnderFire(BotOwner owner)
        {
            if (owner?.ShootData == null) return;

            _setUnderFireMethod ??= owner.ShootData.GetType()
                .GetMethod("SetUnderFire", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            _setUnderFireMethod?.Invoke(owner.ShootData, null);
        }

        /// <summary>
        /// Attempts to trigger suppression behavior using panic controller or fallback component.
        /// </summary>
        public static void TrySuppressBot(Player bot, Vector3 source)
        {
            if (bot == null || !bot.IsAI || bot.AIData == null) return;

            if (BotPanicUtility.TryGet(bot, out var panic))
            {
                panic.TriggerPanic("Suppression", source);
            }
            else if (bot.TryGetComponent(out BotSuppressionReactionComponent suppression))
            {
                suppression.OnSuppressed(source);
            }
        }

        /// <summary>
        /// Determines if suppression should occur based on visual distance or ambient darkness.
        /// </summary>
        public static bool ShouldTriggerSuppression(Player bot, float visibilityThreshold = 12f, float ambientThreshold = 0.2f)
        {
            var owner = GetBotOwner(bot);
            if (owner?.LookSensor == null) return false;

            float visibleDist = owner.LookSensor.ClearVisibleDist;
            float ambient = RenderSettings.ambientLight.grayscale;

            return visibleDist < visibilityThreshold || ambient < ambientThreshold;
        }

        #endregion

        #region Runtime System Bootstrapping

        /// <summary>
        /// Ensures the LightingSystemBootstrapper is loaded into the scene for runtime triggers.
        /// </summary>
        public static void EnsureLightingBootstrapExists()
        {
            const string objName = "RLO_LightingBootstrap";
            if (GameObject.Find(objName) != null) return;

            var go = new GameObject(objName);
            go.AddComponent<LightingSystemBootstrapper>();
            DontDestroyOnLoad(go);

            LogDebug("LightingSystemBootstrapper initialized.");
        }

        #endregion

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEBUG")]
        private static void LogDebug(string msg) =>
            UnityEngine.Debug.Log($"[RLO] {msg}");
    }
}
