using System;
using BepInEx.Configuration;

namespace MOAR.Helpers
{
    /// <summary>
    /// Custom UI metadata for BepInEx ConfigurationManager integration.
    /// Enables enhanced behavior like sliders, hotkey fields, tooltips, and advanced UI overrides.
    /// </summary>
    internal sealed class ConfigurationManagerAttributes
    {
        /// <summary>
        /// If true, numeric values are shown as percentage sliders.
        /// </summary>
        public bool? ShowRangeAsPercent { get; set; }

        /// <summary>
        /// Custom UI drawing method for general config entries.
        /// </summary>
        public Action<ConfigEntryBase> CustomDrawer { get; set; }

        /// <summary>
        /// Delegate for custom hotkey input UI rendering.
        /// </summary>
        public CustomHotkeyDrawerFunc CustomHotkeyDrawer { get; set; }

        /// <summary>
        /// If false, hides the config entry from the UI entirely.
        /// </summary>
        public bool? Browsable { get; set; }

        /// <summary>
        /// Optional category name for grouping this entry in the UI.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Optional value to override the default shown in the UI.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// If true, hides the "reset to default" button.
        /// </summary>
        public bool? HideDefaultButton { get; set; }

        /// <summary>
        /// If true, hides the display name label of this setting.
        /// </summary>
        public bool? HideSettingName { get; set; }

        /// <summary>
        /// Tooltip description shown in the configuration UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Custom label shown instead of the setting name.
        /// </summary>
        public string DispName { get; set; }

        /// <summary>
        /// Display order priority in the configuration UI.
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// If true, the config entry is read-only and cannot be changed via the UI.
        /// </summary>
        public bool? ReadOnly { get; set; }

        /// <summary>
        /// If true, this field appears only when advanced mode is enabled.
        /// </summary>
        public bool? IsAdvanced { get; set; }

        /// <summary>
        /// Function to convert the setting value to a UI string.
        /// </summary>
        public Func<object, string> ObjToStr { get; set; }

        /// <summary>
        /// Function to convert a UI string back into the setting object.
        /// </summary>
        public Func<string, object> StrToObj { get; set; }

        /// <summary>
        /// Delegate type for custom hotkey drawing methods.
        /// </summary>
        /// <param name="setting">The config entry to draw.</param>
        /// <param name="isCurrentlyAcceptingInput">Whether the input is currently focused for key capture.</param>
        public delegate void CustomHotkeyDrawerFunc(ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);
    }
}
