using System;
using System.Collections.Generic;
using System.Linq;

namespace Bleatingsheep.OsuMixedApi
{
    [Flags]
    public enum Mods
    {
        None = 0,
        NoFail = 1,
        Easy = 2,
        NoVideo = 4, // Not used anymore, but can be found on old plays like Mesita on b/78239
        Hidden = 8,
        HardRock = 16,
        SuddenDeath = 32,
        DoubleTime = 64,
        Relax = 128,
        HalfTime = 256,
        Nightcore = 512, // Only set along with DoubleTime. i.e: NC only gives 576
        Flashlight = 1024,
        Autoplay = 2048,
        SpunOut = 4096,
        Relax2 = 8192,  // Autopilot?
        Perfect = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
        Key4 = 32768,
        Key5 = 65536,
        Key6 = 131072,
        Key7 = 262144,
        Key8 = 524288,
        keyMod = Key4 | Key5 | Key6 | Key7 | Key8,
        FadeIn = 1048576,
        Random = 2097152,
        LastMod = 4194304,
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | keyMod,
        Key9 = 16777216,
        Key10 = 33554432,
        Key1 = 67108864,
        Key3 = 134217728,
        Key2 = 268435456,
        ScoreV2 = 536870912,
    }

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
            if (modString.Length % 2 != 0) throw new ArgumentException("String argument is invalid.", nameof(modString));
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
                if (!pairs.TryGetValue(mod, out Mods current)) throw new ArgumentException("String argument is invalid.", nameof(modString));

                current |= require.GetValueOrDefault(current);

                if ((current & result) != 0) throw new ArgumentException("Repeated mods were found.");

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
                if (needSp) result += ", ";

                result += mods.ToString();
            }

            return result;
        }
    }
}
