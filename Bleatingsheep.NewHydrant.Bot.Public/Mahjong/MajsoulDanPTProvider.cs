using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bleatingsheep.NewHydrant.Mahjong;

#nullable enable
public class MajsoulDanPTProvider
{
    private static readonly Dictionary<string, int[]> ExtraDanPT4 = new Dictionary<string, int[]>
    {
        {"王座の間南喰赤", new int[] { 120, 60, 0 }},
        {"王座の間東喰赤", new int[] { 60, 30, 0 }},
        {"Throne Room South", new int[] { 120, 60, 0 }},
        {"Throne Room East", new int[] { 60, 30, 0 }},
        {"玉の間南喰赤", new int[] { 110, 55, 0 }},
        {"玉の間東喰赤", new int[] { 55, 30, 0 }},
        {"Jade Room South", new int[] { 110, 55, 0 }},
        {"Jade Room East", new int[] { 55, 30, 0 }},
        {"金の間南喰赤", new int[] { 80, 40, 0 }},
        {"金の間東喰赤", new int[] { 40, 20, 0 }},
        {"Gold Room South", new int[] { 80, 40, 0 }},
        {"Gold Room East", new int[] { 40, 20, 0 }},
        {"銀の間南喰赤", new int[] { 40, 20, 0 }},
        {"銀の間東喰赤", new int[] { 20, 10, 0 }},
        {"Silver Room South", new int[] { 40, 20, 0 }},
        {"Silver Room East", new int[] { 20, 10, 0 }},
        {"銅の間南喰赤", new int[] { 20, 10, 0 }},
        {"銅の間東喰赤", new int[] { 10, 5, 0 }},
        {"Bronze Room South", new int[] { 20, 10, 0 }},
        {"Bronze Room East", new int[] { 10, 5, 0 }},
    };

    private static readonly Dictionary<string, int> LastPlacePT4E = new Dictionary<string, int>
    {
        {"雀聖★3", 130},
        {"雀聖★2", 120},
        {"雀聖★1", 110},
        {"Saint III", 130},
        {"Saint II", 120},
        {"Saint I", 110},
        {"雀豪★3", 100},
        {"雀豪★2", 90},
        {"雀豪★1", 80},
        {"Master III", 100},
        {"Master II", 90},
        {"Master I", 80},
        {"雀傑★3", 60},
        {"雀傑★2", 50},
        {"雀傑★1", 40},
        {"Expert III", 60},
        {"Expert II", 50},
        {"Expert I", 40},
        {"雀士★3", 30},
        {"雀士★2", 20},
        {"雀士★1", 10},
        {"Adept III", 30},
        {"Adept II", 20},
        {"Adept I", 10},
        {"初心★3", 0},
        {"初心★2", 0},
        {"初心★1", 0},
        {"Novice III", 0},
        {"Novice II", 0},
        {"Novice I", 0},
    };

    private static readonly Dictionary<string, int> LastPlacePT4S = new Dictionary<string, int>
    {
        {"雀聖★3", 240},
        {"雀聖★2", 225},
        {"雀聖★1", 210},
        {"Saint III", 240},
        {"Saint II", 225},
        {"Saint I", 210},
        {"雀豪★3", 195},
        {"雀豪★2", 180},
        {"雀豪★1", 165},
        {"Master III", 195},
        {"Master II", 180},
        {"Master I", 165},
        {"雀傑★3", 120},
        {"雀傑★2", 100},
        {"雀傑★1", 80},
        {"Expert III", 120},
        {"Expert II", 100},
        {"Expert I", 80},
        {"雀士★3", 60},
        {"雀士★2", 40},
        {"雀士★1", 20},
        {"Adept III", 60},
        {"Adept II", 40},
        {"Adept I", 20},
        {"初心★3", 0},
        {"初心★2", 0},
        {"初心★1", 0},
        {"Novice III", 0},
        {"Novice II", 0},
        {"Novice I", 0},
    };

    public int[] GetPTList(string dan, string roomLevel, int players = 4)
    {
        if (players != 4)
        {
            throw new NotSupportedException("Currently only supports 4 players.");
        }

        var result = new int[players];
        if (roomLevel.Contains("友人戦") || roomLevel.Contains("交流の間") || roomLevel.Contains("Friendly"))
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
            roomLevel.EndsWith("南喰赤") || roomLevel.EndsWith("South")
            ? LastPlacePT4S
            : roomLevel.EndsWith("東喰赤") || roomLevel.EndsWith("East")
                ? LastPlacePT4E
                : throw new ArgumentException($"{dan} is not a valid dan.");
        result[3] -= fetchingDictionary[dan];
        return result;
    }
}
#nullable restore