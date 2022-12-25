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
        Task<IExecutingResult<PlusHistory>> GetRecentPlusHistory(int osuId);
        Task<IExecutingResult> AddPlusHistoryAsync(IUserPlus userPlus);
        Task<IExecutingResult> AddPlusHistoryRangeAsync(IEnumerable<IUserPlus> userPluses);
        Task<IExecutingResult<IList<int>>> GetPlusRecordedUsersAsync();
        Task<IExecutingResult<RelationshipInfo>> GetRelationshipAsync(long qq, string relationship);
        Task<IExecutingResult<int?>> AddOrUpdateRelationship(long qq, string relationship, int userId);
    }
}
