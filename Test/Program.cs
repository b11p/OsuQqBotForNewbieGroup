using Bleatingsheep.OsuMixedApi;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
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
    }
}
