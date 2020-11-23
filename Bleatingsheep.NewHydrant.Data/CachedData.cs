using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

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
        /// <exception cref="MySqlException"></exception>
        /// <returns></returns>
        public static async Task<IBeatmapPlus> GetBeatmapPlusAsync(int beatmapId)
        {
            return await new PerformancePlusSpider().GetCachedBeatmapPlusAsync(beatmapId);
        }

        /// <exception cref="DbUpdateException">数据库查询失败。</exception>
        /// <exception cref="ExceptionPlus">查询 PP+ 失败。</exception>
        /// <exception cref="MySqlException"></exception>
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

        /// <exception cref="MySqlException"></exception>
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
