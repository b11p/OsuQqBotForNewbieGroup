using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database;
using Bleatingsheep.OsuQqBot.Database.Models;
using OsuQqBot;
using System;
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
                await CachedTest();
                //await BloodcatTestAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static async Task BloodcatTestAsync()
        {
            var api = BloodcatApi.Client;
            var result = await api.SearchRankedByKeywordAsync("");
            var r2 = await api.SearchRankedByKeywordAsync("no title");
            var resultMania = await api.SearchRankedByKeywordAsync("apple", Mode.Mania);
            var resultMult = await api.SearchRankedByKeywordAsync("apple", Mode.Standard, Mode.Mania);
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
