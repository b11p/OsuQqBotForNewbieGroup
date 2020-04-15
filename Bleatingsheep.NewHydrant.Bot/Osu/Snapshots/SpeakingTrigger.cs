using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.Osu;
using Microsoft.Extensions.Caching.Memory;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots
{
    [Function("speaking_trigger_for_snapshot")]
    public class SpeakingTrigger : OsuFunction, IMessageMonitor
    {
        private static readonly MemoryCache s_cache = new MemoryCache(new MemoryCacheOptions());

        public async Task OnMessageAsync(MessageContext message, HttpApiClient api)
        {
            if (s_cache.TryGetValue(message.UserId, out _))
            {
                return;
            }
            s_cache.Set(message.UserId, DateTimeOffset.Now, TimeSpan.FromHours(1));
            (_, var uid) = await DataProvider.GetBindingIdAsync(message.UserId).ConfigureAwait(false);
            if (uid is null) return;
            var snapUtility = new SnapshotUtility();
            var tasks = new[] {
                snapUtility.UpdateAsync(uid.Value, Mode.Standard),
                snapUtility.UpdateAsync(uid.Value, Mode.Taiko),
                snapUtility.UpdateAsync(uid.Value, Mode.Catch),
                snapUtility.UpdateAsync(uid.Value, Mode.Mania),
            };
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
