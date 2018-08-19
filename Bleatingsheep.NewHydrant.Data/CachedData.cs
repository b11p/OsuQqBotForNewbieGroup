using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.NewHydrant.Data
{
    public static class CachedData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="beatmapId"></param>
        /// <exception cref="DbUpdateException">数据库查询失败。</exception>
        /// <exception cref="ExceptionPlus">查询 PP+ 失败。</exception>
        /// <returns></returns>
        public static async Task<IBeatmapPlus> GetBeatmapPlusAsync(int beatmapId)
        {
            return await new PerformancePlusSpider().GetCachedBeatmapPlusAsync(beatmapId);
        }

        /// <exception cref="DbUpdateException">数据库查询失败。</exception>
        /// <exception cref="ExceptionPlus">查询 PP+ 失败。</exception>
        public static async Task<IBeatmapPlus> GetCachedBeatmapPlusAsync(this PerformancePlusSpider ppp, int id)
        {
            var fromDb = await GetFromDbAsync(c => c.BeatmapPlusCache, b => b.Id == id);
            if (fromDb != null)
                return fromDb;

            var fromPpp = await ppp.GetBeatmapPlusAsync(id) as BeatmapPlus;

            if (fromPpp != null)
                await TryAddCacheAsync(c => c.BeatmapPlusCache, fromPpp);

            return fromPpp;
        }

        public static async Task<Beatmap[]> GetCachedBeatmapAsync(this OsuApiClient osuApiClient, string md5)
        {
            try
            {
                var fromDb = await GetFromDbAsync(context => context.CachedBeatmaps, b => b.FileMD5 == md5);
                if (fromDb != null)
                    return new[] { fromDb };
            }
            catch
            {
            }

            var fromApi = await osuApiClient.GetBeatmapAsync(md5);

            if (fromApi?.Any() == true && fromApi[0].IsInfoFixed())
            {
                await TryAddCacheAsync(context => context.CachedBeatmaps, fromApi[0]);
            }
            return fromApi;
        }

        private static async Task<T> GetFromDbAsync<T>(Func<NewbieContext, DbSet<T>> table, Expression<Func<T, bool>> predicate) where T : class
        {
            using (var context = new NewbieContext())
            {
                var result = await table(context).SingleOrDefaultAsync(predicate);
                return result;
            }
        }

        private static async Task TryAddCacheAsync<T>(Func<NewbieContext, DbSet<T>> table, T entity) where T : class
        {
            try
            {
                using (var context = new NewbieContext())
                {
                    await table(context).AddAsync(entity);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
