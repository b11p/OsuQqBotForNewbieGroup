using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Mahjong;

#nullable enable
public class MajsoulDanPTProvider
{
    private static readonly Dictionary<string, double[]> ExtraDanPT4 = new Dictionary<string, double[]>
    {
        {"王座の間南喰赤", new double[] { 120, 60, 0 }},
        {"王座の間東喰赤", new double[] { 60, 30, 0 }},
        {"玉の間南喰赤", new double[] { 110, 55, 0 }},
        {"玉の間東喰赤", new double[] { 55, 30, 0 }},
        {"金の間南喰赤", new double[] { 80, 40, 0 }},
        {"金の間東喰赤", new double[] { 40, 20, 0 }},
        {"銀の間南喰赤", new double[] { 40, 20, 0 }},
        {"銀の間東喰赤", new double[] { 20, 10, 0 }},
        {"銅の間南喰赤", new double[] { 20, 10, 0 }},
        {"銅の間東喰赤", new double[] { 10, 5, 0 }},
    };

    private static readonly Dictionary<string, double> LastPlacePT4E = new Dictionary<string, double>
    {
        {"雀聖★3", 130},
        {"雀聖★2", 120},
        {"雀聖★1", 110},
        {"雀豪★3", 100},
        {"雀豪★2", 90},
        {"雀豪★1", 80},
        {"雀傑★3", 60},
        {"雀傑★2", 50},
        {"雀傑★1", 40},
        {"雀士★3", 30},
        {"雀士★2", 20},
        {"雀士★1", 10},
        {"初心★3", 0},
        {"初心★2", 0},
        {"初心★1", 0},
    };

    private static readonly Dictionary<string, double> LastPlacePT4S = new Dictionary<string, double>
    {
        {"雀聖★3", 240},
        {"雀聖★2", 225},
        {"雀聖★1", 210},
        {"雀豪★3", 195},
        {"雀豪★2", 180},
        {"雀豪★1", 165},
        {"雀傑★3", 120},
        {"雀傑★2", 100},
        {"雀傑★1", 80},
        {"雀士★3", 60},
        {"雀士★2", 40},
        {"雀士★1", 20},
        {"初心★3", 0},
        {"初心★2", 0},
        {"初心★1", 0},
    };

    public double[] GetPTList(string dan, string roomLevel, int players = 4)
    {
        if (players != 4)
        {
            throw new NotSupportedException("Currently only supports 4 players.");
        }

        var result = new double[players];
        if (roomLevel.Contains("友人戦") || roomLevel.Contains("交流の間"))
        {
            result[0] = 100;
            return result;
        }

        var danPt = ExtraDanPT4.GetValueOrDefault(roomLevel);
        if (danPt == null)
        {
            throw new ArgumentException($"{roomLevel} is not a valid room level.");
        }
        danPt.CopyTo(result, 0);

        // madian
        result[0] += 15;
        result[1] += 5;
        result[2] += -5;
        result[3] += -15;

        // last place
        var fetchingDictionary =
            roomLevel.EndsWith("南喰赤")
            ? LastPlacePT4S
            : roomLevel.EndsWith("東喰赤")
                ? LastPlacePT4E
                : throw new ArgumentException($"{dan} is not a valid dan.");
        result[3] -= fetchingDictionary[dan];
        return result;
    }
}
#nullable restore