using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu;
using Microsoft.Extensions.Caching.Memory;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots
{
    //[Component("speaking_trigger_for_snapshot")]
    public class SpeakingTrigger : IMessageMonitor
    {
        private static readonly MemoryCache s_cache = new MemoryCache(new MemoryCacheOptions());
        private readonly DataMaintainer _dataMaintainer;
        private readonly IDataProvider _dataProvider;

        public SpeakingTrigger(DataMaintainer dataMaintainer, IDataProvider dataProvider)
        {
            _dataMaintainer = dataMaintainer;
            _dataProvider = dataProvider;
        }

        public async Task OnMessageAsync(MessageContext message, HttpApiClient api)
        {
            if (s_cache.TryGetValue(message.UserId, out _))
            {
                return;
            }
            s_cache.Set(message.UserId, DateTimeOffset.UtcNow, TimeSpan.FromHours(1));
            var uid = await _dataProvider.GetOsuIdAsync(message.UserId).ConfigureAwait(false);
            if (uid is null)
                return;
            var tasks = new[] {
                _dataMaintainer.UpdateAsync(uid.Value, Mode.Standard),
                _dataMaintainer.UpdateAsync(uid.Value, Mode.Taiko),
                _dataMaintainer.UpdateAsync(uid.Value, Mode.Catch),
                _dataMaintainer.UpdateAsync(uid.Value, Mode.Mania),
            };
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
