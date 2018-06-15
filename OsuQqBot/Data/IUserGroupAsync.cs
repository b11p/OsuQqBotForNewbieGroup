using System.Threading.Tasks;
using static OsuQqBot.Data.Groups;

namespace OsuQqBot.Data
{
    interface IUserGroupAsync
    {
        Task<bool> IsAsync(long qq, string group);
        Task<bool> AddAsync(long qq, string group);
        Task<bool> DeleteAsync(long qq, string group);
    }

    internal static class UserGroupExtensions
    {
        public static async Task<bool> ShouldIgnorePerformance(this IUserGroupAsync group, long qq) => await group.IsAsync(qq, Admin) || await group.IsAsync(qq, Coach) || await group.IsAsync(qq, Bot);
    }
}
