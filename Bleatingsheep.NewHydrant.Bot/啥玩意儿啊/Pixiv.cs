using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Extentions;
using HtmlAgilityPack;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("pixiv")]
    public class Pixiv : IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            string url = "https://rss.bleatingsheep.org/pixiv/ranking/day";
            var xmlReader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(xmlReader);
            var tuple = feed.Items.Select(i =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(i.Summary.Text);
                return (item: i, nodes: doc.DocumentNode.SelectNodes("//p/img"));
            }).Randomize().FirstOrDefault(t => t.nodes.Count == 1);
            var (item, imgNode) = (tuple.item, tuple.nodes?.First());
            if (imgNode != null)
            {
                var imgUrl = imgNode.Attributes["src"].Value;
                if (await api.SendMessageAsync(
                    endpoint: context.Endpoint,
                    message: new Message(item.Title.Text + "\r\n")
                        + Message.NetImage(imgUrl)
                        + new Message("\r\n" + item.Links.FirstOrDefault().Uri)
                    ) == null)
                {
                    await api.SendMessageAsync(context.Endpoint, "图片发送失败。");
                }
            }
            else
            {
                await api.SendMessageAsync(context.Endpoint, "没有符合要求的图片。");
            }
        }

        public bool ShouldResponse(MessageContext context)
            => context.Content.TryGetPlainText(out string text) && text.Trim() == "ピクシブ";
    }
}
