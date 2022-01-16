using System;
using System.Linq;
using Bleatingsheep.NewHydrant.Core;

namespace Bleatingsheep.NewHydrant.Osu.Newbie
{
    internal abstract class NewbieCardChecker : Service
    {
        private NewbieCardChecker() { }

        public static INewbieInfoProvider IgnoreListProvider => HardcodedProvider.GetProvider();

        /// <summary>
        /// 获取群名片提示。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string GetHintMessage(string name, string card)
        {
            string hint;
            if (OsuHelper.DiscoverUsernames(card).Any(u => u.Equals(name, StringComparison.OrdinalIgnoreCase)))
                hint = null;
            // 用户名不行。
            else if (card.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                // 临时忽略。
                hint = "建议修改群名片，不要在用户名前后添加可以被用做用户名的字符，以免混淆。";
                hint += "\r\n" + "建议群名片：" + RecommendCard(card, name);
            }
            else
            {
                hint = "为了方便其他人认出您，请修改群名片，必须包括正确的 osu! 用户名。";
            }

            return hint;
        }

        /// <summary>
        /// 根据群名片和用户名推荐群名片
        /// </summary>
        private static string RecommendCard(string card, string username)
        {
            int firstIndex = card.IndexOf(username, StringComparison.OrdinalIgnoreCase);
            if (firstIndex != -1)
            {
                string recommendCard = card.Substring(0, firstIndex);
                if (firstIndex != 0)
                    recommendCard += "|";
                recommendCard += username;
                if (firstIndex + username.Length < card.Length)
                {
                    recommendCard += "|" + card.Substring(firstIndex + username.Length);
                }
                return recommendCard;
            }
            return null;
        }
    }
}
