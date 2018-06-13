using System;
using System.Collections.Generic;
using System.Linq;

namespace Bleatingsheep.OsuMixedApi
{
    public static class ModeExtensions
    {
        private const string ModeInfo = @"0,std,osu,osu!,standard
1,taiko,osu!taiko
2,ctb,catch,osu!catch
3,mania,osu!mania";

        private static readonly IReadOnlyDictionary<string, Mode> pairs;

        static ModeExtensions()
        {
            void ConcatLine(string line, Mode mode, ref IEnumerable<KeyValuePair<string, Mode>> toAdd)
            {
                var alias = line.Split(',').Select(s => new KeyValuePair<string, Mode>(s, mode));
                toAdd = toAdd.Concat(alias);
            }

            IEnumerable<KeyValuePair<string, Mode>> maps = new Dictionary<string, Mode>();

            var aliases = ModeInfo.ToUpperInvariant().Split("\r\n");

            for (int i = 0; i < aliases.Length; i++)
            {
                ConcatLine(aliases[i], (Mode)i, ref maps);
            }

            pairs = new Dictionary<string, Mode>(maps);
        }

        /// <summary>
        /// Comvert a <see cref="string"/> to <see cref="Mode"/>.
        /// </summary>
        /// <param name="s">A string containing a mode to convert.</param>
        /// <exception cref="ArgumentNullException"><c>s</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><c>s</c> is not a valid mode string.</exception>
        /// <returns></returns>
        public static Mode Parse(string s)
        {
            s = s.ToUpperInvariant();
            return pairs.TryGetValue(s, out Mode result) ? result : throw new ArgumentException("Invalid mode string.", nameof(s));
        }

        public static string GetShortModeString(this Mode mode)
        {
            switch (mode)
            {
                case Mode.Standard:
                    return "osu!";
                case Mode.Taiko:
                    return "taiko";
                case Mode.Ctb:
                    return "catch";
                case Mode.Mania:
                    return "mania";
                default:
                    return null;
            }
        }
    }
}
