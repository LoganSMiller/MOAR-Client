using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace MOAR.Helpers
{
    /// <summary>
    /// Utility extensions for keyboard shortcut evaluation with full modifier awareness.
    /// </summary>
    public static class UIUtils
    {
        /// <summary>
        /// Determines whether the main key and all modifiers are currently held.
        /// </summary>
        /// <param name="shortcut">The keyboard shortcut configuration.</param>
        /// <returns>True if the shortcut is actively being held.</returns>
        public static bool BetterIsPressed(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None &&
                   Input.GetKey(shortcut.MainKey) &&
                   AreModifiersHeld(shortcut);
        }

        /// <summary>
        /// Determines whether the main key and all modifiers were just pressed this frame.
        /// </summary>
        /// <param name="shortcut">The keyboard shortcut configuration.</param>
        /// <returns>True if the shortcut was just pressed.</returns>
        public static bool BetterIsDown(this KeyboardShortcut shortcut)
        {
            return shortcut.MainKey != KeyCode.None &&
                   Input.GetKeyDown(shortcut.MainKey) &&
                   AreModifiersHeld(shortcut);
        }

        /// <summary>
        /// Checks whether all required modifier keys for a shortcut are currently held.
        /// </summary>
        /// <param name="shortcut">The keyboard shortcut to evaluate.</param>
        /// <returns>True if no modifiers are defined or all are held.</returns>
        private static bool AreModifiersHeld(KeyboardShortcut shortcut)
        {
            return shortcut.Modifiers == null || !shortcut.Modifiers.Any() || shortcut.Modifiers.All(Input.GetKey);
        }
    }
}
