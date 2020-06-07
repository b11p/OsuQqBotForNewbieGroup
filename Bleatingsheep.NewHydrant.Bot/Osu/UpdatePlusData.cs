using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.Osu.PerformancePlus;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Component("pp+_update")]
    internal class UpdatePlusData : OsuFunction, IRegularAsync
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();

        public TimeSpan? OnUtc => new TimeSpan(20, 0, 0);

        public TimeSpan? Every => null;

        public async Task RunAsync(HttpApiClient api)
        {
            var result = await Database.GetPlusRecordedUsersAsync();
            if (!result.Success)
            {
                FLogger.LogInBackground("更新 PP+ 数据时访问数据库失败。");
                FLogger.LogException(result.Exception);
                return;
            }

            IEnumerable<int> todo = result.Result;
            Logger.Info($"找到{todo.Count()}个查询过的玩家。");
            int retry = 10;
            do
            {
                const int threads = 10;
                Logger.Debug($"开始查询，线程数为{threads.ToString(CultureInfo.InvariantCulture)}");
                var failed = new ConcurrentBag<int>();
                var results = new ConcurrentBag<IUserPlus>();

                Parallel.ForEach(todo, new ParallelOptions
                {
                    MaxDegreeOfParallelism = threads,
                }, userId =>
                {
                    try
                    {
                        var user = s_spider.GetUserPlusAsync(userId).ConfigureAwait(false).GetAwaiter().GetResult();
                        if (user != null)
                            results.Add(user);
                    }
                    catch (Exception)
                    {
                        failed.Add(userId);
                    }
                });
                Logger.Info($"查询成功{results.Count}条。");
                Logger.Info($"失败{failed.Count}条，首个失败是{failed.FirstOrDefault()}");
                var addResult = await Database.AddPlusHistoryRangeAsync(results);
                if (!addResult.Success)
                {
                    FLogger.LogInBackground("添加新的 PP+ 数据失败。");
                    FLogger.LogException(addResult.Exception);
                }

                todo = failed.ToList();
                await Task.Delay(600_000);
                retry--;
            } while (todo.Any() && retry > 0);


        }
    }
}
