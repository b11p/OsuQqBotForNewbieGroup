using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Component("ip")]
    public class IP : Service, IMessageCommand
    {
        private static readonly Regex s_regex = new Regex(@"^\s*ip\s+(?<ip>\S+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var result = await IPLocation.IPLocator.Default.GetLocationAsync(_address).ConfigureAwait(false);
            await api.SendMessageAsync(context.Endpoint, result switch
            {
                (false, _) => "查询失败。",
                (true, null) => "未找到结果。",
                (true, var l) => l.ToString(),
            }).ConfigureAwait(false);
        }

        private IPAddress _address;

        [Parameter("ip")]
        public string IPString { get; set; }

        public bool ShouldResponse(MessageContext context)
        {
            return context.Content.TryGetPlainText(out string text) && RegexCommand(s_regex, text)
                && IPAddress.TryParse(IPString, out _address);
        }
    }
}
