using Sisters.WudiLib.Posts;
using Sisters.WudiLib;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant;
#nullable enable
public static class CitedImageUrlUtility
{
    public static async ValueTask<string?> GetCitedImageUrlAsync(MessageContext context, HttpApiClient api, ILogger logger)
    {
        // 获取图片
        if (!context.Content.Sections[0].Data.TryGetValue("id", out var strMessageId) || !int.TryParse(strMessageId, out int messageId))
        {
            logger.LogError("获取消息 ID 失败，引用消息 ID {MessageId}.", context.MessageId);
            await api.SendMessageAsync(context.Endpoint, "获取消息 ID 失败，可能需要重新发送图片。");
            return null;
        }
        var messageResponse = await api.GetMessage(messageId);
        if (messageResponse?.Message is not ReceivedMessage message)
        {
            logger.LogError("获取消息失败，消息 ID：{messageId}", messageId);
            await api.SendMessageAsync(context.Endpoint, "获取消息内容失败，可能需要重新发送图片。");
            return null;
        }
        if (message.Sections is not [{ Type: "image" } s])
        {
            await api.SendMessageAsync(context.Endpoint, "引用的消息不是单张图片，请重新选择。");
            return null;
        }
        if (!s.Data.TryGetValue("url", out var url))
        {
            await api.SendMessageAsync(context.Endpoint, "获取图片 URL 失败。");
            return null;
        }
        return url;
    }
}