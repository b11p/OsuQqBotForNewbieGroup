using System;
using System.Text.RegularExpressions;
using Bleatingsheep.OsuMixedApi;
using OsuQqBot.Query;
using Sisters.WudiLib.Posts;

namespace OsuQqBot.AttributedFunctions
{
    [Function]
    class CheckMemberRequest : IMessageCommandable
    {
        private const long ManagingGroupId = 695600319; // 新人群管理群号。
        private long ReporterId => OsuQqBot.Daloubot; // dalou 上报申请用的 QQ 号。
        private const long NewbieGroupId = 614892339;
        private const string Pattern = @"^收到新人群加群申请\r\n群号: (\d+)\r\n群类型: .*?\r\n申请者: (\d+)\r\n验证信息: (.*)$"; // 匹配上报申请的消息。
        private readonly static Regex regex = new Regex(Pattern, RegexOptions.Compiled);

        private Match _match;
        private long GroupId => long.Parse(_match.Groups[1].Value);
        private long UserId => long.Parse(_match.Groups[2].Value);
        private string Message => _match.Groups[3].Value;

        private static string InfoOf(UserInfo userInfo) => userInfo.TextInfo();

        IMessageCommandable IMessageCommandable.Create() => new CheckMemberRequest();

        void IMessageCommandable.Process(Message message, Sisters.WudiLib.HttpApiClient api)
        {
            var names = UsernameUtils.ParseUsername(Message);
            var valid = Querying.Instance.CheckUsername(names, false);
            foreach (var user in valid)
            {
                try
                {
                    api.SendMessageAsync(message.Endpoint, InfoOf(user)).Wait();
                }
                catch (AggregateException) { }
            }
        }

        bool IMessageCommandable.ShouldResponse(Message message)
        {
            if (!(message is GroupMessage groupMessage))
                return false;
            if (groupMessage.GroupId != ManagingGroupId || groupMessage.Source.IsAnonymous || groupMessage.Source.UserId != ReporterId)
                return false;
            var content = message.Content;
            if (!content.IsPlaintext)
                return false;
            _match = regex.Match(content.Text);
            return _match.Success;
        }
    }
}
