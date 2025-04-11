using UnityEngine;
using EFT;

namespace MOAR.AI
{
    /// <summary>
    /// Static helper methods for accessing common bot AI components from cache.
    /// </summary>
    public static class BotCacheUtility
    {
        public static bool TryGet<T>(BotComponentCache cache, out T result) where T : MonoBehaviour
        {
            result = null;
            if (cache == null)
                return false;

            result = cache.Bot?.GetComponent<T>();
            return result != null;
        }

        public static bool TryGetPanicComponent(BotComponentCache cache, out BotPanicHandler result)
        {
            result = null;
            if (cache == null)
                return false;

            result = cache.PanicHandler;
            return result != null;
        }

        public static Transform Head(BotComponentCache cache)
        {
            if (cache?.Bot?.gameObject == null)
                return null;

            var transforms = cache.Bot.gameObject.GetComponentsInChildren<Transform>();

            foreach (var t in transforms)
            {
                if (t.name.ToLower().Contains("head"))
                    return t;
            }

            return null;
        }

        public static Transform Look(BotComponentCache cache)
        {
            return cache?.Bot?.LookSensor?.PointOfViewTransform;
        }

        public static BotOwner Owner(BotComponentCache cache)
        {
            return cache?.Bot;
        }
    }
}
