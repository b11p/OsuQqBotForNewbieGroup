using Bleatingsheep.OsuMixedApi;
using System;

namespace OsuQqBot.Query
{
    internal static class UserInfoExtension
    {
        public static string TextInfo(this UserInfo userInfo, bool showMode = false)
        {
            string[] byLine = new string[9];

            string displayAcc;
            try
            {
                displayAcc = userInfo.Accuracy.ToString("#.##%");
            }
            catch (FormatException)
            {
                displayAcc = userInfo.Accuracy.ToString();
            }

            byLine[0] = userInfo.Name + "的个人信息" + (userInfo.Mode == Mode.Standard && !showMode ? "" : "—" + userInfo.Mode.GetShortModeString());
            byLine[1] = string.Empty;
            byLine[2] = userInfo.Performance + "pp 表现";
            byLine[3] = "#" + userInfo.Rank;
            byLine[4] = userInfo.Country + " #" + userInfo.CountryRank;
            byLine[5] = (userInfo.RankedScore).ToString("#,###") + " Ranked谱面总分";
            byLine[6] = displayAcc + " 准确率";
            byLine[7] = userInfo.PlayCount + " 游玩次数";
            byLine[8] = (userInfo.TotalHits).ToString("#,###") + " 总命中次数";

            return string.Join(Environment.NewLine, byLine);
        }
    }
}
