using System.Collections.Generic;
using System.Threading.Tasks;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public interface INewbieDatabase
    {
        Task<IExecutingResult<BindingInfo>> GetBindingInfoAsync(long qq);

        Task<IExecutingResult<int?>> GetBindingIdAsync(long qq);
        Task<IExecutingResult> AddNewBindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName);
        Task<IExecutingResult<PlusHistory>> GetRecentPlusHistory(int osuId);
        Task<IExecutingResult> AddPlusHistoryAsync(IUserPlus userPlus);
        Task<IExecutingResult> AddPlusHistoryRangeAsync(IEnumerable<IUserPlus> userPluses);
        Task<IExecutingResult<IList<int>>> GetPlusRecordedUsersAsync();
        Task<IExecutingResult<int?>> ResetBindingAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName);
    }
}
