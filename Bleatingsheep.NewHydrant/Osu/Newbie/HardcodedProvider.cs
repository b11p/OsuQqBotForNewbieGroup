using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib.Responses;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    internal class HardcodedProvider : INewbieInfoProvider, IInitializable
    {
        private static readonly HashSet<long> IgnoreList = new HashSet<long>
        {
            2478057279, // BanchooBot by int100
            405622418, // 大号
            1677323371, // interBot
            1376729907, // 松花蛋
            1394932996, // Sayobot
            1587912578, // Yamiko
            2762388263, // Yumli_Bot
            3082577334, // DalouBot
            3527783823, // nodebot
            2308394636, // yimoQWQ
            1131545658, // 消防栓 beta
            2680306741, // interbot2
        };

        private static readonly HashSet<long> IgnorePerformanceListBase = new HashSet<long>
        {
            123312230,
            183392679,
            920979541,
            1239219529,
            1012621328,
            2541721178,
            2482000231,
            1442455430,
            431600414,
            630060047,
            2643555740,
            2307282906,
            546748348,
            1023406736,
            1277818495,
            1354413508,
            2875452763,
            3363569388,
            359603915,
            2308394636,
            1316740753,
            2412800228,
            2429299722,
            1149483077,
            962549599,
            1208604740,
            1061566571,
        };

        private const long NewbieGroupId = 614892339;
        private static HashSet<long> s_ignorePerformanceNewbie = new HashSet<long>();

        private const long NewbieGroup2Id = 758120648;
        private static HashSet<long> s_ignorePerformanceNewbie2 = new HashSet<long>();

        private static readonly HashSet<GroupMemberInfo.GroupMemberAuthority> AcceptedIgnore = new HashSet<GroupMemberInfo.GroupMemberAuthority>
        {
            GroupMemberInfo.GroupMemberAuthority.Leader,
            GroupMemberInfo.GroupMemberAuthority.Manager
        };

        public static INewbieInfoProvider GetProvider() => new HardcodedProvider();

        public string Name => "ignore";

        public IEnumerable<long> MonitoredGroups { get; } = new List<long> { NewbieGroupId, NewbieGroup2Id }.AsReadOnly();

        public async Task<bool> InitializeAsync(ExecutingInfo executingInfo)
        {
            var qq = executingInfo.Qq;
            var listTask2 = qq.GetGroupMemberListAsync(NewbieGroup2Id);
            var list1 = await qq.GetGroupMemberListAsync(NewbieGroupId);
            var list2 = await listTask2;
            s_ignorePerformanceNewbie = list1.Where(mi => AcceptedIgnore.Contains(mi.Authority)).Select(mi => mi.UserId).ToHashSet();
            s_ignorePerformanceNewbie2 = list2.Where(mi => AcceptedIgnore.Contains(mi.Authority)).Select(mi => mi.UserId).ToHashSet();

            return true;
        }

#pragma warning disable CS1998
        public async Task<bool> ShouldIgnoreAsync(long qq) => IgnoreList.Contains(qq);
        public async Task<bool> ShouldIgnorePerformanceAsync(long group, long qq) => group == NewbieGroupId ? IgnorePerformanceListBase.Contains(qq) : false;
#pragma warning restore CS1998
        public double? PerformanceLimit(long group) => group == NewbieGroupId ? (double?)2500 : null;
    }
}
