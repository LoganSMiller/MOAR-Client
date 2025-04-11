// ----------------------------------
// IFlashReactiveBot.cs
// ----------------------------------
using UnityEngine;

namespace MOAR.AI
{
    /// <summary>
    /// Interface for bots that can respond to bright lighting such as flashlights or muzzle flares.
    /// </summary>
    public interface IFlashReactiveBot
    {
        void OnFlashExposure(Vector3 lightOrigin);
    }
}
