using System;
using BepInEx.Configuration;

namespace MOAR.Helpers
{
    /// <summary>
    /// Provides metadata used by BepInEx ConfigurationManager for customizing how config options are displayed.
    /// Includes support for hotkey rendering, tooltips, grouping, order, and other UI tweaks.
    /// </summary>
    internal sealed class ConfigurationManagerAttributes
    {
        /// <summary>
        /// Renders numerical values as percentage sliders if true.
        /// </summary>
        public bool? ShowRangeAsPercent { get; set; }

        /// <summary>
        /// Custom drawer delegate used to override UI behavior for this setting.
        /// </summary>
        public Action<ConfigEntryBase>? CustomDrawer { get; set; }

        /// <summary>
        /// Optional hotkey-specific custom drawer delegate.
        /// </summary>
        public CustomHotkeyDrawerFunc? CustomHotkeyDrawer { get; set; }

        /// <summary>
        /// Determines if this config entry is shown in the configuration UI.
        /// </summary>
        public bool? Browsable { get; set; }

        /// <summary>
        /// Optional category this entry belongs to (used for UI grouping).
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Default value to restore when "Reset" is clicked.
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Hides the reset-to-default button if true.
        /// </summary>
        public bool? HideDefaultButton { get; set; }

        /// <summary>
        /// Hides the setting's display name if true.
        /// </summary>
        public bool? HideSettingName { get; set; }

        /// <summary>
        /// Tooltip shown when the mouse hovers over this setting.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Custom display label shown instead of the default config entry name.
        /// </summary>
        public string? DispName { get; set; }

        /// <summary>
        /// Controls the order of appearance in the UI (lower appears first).
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// If true, this setting is shown as read-only in the UI.
        /// </summary>
        public bool? ReadOnly { get; set; }

        /// <summary>
        /// If true, the setting only appears in advanced mode.
        /// </summary>
        public bool? IsAdvanced { get; set; }

        /// <summary>
        /// Optional function for rendering object values as strings.
        /// </summary>
        public Func<object, string>? ObjToStr { get; set; }

        /// <summary>
        /// Optional function for parsing string values back to object values.
        /// </summary>
        public Func<string, object>? StrToObj { get; set; }

        /// <summary>
        /// Delegate for drawing a custom hotkey entry.
        /// </summary>
        /// <param name="setting">The config entry being drawn.</param>
        /// <param name="isCurrentlyAcceptingInput">Whether input capture is currently active for this field.</param>
        public delegate void CustomHotkeyDrawerFunc(ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);
    }
}
