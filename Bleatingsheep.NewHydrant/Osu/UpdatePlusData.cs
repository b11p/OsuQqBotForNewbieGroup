using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu.PerformancePlus;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("pp+_update")]
    internal class UpdatePlusData : OsuFunction, IRegularAsync
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();

        public TimeSpan? OnUtc => new TimeSpan(20, 30, 0);

        public TimeSpan? Every => null;

        public async Task RunAsync(HttpApiClient api, ExecutingInfo executingInfo)
        {
            var result = await Database.GetPlusRecordedUsersAsync();
            if (!result.Success)
            {
                executingInfo.Logger.LogInBackground("更新 PP+ 数据时访问数据库失败。");
                executingInfo.Logger.LogException(result.Exception);
                return;
            }

            IEnumerable<int> todo = result.Result;
            int retry = 10;
            do
            {
                var failed = new ConcurrentBag<int>();
                var results = new ConcurrentBag<IUserPlus>();

                todo.AsParallel().ForAll(userId =>
                {
                    try
                    {
                        var user = s_spider.GetUserPlusAsync(userId).Result;
                        if (user != null)
                            results.Add(user);
                    }
                    catch (Exception)
                    {
                        failed.Add(userId);
                    }
                });
                var addResult = await Database.AddPlusHistoryRangeAsync(results);
                if (!addResult.Success)
                {
                    executingInfo.Logger.LogInBackground("添加新的 PP+ 数据失败。");
                    executingInfo.Logger.LogException(addResult.Exception);
                }

                todo = failed.ToList();
                await Task.Delay(600_000);
                retry--;
            } while (todo.Any() && retry > 0);


        }
    }
}
