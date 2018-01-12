using System.Collections.Generic;

namespace OsuQqBot
{
    public partial class OsuQqBot
    {
        /// <summary>
        /// 从群名片获取可能的osu!用户名
        /// </summary>
        /// <param name="groupNamecard">群名片</param>
        /// <returns>一个数组，包括可能的用户名</returns>
        //static (string username, long uid)[] ParseUsername(string groupNamecard)
        static string[] ParseUsername(string groupNamecard)
        {
            var matches = regexMatchingUsername.Matches(groupNamecard);
            List<string> results = new List<string>();
            for (int i = 0; i < matches.Count; i++)
            {
                string username = matches[i].Groups[1].Value;
                results.Add(username);
            }
            return results.ToArray();
        }

        /**
         * 注意这个模式有一点问题，由于username中可以使用空格，但是不能用在开头或者结尾（会被删除），
         * 遇到连续的单词，如果总长度超过15，会被拆分。
         * 例如："最强ever my angel my angel my angel my angel my angel"，会匹配到下列内容
         * "强ever my angel" 保存 "ever my angel"
         * " my angel my" 保存 "my angel my"
         * " angel my angel" 保存 "angel my angel"
         * " my angel" 保存 "my angel"
         */
        const string pattern = "(?:^|[^0-9A-Za-z_\\-\\[\\]])" + // 匹配字符串开始，或者任何不能使用在osu! username的字符，或者空格（不能使用在username开头）
            "([0-9A-Za-z_\\-\\[\\]][0-9A-Za-z_\\-\\[\\] ]{1,13}[0-9A-Za-z_\\-\\[\\]])" + // 匹配ID中可以使用的字符，其中ID的长度是3-15
            "(?=$|[^0-9A-Za-z_\\-\\[\\]])"; // 匹配字符串结束，或者不能使用在osu! username的字符，或者空格（不能使用在username结尾）
        static readonly System.Text.RegularExpressions.Regex regexMatchingUsername = new System.Text.RegularExpressions.Regex(pattern);
    }
}