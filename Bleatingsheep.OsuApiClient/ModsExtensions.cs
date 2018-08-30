using System;
using System.Collections.Generic;
using System.Linq;

namespace Bleatingsheep.OsuMixedApi
{
    public static class ModsExtensions
    {
        private static readonly IReadOnlyDictionary<string, Mods> pairs = new Dictionary<string, Mods>
        {
            //{ "NONE", Mods.None },
            { "NF", Mods.NoFail },
            { "EZ", Mods.Easy },
            { "HD", Mods.Hidden },
            { "HR", Mods.HardRock },
            { "SD", Mods.SuddenDeath },
            { "DT", Mods.DoubleTime },
            // Relax
            { "HT", Mods.HalfTime },
            { "NC", Mods.Nightcore },
            { "FL", Mods.Flashlight },
            { "PF", Mods.Perfect },
        };

        private static readonly IReadOnlyDictionary<Mods, Mods> require = new Dictionary<Mods, Mods>
        {
            { Mods.Nightcore, Mods.DoubleTime },
            { Mods.Perfect, Mods.SuddenDeath },
        };

        private static IEnumerable<string> Split(string modString)
        {
            if (modString.Length % 2 != 0)
                throw new ArgumentException("String argument is invalid.", nameof(modString));
            for (int i = 0; i < modString.Length; i += 2)
            {
                yield return modString.Substring(i, 2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modString"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public static Mods Parse(string modString)
        {
            if (modString == null)
            {
                throw new ArgumentNullException(nameof(modString));
            }

            modString = modString.ToUpperInvariant();

            if (modString == string.Empty || modString == "NONE")
            {
                return Mods.None;
            }

            Mods result = Mods.None;

            foreach (string mod in Split(modString))
            {
                if (!pairs.TryGetValue(mod, out Mods current))
                    throw new ArgumentException("String argument is invalid.", nameof(modString));

                current |= require.GetValueOrDefault(current);

                if ((current & result) != 0)
                    throw new ArgumentException("Repeated mods were found.");

                result |= current;
            }

            return result;
        }

        /// <summary>
        /// Get the display string of mods.
        /// </summary>
        /// <param name="mods"></param>
        /// <returns></returns>
        public static string Display(this Mods mods)
        {
            foreach (Mods mod in require.Where(r => (r.Key & mods) != 0).Select(r => r.Value))
            {
                mods &= ~mod;
            }

            bool needSp = false;
            string result = string.Empty;

            foreach (var (mod, description) in pairs.Where(p => mods.HasFlag(p.Value)).Select(p => (p.Value, p.Key)).ToArray())
            {
                needSp = true;
                result += description;
                mods &= ~mod;
            }

            if (mods != 0)
            {
                if (needSp)
                    result += ", ";

                result += mods.ToString();
            }

            return result;
        }
    }
}
