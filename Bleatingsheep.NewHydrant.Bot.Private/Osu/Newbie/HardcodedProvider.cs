using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    internal class HardcodedProvider : INewbieInfoProvider
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
            2639140005, // 小taolex
            2636027237,
            1806173747, // 白bot
            834276213, // 猫猫bot
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
            1061566571, // dalou
            178039743, // whir
            820773512, // 亚森
            97512825, // a9
        };

        private const long NewbieGroupId = 595985887;

        public static INewbieInfoProvider GetProvider() => new HardcodedProvider();

        public string Name => "ignore";

        public IEnumerable<long> MonitoredGroups { get; } = new List<long> { NewbieGroupId }.AsReadOnly();

#pragma warning disable CS1998
        public async Task<bool> ShouldIgnoreAsync(long qq) => IgnoreList.Contains(qq);
        public async Task<bool> ShouldIgnorePerformanceAsync(long group, long qq) => group == NewbieGroupId ? IgnorePerformanceListBase.Contains(qq) : false;
#pragma warning restore CS1998
        public double? PerformanceLimit(long group) => group == NewbieGroupId ? (double?)2500 : null;
    }
}
