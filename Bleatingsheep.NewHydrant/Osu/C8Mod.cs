using System;
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
        public static string CostOf(IUserPlus plus)
        {
            double[,] db = new double[6, 13] {
    {1700,1975,2250,2525,2800,3075,3365,3800,4400,4900,5900,6900,9999},
    {450,563,675,788,900,1013,1225,1500,1825,2245,3200,4400,9999},
    {400,463,525,588,650,713,825,950,1350,1650,2300,3050,9999},
    {1250,1363,1475,1588,1700,1813,1925,2200,2400,2650,3100,3600,9999},
    {1000,1100,1200,1300,1400,1500,1625,1800,2000,2200,2600,3050,9999},
    {1200,1300,1400,1500,1600,1700,1825,2000,2800,3500,4800,6150,9999}
    };
            string[] dn = new string[12] { "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ", "Ⅸ", "Ⅹ", "ΕⅩ", default };
            int[,] n = new int[2, 6] { { plus.AimJump, plus.AimFlow, plus.Precision, plus.Speed, plus.Stamina, plus.Accuracy }, { -1, -1, -1, -1, -1, -1 } };
            //计算段位
            int suml = 0, max1 = 0;
            int i, j;
            for (i = 0; i < 6; i++)
            {
                for (j = 0; j < 13; j++) if (n[0, i] > db[i, j]) n[1, i]++;
                if (n[1, i] == -1) n[1, i] -= 1; suml += n[1, i];
                if (n[1, i] > max1) max1 = n[1, i];
            }
            //计算第二高段位
            int max2 = -2, max2t = 0;
            for (i = 0; i < 6; i++)
            {
                if (n[1, i] > max2 && n[1, i] != max1) max2 = n[1, i];
                if (n[1, i] == max1) max2t += 1;
                if (n[1, i] == 11) n[1, i] = (int)Math.Ceiling(n[0, i] / db[i, 11] * 10);
                dn[11] += (n[1, i] + " ");
            }
            if (max2t > 1) max2 = max1;
            //针对一般群员
            if (max2 < 1) return "进阶群综合指数:"
                    + (10 * Sqrt((Atan((2.0 * plus.AimJump - (1700 + 1300)) / (1700 - 1300)) + PI / 2 + 8) * (Atan((2.0 * plus.AimFlow - (450 + 200)) / (450 - 200)) + PI / 2 + 3))
            + (Atan((2.0 * plus.Precision - (400 + 200)) / (400 - 200)) + PI / 2)
            + 7 * (Atan((2.0 * plus.Speed - (1250 + 950)) / (1250 - 950)) + PI / 2)
            + 3 * (Atan((2.0 * plus.Stamina - (1000 + 600)) / (1000 - 600)) + PI / 2)
            + 10 * (Atan((2.0 * plus.Accuracy - (1200 + 600)) / (1200 - 600)) + PI / 2)).ToString("f2");
            //针对高分人
            else return "进阶群进阶指数判定：" + dn[max2 - 1] + (suml < max2 * 6 - 4 ? "\r\n" : "段达标\r\n")
                    + "详细情况:" + dn[11] + "=" + suml + " 达标要求" + (max2 * 6 - 4);
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
            return raw + $"\r\n{cost}";
        }
    }
}
