using Bleatingsheep.Osu.PerformancePlus;
using Bleatingsheep.OsuQqBot.Database.Models;
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
            double[,] db = new double[2, 6] { { 1700, 450, 400, 1250, 1000, 1200 }, { 275, 112.5, 62.5, 112.5, 100, 100 } };
            double[,] n = new double[2, 6] { { plus.AimJump, plus.AimFlow, plus.Precision, plus.Speed, plus.Stamina, plus.Accuracy }, { -2, -2, -2, -2, -2, -2 } };
            //计算段位
            double suml = 0, max1 = 0;
            int i;
            for (i = 0; i < 6; i++)
            {
                while (n[0, i] >= db[0, i])
                {
                    n[0, i] -= db[1, i];
                    n[1, i] += (n[1, i] == -2 ? 2 : 1);
                }
                suml += n[1, i];
                if (n[1, i] > max1)
                    max1 = n[1, i];
            }
            //计算第二高的段位
            double max2 = -2;
            for (i = 0; i < 6; i++)
                if (max1 == n[1, i])
                {
                    n[1, i] = -2;
                    break;
                }
            for (i = 0; i < 6; i++)
                if (max1 - max2 > max1 - n[1, i])
                    max2 = n[1, i];
            //针对一般群员
            if (max2 < 1)
                return 10 * Sqrt((Atan((2.0 * plus.AimJump - (1700 + 1300)) / (1700 - 1300)) + PI / 2 + 8) * (Atan((2.0 * plus.AimFlow - (450 + 200)) / (450 - 200)) + PI / 2 + 3))
+ (Atan((2.0 * plus.Precision - (400 + 200)) / (400 - 200)) + PI / 2)
+ 7 * (Atan((2.0 * plus.Speed - (1250 + 950)) / (1250 - 950)) + PI / 2)
+ 3 * (Atan((2.0 * plus.Stamina - (1000 + 600)) / (1000 - 600)) + PI / 2)
+ 10 * (Atan((2.0 * plus.Accuracy - (1200 + 600)) / (1200 - 600)) + PI / 2);
            //针对高分人
            else
                return suml + (max1 < 5 ? (max2 * 6 - 4) / (max2 > 2 ? 100.0 : 10.0) : 999.99 - suml) * (suml < 0 ? -1 : 1);
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
            var cost = CostOf(userPlus);
            return raw + $"\r\n化学式没付钱{(cost >= 50 ? "指数" : "段位")}：{cost:0.00}";
        }
    }
}
