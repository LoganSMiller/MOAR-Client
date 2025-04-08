using System;

namespace MOAR.Helpers
{
    public static class EnumUtils
    {
        public static T SafeParseEnum<T>(string value, T fallback) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
                return fallback;

            return Enum.TryParse<T>(value, ignoreCase: true, out var result) ? result : fallback;
        }
    }
}
