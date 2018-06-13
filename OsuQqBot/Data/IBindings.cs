using Bleatingsheep.OsuQqBot.Database.Models;
using System.Threading.Tasks;

namespace OsuQqBot.Data
{
    interface IBindingsAsync
    {
        Task<int?> BindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName);

        Task<int?> GetBindingIdAsync(long qq);

        Task<BindingInfo> GetBindingInfoAsync(long qq);
    }
}
