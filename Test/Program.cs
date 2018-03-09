using Bleatingsheep.OsuMixedApi;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string key = string.Empty;
            Console.WriteLine("input int key");
            while (key == string.Empty)
                key = Console.ReadLine().Trim();
            var api = OsuApiClient.ClientUsingKey(key);
            var beatmap = api.GetBeatmapsAsync(1441454).Result;
        }
    }
}
