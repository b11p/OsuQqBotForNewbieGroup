using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
#nullable enable
    public partial class 称金币
    {
        private class ExecuteInfo
        {
            public const int Max = 8;

            private List<(int heavy, int light)> Known { get; } = new List<(int, int)>();
            private readonly List<string> _knownForHints = new List<string>();
            public ReadOnlyCollection<string> KnownForHints => _knownForHints.ToList().AsReadOnly();
            public int WeighingCount => Known.Count;

            private IReadOnlyCollection<IList<int>> Potentials { get; set; } = Enumerable.Empty<IList<int>>().ToList();

            public ExecuteInfo()
            {
                AddCompare(1, 2, GetRandomBool(), false);
                InitPotantials();
            }

            private static bool GetRandomBool() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 2 == 0;

            private void AddCompare(int left, int right, bool leftIsHeavier, bool duplicated)
            {
                if (!duplicated)
                    Known.Add(leftIsHeavier ? (left, right) : (right, left));
                _knownForHints.Add($"{left} {(leftIsHeavier ? "重过" : "轻于")} {right}" + (duplicated ? " (重复)" : ""));
            }

            private void InitPotantials()
            {
                var current = Enumerable.Range(1, Max).Select(i => new[] { i } as IList<int>).ToList();
                for (int i = 1; i < Max; i++)
                {
                    current =
                        (from k in current
                         from j in Enumerable.Range(1, Max)
                         where !k.Contains(j) && !k.Any(present => Known.Contains((present, j)))
                         select (k.Append(j).ToList()))
                         .ToList<IList<int>>();
                }
                Potentials = current;
            }

            private (int, int) GetPotentialsCount(int left, int right)
            {
                Span<int> potentials = stackalloc int[2] { 0, 0 };

                var scenarios = Potentials;
                foreach (var item in scenarios)
                {
                    //var weightIndex = item.Select((w, i) => (index: i, weight: w));
                    if (item.IndexOf(left) > item.IndexOf(right))
                    {
                        // left is heavier
                        potentials[0]++;
                    }
                    else if (item.IndexOf(left) < item.IndexOf(right))
                    {
                        // right is heavier
                        potentials[1]++;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

                return (potentials[0], potentials[1]);
            }

            public IList<string> GetKnown(out bool isClear)
            {
                lock (Known)
                {
                    isClear = Potentials.Count <= 1;
                    return KnownForHints;
                }
            }

            public IList<string> Weigh(int left, int right, out bool isClear, out bool isKnown, out IList<int>? result)
            {
                lock (Known)
                {
                    var (hl, hr) = GetPotentialsCount(left, right);
                    if (hl == 0 || hr == 0)
                    {
                        isKnown = true;
                        isClear = false;
                        result = null;
                        AddCompare(left, right, hr == 0, true);
                        return KnownForHints;
                    }
                    bool leftHeavy = (hl - hr) switch
                    {
                        var i when i > 0 => true,
                        var i when i < 0 => false,
                        0 => GetRandomBool(),
                        _ => GetRandomBool(),
                    };
                    AddCompare(left, right, leftHeavy, false);
                    //Known.Add(leftHeavy ? (left, right) : (right, left));
                    isKnown = false;
                    Potentials = Potentials.Where(potential =>
                    {
                        Span<int> weights = stackalloc int[Max];
                        for (int i = 0; i < Max; i++)
                        {
                            weights[potential[i] - 1] = i + 1;
                        }
                        foreach (var (heavy, light) in Known)
                        {
                            if (weights[heavy - 1] < weights[light - 1])
                                return false;
                        }
                        return true;
                    }).ToList();
                    isClear = Potentials.Count <= 1;
                    result = isClear switch
                    {
                        true => Potentials.FirstOrDefault()?.ToList(),
                        false => null,
                    };
                    return KnownForHints;
                }
            }
        }
    }
#nullable restore
}
