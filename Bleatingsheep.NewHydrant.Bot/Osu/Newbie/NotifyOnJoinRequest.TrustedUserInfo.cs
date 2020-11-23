using System.Diagnostics.CodeAnalysis;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    internal partial class NotifyOnJoinRequest
    {
        private class TrustedUserInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int TotalHits { get; set; }
            public double Performance { get; set; }
            public int PlayCount { get; set; }
            public bool IsBanned { get; set; }

#nullable enable
            [return: NotNullIfNotNull("userInfo")]
            public static implicit operator TrustedUserInfo?(OsuMixedApi.UserInfo? userInfo)
                => userInfo is null ? null :
                new TrustedUserInfo
                {
                    Id = userInfo.Id,
                    Name = userInfo.Name,
                    TotalHits = userInfo.TotalHits,
                    Performance = userInfo.Performance,
                    PlayCount = userInfo.PlayCount,
                    IsBanned = false,
                };
#nullable restore
        }
    }
}
