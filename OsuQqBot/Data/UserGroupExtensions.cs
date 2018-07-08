using System.Threading.Tasks;
using static OsuQqBot.Data.Groups;

namespace OsuQqBot.Data
{
    internal static class UserGroupExtensions
    {
        public static bool IsGranted(this IUserGroupAsync group, long qq) => group.Is(qq, SA) || group.Is(qq, Admin);

        public static bool ShouldIgnoreCard(this IUserGroupAsync group, long qq) => group.Is(qq, Bot) || group.Is(qq, OtherIgnore);

        public static bool ShouldIgnorePerformance(this IUserGroupAsync group, long qq) => group.IsGranted(qq) || group.Is(qq, NoCheckPerformance) || group.ShouldIgnoreCard(qq);
    }
}
