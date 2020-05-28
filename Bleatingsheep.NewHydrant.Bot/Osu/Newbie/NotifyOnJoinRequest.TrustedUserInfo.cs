using System.Diagnostics.CodeAnalysis;
using MotherShipDatabase;

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

            public static TrustedUserInfo FromMotherShip(Userinfo info, Userrole role) => new TrustedUserInfo
            {
                Id = info.UserId.Value,
                Name = role.CurrentUname,
                TotalHits = (int)(info.Count300 + info.Count100 + info.Count50),
                Performance = (double)info.PpRaw,
                PlayCount = info.Playcount.Value,
                IsBanned = role.IsBanned,
            };
        }
    }
}
