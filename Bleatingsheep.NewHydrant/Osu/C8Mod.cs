using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuQqBot.Database.Models;
using Sisters.WudiLib;
using static System.Math;

namespace Bleatingsheep.NewHydrant.Osu
{
    public static class C8Mod 
    {
        /// <summary>
        /// 计算化学式 cost。
        /// </summary>
        /// <param name="plus">用户数据信息。</param>
        /// <returns></returns>
        public static double CostOf(IUserPlus plus)
        {
            return 10 * Sqrt((Atan((2 * plus.AimJump - (1700 + 1300)) / (1700 - 1300)) + PI / 2 + 8) * (Atan((2 * plus.AimFlow - (450 + 200)) / (450 - 200)) + PI / 2 + 3))
+ (Atan((2 * plus.Precision - (400 + 200)) / (400 - 200)) + PI / 2)
+ 7 * (Atan((2 * plus.Speed - (1250 + 950)) / (1250 - 950)) + PI / 2)
+ 3 * (Atan((2 * plus.Stamina - (1000 + 600)) / (1000 - 600)) + PI / 2)
+ 10 * (Atan((2 * plus.Accuracy - (1200 + 600)) / (1200 - 600)) + PI / 2);
        }
        
        /// <summary>
        /// PP+ 化学式新人群（进阶）威力加强 mod。
        /// </summary>
        /// <param name="raw">原始输出。</param>
        /// <param name="history">对比的数据，可能为 <c>null</c>。</param>
        /// <param name="userPlus">当前数据。</param>
        /// <returns>修改后的输出。</returns>
        public static string ModPerformancePlus(string raw, PlusHistory history, IUserPlus userPlus)
        {
            return raw + $"\r\n化学式没付钱指数：{CostOf(userPlus):0.0}";
        }
    }
}
