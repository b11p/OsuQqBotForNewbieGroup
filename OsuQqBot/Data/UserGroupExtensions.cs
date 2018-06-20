using System.Threading.Tasks;
using static OsuQqBot.Data.Groups;

namespace OsuQqBot.Data
{
    internal static class UserGroupExtensions
    {
        public static async Task<bool> IsGrantedAsync(this IUserGroupAsync group, long qq) => await group.IsAsync(qq, SA) || await group.IsAsync(qq, Admin);

        public static async Task<bool> ShouldIgnoreCardAsync(this IUserGroupAsync group, long qq) => await group.IsAsync(qq, Bot);

        public static async Task<bool> ShouldIgnorePerformanceAsync(this IUserGroupAsync group, long qq) => await group.IsGrantedAsync(qq) || await group.IsAsync(qq, Coach) || await group.ShouldIgnoreCardAsync(qq);
    }
}
