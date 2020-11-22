using System;

namespace Bleatingsheep.NewHydrant.Utilities
{
    public static class IncrementUtility
    {
        public static string FormatIncrement(double? increment, string format = "####.##")
            => FormatIncrement(increment, $"+{format};;+", $"-{format};;-", string.Empty);

        public static string FormatIncrement(double? increment, char incrementPrefix, char decrementPrefix, string format = "####.##")
            => FormatIncrement(increment, $"{incrementPrefix}{format};;{incrementPrefix}", $"{decrementPrefix}{format};;{decrementPrefix}", string.Empty);

        private static string FormatIncrement(double? increment, string incrementFormat, string decrementFormat, string invarientDisplay = "")
        {
            var v = (increment ?? 0) switch
            {
                > 0 => increment?.ToString(incrementFormat),
                < 0 => (-increment)?.ToString(decrementFormat),
                0 => invarientDisplay,
                double.NaN => throw new ArgumentException("Increment must not be double.NaN.", nameof(increment)),
            };
            if (!string.IsNullOrEmpty(v))
            {
                v = $" ({v})";
            }
            return v;
        }

        public static string FormatIncrementPercentage(double? increment)
            => FormatIncrement(increment, "#.##%");
    }
}
