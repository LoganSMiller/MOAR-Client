using System.Collections.Generic;
using UnityEngine;

namespace MOAR.AI
{
    /// <summary>
    /// Tracks all active flashlight sources in the scene for bots to react to.
    /// </summary>
    public static class FlashlightRegistry
    {
        private static readonly List<Light> _activeLights = new();

        /// <summary>
        /// Returns a list of valid active flashlights in the scene.
        /// </summary>
        public static List<Light> GetActiveFlashlights()
        {
            _activeLights.Clear();

            foreach (var light in GameObject.FindObjectsOfType<Light>())
            {
                if (light.enabled && light.type == LightType.Spot && light.intensity >= 1.5f)
                {
                    _activeLights.Add(light);
                }
            }

            return _activeLights;
        }
    }
}
