using System.Collections.Generic;
using System.Threading.Tasks;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuQqBot.Database.Models;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public abstract class NewbieDatabaseBase : INewbieDatabase
    {
        public abstract Task<IExecutingResult> AddNewBindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName);
        public abstract Task<IExecutingResult> AddPlusHistoryAsync(IUserPlus userPlus);
        public abstract Task<IExecutingResult> AddPlusHistoryRangeAsync(IEnumerable<IUserPlus> userPluses);
        public virtual async Task<IExecutingResult<int?>> GetBindingIdAsync(long qq) => (await GetBindingInfoAsync(qq)).TryGet(bi => bi?.OsuId);
        public abstract Task<IExecutingResult<BindingInfo>> GetBindingInfoAsync(long qq);
        public abstract Task<IExecutingResult<IList<int>>> GetPlusRecordedUsersAsync();
        public abstract Task<IExecutingResult<PlusHistory>> GetRecentPlusHistory(int osuId);
    }
}
