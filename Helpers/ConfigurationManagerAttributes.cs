using System;
using BepInEx.Configuration;

namespace MOAR.Helpers
{
    /// <summary>
    /// Metadata container for enhancing BepInEx ConfigurationManager UI behavior.
    /// Allows customization of editors, display labels, tooltips, sorting, and more.
    /// </summary>
    internal sealed class ConfigurationManagerAttributes
    {
        /// <summary>
        /// If true, numeric fields with min/max will render as percentage sliders.
        /// </summary>
        public bool? ShowRangeAsPercent { get; set; }

        /// <summary>
        /// Delegate to draw a custom UI element for this config entry.
        /// </summary>
        public Action<ConfigEntryBase>? CustomDrawer { get; set; }

        /// <summary>
        /// Delegate to draw a custom UI element for hotkey configuration.
        /// </summary>
        public CustomHotkeyDrawerFunc? CustomHotkeyDrawer { get; set; }

        /// <summary>
        /// Whether this setting should be shown in the configuration UI. Defaults to true.
        /// </summary>
        public bool? Browsable { get; set; }

        /// <summary>
        /// Optional grouping category this entry belongs to in the UI.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// The value to restore if the user clicks "Reset".
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// If true, the reset-to-default button is hidden.
        /// </summary>
        public bool? HideDefaultButton { get; set; }

        /// <summary>
        /// If true, the name label of the config entry is hidden.
        /// </summary>
        public bool? HideSettingName { get; set; }

        /// <summary>
        /// The tooltip text shown on hover.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The custom label shown in place of the config entry name.
        /// </summary>
        public string? DispName { get; set; }

        /// <summary>
        /// Determines the ordering of this field in the config UI (lower = first).
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// If true, this field cannot be edited via the UI.
        /// </summary>
        public bool? ReadOnly { get; set; }

        /// <summary>
        /// If true, this field is only visible when advanced mode is enabled.
        /// </summary>
        public bool? IsAdvanced { get; set; }

        /// <summary>
        /// Optional function to convert an object to a string for display.
        /// </summary>
        public Func<object, string>? ObjToStr { get; set; }

        /// <summary>
        /// Optional function to convert a string back into an object for storage.
        /// </summary>
        public Func<string, object>? StrToObj { get; set; }

        /// <summary>
        /// Signature for defining custom rendering behavior for hotkey config fields.
        /// </summary>
        /// <param name="setting">The config entry being edited.</param>
        /// <param name="isCurrentlyAcceptingInput">Whether input is currently being captured for this field.</param>
        public delegate void CustomHotkeyDrawerFunc(ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);
    }
}
