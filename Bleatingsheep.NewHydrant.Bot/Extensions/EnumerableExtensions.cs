using System;
using System.Collections.Generic;
using System.Linq;

namespace Bleatingsheep.NewHydrant.Extentions
{
    static class EnumerableExtensions
    {
        private static readonly object s_randomLock = new object();
        private static readonly Random s_random = new Random();

        public static List<T> Randomize<T>(this IEnumerable<T> source)
        {
            lock (s_randomLock)
            {
                var result = source.ToList();
                // i 是 Count 到 2
                for (int i = result.Count - 1; i > 0; i--)
                {
                    var swap = s_random.Next(i + 1);
                    if (i != swap)
                    {
                        var temp = result[i];
                        result[i] = result[swap];
                        result[swap] = temp;
                    }
                }
                return result;
            }
        }
    }
}
