using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bleatingsheep.NewHydrant.Data;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetBeatmapPlusAsPossibleController : ControllerBase
    {
        private const int QueueLimit = 76543;
        private static readonly ConcurrentQueue<int> s_requestQueue = new ConcurrentQueue<int>();

        private static readonly object s_taskLock = new object();
        private static Task s_task;

        private static async void CacheBeatmaps()
        {
            if (s_task != null && !s_task.IsCompleted)
                return;

            await Task.Yield();

            lock (s_taskLock)
            {
                if (s_task != null && !s_task.IsCompleted)
                    return;

                s_task = new Task(async () =>
                {
                    NewbieContext context = null;
                    var plus = new PerformancePlusSpider();

                    while (s_requestQueue.TryDequeue(out int id))
                    {
                        try
                        {
                            if (context == null)
                                context = new NewbieContext();
                            if (await context.BeatmapPlusCache.AnyAsync(b => b.Id == id))
                                continue;

                            (context as IDisposable).Dispose();
                            context = null;

                            try
                            {
                                await plus.GetCachedBeatmapPlusAsync(id);
                            }
                            catch (ExceptionPlus e) when (e.Message.Contains("500", StringComparison.Ordinal))
                            {
                                // ignored
                            }
                        }
                        catch (Exception)
                        {
                            // 释放数据库 context，确保稍后重新连接。
                            context = null;
                            s_requestQueue.Enqueue(id);
                            await Task.Delay(60000);
                        }
                    }
                }, TaskCreationOptions.LongRunning);
                s_task.Start();
            }
        }

        [HttpPost]
        public ActionResult<List<BeatmapPlus>> GetExistedBeatmaps(int[] queryBeatmaps)
        {
            using (var context = new NewbieContext())
            {
                var result = context.BeatmapPlusCache.Where(p => queryBeatmaps.Contains(p.Id)).ToList();
                var notInCache = queryBeatmaps.Except(result.Select(b => b.Id));
                foreach (var id in notInCache)
                {
                    if (s_requestQueue.Count > QueueLimit)
                        break;
                    s_requestQueue.Enqueue(id);
                    CacheBeatmaps();
                }
                return result;
            }
        }
    }
}