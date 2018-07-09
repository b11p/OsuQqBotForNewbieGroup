using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public interface INewbieDatabase
    {
        Task<BindingInfo> GetBindingInfoAsync(long qq);

        Task<int?> GetBindingIdAsync(long qq);
    }
}
