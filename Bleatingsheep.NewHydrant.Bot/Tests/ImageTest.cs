using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Tests
{
    [Component("img")]
    public class ImageTest : Service, IMessageCommand
    {
        private static readonly Regex s_regex = new Regex(@"^image (?<url>.*)$", RegexOptions.Compiled);

        [Parameter("url")]
        public string Url { get; set; }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            Logger.Debug($"开始读取 URL {Url} ");
            using (var httpClient = new HttpClient())
            {
                var data = await httpClient.GetByteArrayAsync(Url);
                Logger.Debug($"取到 {data.Length} 字节数据。");
                var sendResponse = await api.SendMessageAsync(context.Endpoint, Message.ByteArrayImage(data));
                Logger.Debug($"发送结果：消息 ID {sendResponse?.MessageId.ToString(CultureInfo.InvariantCulture) ?? "null"}");
            }
        }

        public bool ShouldResponse(MessageContext context)
            => context.UserId == 962549599 && RegexCommand(s_regex, context.Content);
    }
}
