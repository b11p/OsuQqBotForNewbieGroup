using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Extentions;
using HtmlAgilityPack;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("pixiv")]
    public class Pixiv : IMessageCommand
    {
        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            string url = "https://rsshub.app/pixiv/ranking/day";
            var xmlReader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(xmlReader);
            var imgNode = feed.Items.Select(i =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(i.Summary.Text);
                return doc.DocumentNode.SelectNodes("//p/img");
            }).Randomize().FirstOrDefault(nc => nc.Count == 1)?.First();
            if (imgNode != null)
            {
                var imgUrl = imgNode.Attributes["src"].Value;
                await api.SendMessageAsync(context.Endpoint, Message.NetImage(imgUrl));
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
