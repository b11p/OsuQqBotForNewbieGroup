using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public interface INewbieDatabase
    {
        Task<IExecutingResult<BindingInfo>> GetBindingInfoAsync(long qq);

        Task<IExecutingResult<int?>> GetBindingIdAsync(long qq);
        Task<IExecutingResult> AddNewBindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName);
    }
}
