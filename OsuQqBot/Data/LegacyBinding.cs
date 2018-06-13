using Bleatingsheep.OsuQqBot.Database.Models;
using System.Threading.Tasks;

namespace OsuQqBot.Data
{
    class LegacyBinding : IBindingsAsync
    {
        public async Task<int?> BindAsync(long qq, int osuId, string osuName, string source, long operatorId, string operatorName)
        {
            return await Task.Run(() =>
            {
                return (int?)LocalData.Database.Instance.Bind(qq, osuId, source);
            });
        }

        public async Task<BindingInfo> GetBindingInfoAsync(long qq)
        {
            var queried = await Task.Run(() => LocalData.Database.Instance.GetUidFromQq(qq));
            if (queried is long osuId)
            {
                return new BindingInfo { UserId = qq, OsuId = (int)osuId };
            }
            return null;
        }

        public async Task<int?> GetBindingIdAsync(long qq)
        {
            var queried = await Task.Run(() => LocalData.Database.Instance.GetUidFromQq(qq));
            if (queried is long osuId)
            {
                return (int)osuId;
            }
            return null;
        }
    }
}
