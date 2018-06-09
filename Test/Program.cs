using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database;
using Bleatingsheep.OsuQqBot.Database.Models;
using OsuQqBot;
using OsuQqBot.Charts;
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
        
        static async Task Main(string[] args)
        {
            try
            {
                await CsvTest();
                //ExpressionTest();
                //ChartTestInNewbieFurther();
                //var result = await RankAsync(8);
                //await CachedTest();
                //await BloodcatTestAsync();
                //await DatabaseTestAsync();
                //ApiTest();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task CsvTest()
        {
            //var result = await OsuQqBot.Charts.Statistics.CsvResultAsync(1);
        }

        public static void ExpressionTest()
        {
            Expression<ChartTry> exp;
            exp = new Expression<ChartTry>("acc^10*200+combo", Operators, Values);
            exp = new Expression<ChartTry>("combo", Operators, Values);
            exp = new Expression<ChartTry>("score/15000", Operators, Values);
        }

        private static readonly IReadOnlyDictionary<string, (Func<double, double, double> function, int priority)> Operators
            = new Dictionary<string, (Func<double, double, double> function, int priority)>
            {
                { "+", ((x, y) => x + y, 1) },
                { "-", ((x, y) => x - y, 1) },
                { "*", ((x, y) => x * y, 2) },
                { "/", ((x, y) => x / y, 2) },
                { "^", ((x, y) => Math.Pow(x, y), 3) },
            };

        private static readonly IReadOnlyDictionary<string, Func<ChartTry, double>> Values = new Dictionary<string, Func<ChartTry, double>>
        {
            { "acc",  c => c.Accuracy },
            { "accuracy",  c => c.Accuracy },
            { "combo",  c => c.Combo },
            { "score",  c => c.Score },
            { "pp",  c => c.PPWhenCommit },
            { "performance",  c => c.PPWhenCommit },
        };

        public static async Task<IEnumerable<(int uid, double score)>> RankAsync(int chartId)
        {
            var chart = await NewbieDatabase.GetChartWithCommitsAsync(chartId);
            if (chart == null) return null;

            var maps = chart.Maps.Select(b => (beatmap: b, score: (Func<ChartTry, double>)(s => s.Score)));

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
                group new { commit = c, score } by new { c.Beatmap, c.UserId } into commits // Key 是 map 和 uid，Values 是 Commits。
                group commits.Max(cm => cm.score) by commits.Key.UserId into highScores // Key 是 uid，Values 是每个图的最高分。
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

        static void ChartTestInNewbieFurther()
        {
            Chart chart = new Chart();
            chart.Administrators = new List<ChartAdministrator>
            {
                1239219529,
                1061566571,
                546748348,
                2541721178,
                2482000231,
                431600414
            };
            chart.Groups.Add(514661057);
            chart.IsRunning = true;
            chart.ChartCreator = 962549599;
            chart.ChartName = "Dalou选了7.2的一个chart。";
            chart.ChartDescription = "7.2是dalou选的，爽不爽？";
            chart.Maps.Add(new ChartBeatmap()
            {
                BeatmapId = 1580728,
                Mode = Mode.Standard,
                ScoreCalculation = "acc^10*200+combo",
            });
            chart.Maps.Add(new ChartBeatmap()
            {
                BeatmapId = 809513,
                Mode = Mode.Standard,
                ScoreCalculation = "combo",
                AllowsFail = true,
            });
            chart.Maps.Add(new ChartBeatmap()
            {
                BeatmapId = 545555,
                Mode = Mode.Standard,
                ScoreCalculation = "score/15000",
                AllowsFail = true,
            });
            NewbieDatabase.AddChart(chart);
        }

        static void DatabaseQueryTest()
        {
            var result = NewbieDatabase.ChartInGroup(641236878);
        }

        static async Task DatabaseTestAsync()
        {
            var chart = new Chart();
            chart.Administrators.Add(1004121460);
            chart.ChartCreator = 962549599;
            chart.ChartName = "新人群第0.1期Chart（Beta）";
            chart.ChartDescription = "这是新人群第0.1期Chart，因为是技术测试，所以暂时没有奖励";
            chart.Groups.Add(614892339);
            //chart.Groups.Add(641236878);
            chart.IsRunning = true;
            chart.Maps.Add(ChartBeatmap.FromBid(1568319));
            chart.Maps.Add(ChartBeatmap.FromBid(1489625));
            chart.Maps.Add(ChartBeatmap.FromBid(1421312));
            chart.StartTime = new DateTimeOffset(2018, 4, 27, 0, 0, 0, new TimeSpan(8, 0, 0));
            chart.MaximumPerformance = 2500;
            chart.RecommendPerformance = 1250;
            chart.Public = true;
            NewbieDatabase.AddChart(chart);

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
