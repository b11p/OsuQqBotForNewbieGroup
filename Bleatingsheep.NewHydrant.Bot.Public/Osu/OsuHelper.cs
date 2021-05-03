using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bleatingsheep.NewHydrant.Osu
{
    public static class OsuHelper
    {
        /// <summary>
        /// 发现用户名用的模式。
        /// </summary>
        private const string DiscoverPattern = "(?<=^|[^0-9A-Za-z_\\-\\[\\]])" + // 匹配字符串开始，或者任何不能使用在osu! username的字符，或者空格（不能使用在username开头）
        "[0-9A-Za-z_\\-\\[\\]][0-9A-Za-z_\\-\\[\\] ]{1,}[0-9A-Za-z_\\-\\[\\]]" + // 匹配ID中可以使用的字符，早期用户没有ID长度限制
        "(?=$|[^0-9A-Za-z_\\-\\[\\]])"; // 匹配字符串结束，或者不能使用在osu! username的字符，或者空格（不能使用在username结尾）
        /// <summary>
        /// 可以匹配到用户名的模式。
        /// </summary>
        public const string UsernamePattern = "[0-9A-Za-z_\\-\\[\\]][0-9A-Za-z_\\-\\[\\] ]{1,13}[0-9A-Za-z_\\-\\[\\]]"; // 匹配ID中可以使用的字符，其中ID的长度是3-15
        /// <summary>
        /// 判断是不是用户名的模式。
        /// </summary>
        private const string IsUsernamePattern = "^" + UsernamePattern + "$";
        /// <summary>
        /// 表示用户名边界的模式。
        /// </summary>
        private const string BorderPattern = "[^0-9A-Za-z_\\-\\[\\]]";
        private static readonly Regex IsUsernameRegex = new Regex(IsUsernamePattern, RegexOptions.Compiled);
        private static readonly Regex DiscoverRegex = new Regex(DiscoverPattern, RegexOptions.Compiled);

        public static bool IsUsername(string s) => IsUsernameRegex.IsMatch(s);

        public static IEnumerable<string> DiscoverUsernames(string s) => DiscoverRegex.Matches(s).Select(m => m.Value).ToList();
    }
}
