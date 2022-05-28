using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Osu;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
using System.Linq;
using Bleatingsheep.OsuQqBot.Database.Models;
using Bleatingsheep.Osu.PerformancePlus;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Bleatingsheep.NewHydrant.Extentions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Core;

namespace Bleatingsheep.NewHydrant.Osu.Plus
{
    [Component("recommend")]
    class Recommendation : Service, IMessageCommand
    {
        private readonly IDbContextFactory<NewbieContext> _dbContextFactory;

        private ILegacyDataProvider DataProvider { get; }

        public Recommendation(ILegacyDataProvider dataProvider, IDbContextFactory<NewbieContext> dbContextFactory)
        {
            DataProvider = dataProvider;
            _dbContextFactory = dbContextFactory;
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var id = await DataProvider.EnsureGetBindingIdAsync(context.UserId);
            var myWebApi = WebApiClient.HttpApi.Resolve<IPlusApi>();
            var user = await myWebApi.GetUserAsync(id);
            var rw = user.Performances.Total.Select((r, i) => (record: r, weight: Math.Pow(0.95, i)));
            IEnumerable<BeatmapPlus> beatmaps, recommends;
            await using var dbContext = _dbContextFactory.CreateDbContext();
            beatmaps = await dbContext.BeatmapPlusCache.Where(b => user.Performances.Total.Select(r => r.BeatmapId).Contains(b.Id)).ToListAsync();
            var rwbResult = (from rwEntity in rw
                             join b in beatmaps on rwEntity.record.BeatmapId equals b.Id
                             select (rwEntity.record, rwEntity.weight, beatmap: b)).ToList();
            Func<BeatmapPlus, double> selector = b => b.Stars;
            var avgStars = CalculateAverage(rwbResult, selector);
            recommends = (await dbContext.BeatmapPlusCache.Where(b => b.Stars > avgStars - 0.2 && b.Stars < avgStars + 0.2).ToListAsync()).Randomize();

            var recommendResult = CalculateDistance(recommends, CreateDistanceCalculator(rwbResult, selector, b => b.AimTotal, b => b.Precision, b => b.Speed, b => b.Stamina, b => b.Accuracy));
            var selectedRecommend = recommendResult.OrderByDescending(t => t.distance).Take(5);

            await api.SendMessageAsync(context.Endpoint, string.Join("\r\n", selectedRecommend.Select(r => $"https://osu.ppy.sh/b/{r.beatmap.Id}")));
        }

        private static double CalculateAverage(List<(Record record, double weight, BeatmapPlus beatmap)> rwbResult, Func<BeatmapPlus, double> selector)
            => rwbResult.Sum(rwb => selector(rwb.beatmap) * rwb.weight) / rwbResult.Sum(rwb => rwb.weight);

        private Dictionary<Func<BeatmapPlus, double>, double> CreateDistanceCalculator(List<(Record record, double weight, BeatmapPlus beatmap)> rwbResult, params Func<BeatmapPlus, double>[] funcs)
        {
            var result = funcs.ToDictionary(f => f, f => CalculateAverage(rwbResult, f));
            return result;
        }

        private IEnumerable<(BeatmapPlus beatmap, double distance)> CalculateDistance(IEnumerable<BeatmapPlus> recommends, Dictionary<Func<BeatmapPlus, double>, double> calculatorData)
        {
            return recommends.Select(b =>
            {
                double distance = calculatorData.Sum(c => Math.Pow(c.Key(b) - c.Value, 2));
                return (b, distance);
            });
        }

        public bool ShouldResponse(MessageContext context)
            => context.Content.TryGetPlainText(out var text) && text == "荐图";
    }
}
