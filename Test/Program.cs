using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database;
using Bleatingsheep.OsuQqBot.Database.Models;
using OsuQqBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void ApiTest()
        {
            string key = string.Empty;
            Console.WriteLine("input key");
            while (key == string.Empty)
                key = Console.ReadLine().Trim();
            var api = OsuApiClient.ClientUsingKey(key);
            //var beatmap = api.GetBeatmapsAsync(1441454).Result;

            //var recent = api.GetRecentlyAsync(6659067, Mode.Standard, 15).Result;
            //recent = api.GetRecentlyAsync("bleatingsheep", Mode.Standard, 17).Result;

            var best = api.GetBestPerformancesAsync(6659067, Mode.Taiko, 15).Result;
            //foreach (var item in best)
            //{
            //    double d = item.DoubleAccuracy;
            //    decimal m = item.Accuracy;
            //    Console.WriteLine(Math.Pow(d, 1024));
            //    Console.WriteLine(Math.Pow((double)m, 1024));
            //    for (int i = 0; i < 10; i++)
            //    {
            //        d *= d;
            //        m *= m;
            //    }
            //    Console.WriteLine(d);
            //    Console.WriteLine(m);
            //}
            best = api.GetBestPerformancesAsync("bleatingsheep", Mode.Mania, 17).Result;

            var user = api.GetUserInfoAsync(9408048, Mode.Ctb).Result;
        }

        static async Task CachedTest()
        {
            string key = string.Empty;
            Console.WriteLine("input key");
            while (key == string.Empty)
                key = Console.ReadLine().Trim();
            var r = await CachedQuerying.GetBeatmapAsync(1493143, Mode.Standard, key);
        }

        static async Task Main(string[] args)
        {
            try
            {
                //var result = await RankAsync(8);
                //await CachedTest();
                //await BloodcatTestAsync();
                ApiTest();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task<IEnumerable<(int uid, double score)>> RankAsync(int chartId)
        {
            var chart = await NewbieDatabase.GetChartWithCommitsAsync(chartId);
            if (chart == null) return null;

            var maps = chart.Maps.Select(b => (beatmap: b, score: (Func<ChartCommit, double>)(s => s.Score)));

            var debugResult1 = from m in maps
                               from c in m.beatmap.Commits
                               let calcRawScore = m.score(c)
                               let score = double.IsNaN(calcRawScore) ? 0 : calcRawScore
                               select m;
            //group new { commit = c, score } by new { c.BeatmapId, c.Uid };

            var results =
                from m in maps
                from c in m.beatmap.Commits
                let calcRawScore = m.score(c)
                let score = double.IsNaN(calcRawScore) ? 0 : calcRawScore
                group new { commit = c, score } by new { c.Beatmap, c.Uid } into commits // Key 是 map 和 uid，Values 是 Commits。
                group commits.Max(cm => cm.score) by commits.Key.Uid into highScores // Key 是 uid，Values 是每个图的最高分。
                //select (uid: highScores.Key, score: highScores.Sum()) into r
                let r = (uid: highScores.Key, score: highScores.Sum())
                orderby r.score descending
                select r;

            return results;
        }

        static async Task BloodcatTestAsync()
        {
            var api = BloodcatApi.Client;
            var result = await api.SearchRankedByKeywordAsync("");
            var r2 = await api.SearchRankedByKeywordAsync("no title");
            var resultMania = await api.SearchRankedByKeywordAsync("apple", Mode.Mania);
            var resultMult = await api.SearchRankedByKeywordAsync("apple", Mode.Standard, Mode.Mania);
        }

        static async Task ChartQueryTestAsync()
        {
            var result = await NewbieDatabase.GetChartWithCommitsAsync(8);
        }

        static void DatabaseQueryTest()
        {
            var result = NewbieDatabase.ChartInGroup(641236878);
        }

        static async Task DatabaseTestAsync()
        {
            var chart = new Chart();
            chart.ChartAdministrators.Add(1004121460);
            chart.ChartCreator = 962549599;
            chart.ChartName = "测试chart";
            chart.ChartDescription = "这是测试chart";
            chart.Groups.Add(514661057);
            chart.Groups.Add(641236878);
            chart.IsRunning = true;
            chart.Maps.Add(ChartBeatmap.FromBid(459939));
            chart.StartTime = new DateTimeOffset(2018, 3, 30, 0, 0, 0, new TimeSpan(8, 0, 0));
            chart.MaximumPerformance = 3100;
            //NewbieDatabase.AddChart(chart);

            string key = string.Empty;
            Console.WriteLine("input key");
            while (key == string.Empty)
                key = Console.ReadLine().Trim();
            var api = OsuApiClient.ClientUsingKey(key);
            var myRecent = await api.GetRecentlyAsync(6659067, Mode.Standard, 1);
            //var myRecent = await api.GetRecentlyAsync(9453012, Mode.Standard, 1);

            // old
            //var result = NewbieDatabase.Commit(514661057, myRecent[0], 3099);
            //Console.WriteLine(result.ToString());
        }
    }
}
