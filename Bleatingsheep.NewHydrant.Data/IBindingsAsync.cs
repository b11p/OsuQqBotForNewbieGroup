using System;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.NewHydrant.Data
{
    public interface IBindingsAsync
    {
        Task<(bool, int?)> BindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName);

        Task<(bool, int?)> GetBindingIdAsync(long qq);

        Task<(bool, BindingInfo)> GetBindingInfoAsync(long qq);
    }
}
