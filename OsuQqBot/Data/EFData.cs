using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace OsuQqBot.Data
{
    class EFData : IBindingsAsync
    {
        public async Task<int?> BindAsync(long qq, int osuId, string osuName, string source, long operatorId, string operatorName)
        {
            return await NewbieDatabase.BindAsync(qq, osuId, osuName, source, operatorId, operatorName);
        }

        public async Task<BindingInfo> GetBindingInfoAsync(long qq)
        {
            return await NewbieDatabase.GetBindingInfoAsync(qq);
        }

        public async Task<int?> GetBindingIdAsync(long qq)
        {
            return await NewbieDatabase.GetBindingIdAsync(qq);
        }
    }
}
