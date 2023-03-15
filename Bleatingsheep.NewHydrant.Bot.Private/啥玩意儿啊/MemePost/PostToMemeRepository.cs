using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octokit;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using SixLabors.ImageSharp;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.MemePost;
#nullable enable
[Component("post_meme")]
internal partial class PostToMemeRepository : IMessageCommand
{
    private static readonly char[] s_invalidFileNameChars = Path.GetInvalidFileNameChars();

    private readonly IDbContextFactory<NewbieContext> _dbContextFactory;
    private readonly ILogger<PostToMemeRepository> _logger;

    public PostToMemeRepository(IDbContextFactory<NewbieContext> dbContextFactory, ILogger<PostToMemeRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    private string _command = default!;

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        // 获取文件名
        var regex = GetParsingRegex();
        var command = _command.Trim();
        var match = regex.Match(command);
        Debug.Assert(match.Success);
        if (match.Length != command.Length || string.IsNullOrWhiteSpace(match.Groups[1].Value))
        {
            // 可能是有多行，或者未填写文件名。
            await api.SendMessageAsync(context.Endpoint, "命令格式错误，正确格式为“/post 标签”");
            return;
        }
        var fileName = match.Groups[1].Value;
        if (fileName.IndexOfAny(s_invalidFileNameChars) != -1)
        {
            await api.SendMessageAsync(context.Endpoint, "命令格式错误，正确格式为“/post 标签”，标签必须可以用作文件名。");
            return;
        }

        // 获取推送信息
        await using var db = _dbContextFactory.CreateDbContext();
        if (context is not GroupMessage g)
        {
            Debug.Fail(null);
            return;
        }
        var info = await db.BotGroupFields.AsNoTracking().FirstOrDefaultAsync(f => f.GroupId == g.GroupId && f.FieldName == "MemePostInformation");
        if (info?.Data is null || info.Data.Deserialize<MemePostInformation>() is not MemePostInformation pushData)
        {
            // for now, silently return.
            _logger.LogInformation("Group {GroupId} tried to post, but no information found.", g.GroupId);
            return;
        }

        _logger.LogInformation("Run command /post {fileName}, to group {groupId}", fileName, info.GroupId);

        // 获取图片
        if (!g.Content.Sections[0].Data.TryGetValue("id", out var strMessageId) || !int.TryParse(strMessageId, out int messageId))
        {
            _logger.LogError("获取消息 ID 失败，引用消息 ID {MessageId}.", g.MessageId);
            await api.SendMessageAsync(context.Endpoint, "获取消息 ID 失败，可能需要重新发送图片。");
            return;
        }
        var messageResponse = await api.GetMessage(messageId);
        if (messageResponse?.Message is not ReceivedMessage message)
        {
            _logger.LogError("获取消息失败，消息 ID：{messageId}", messageId);
            await api.SendMessageAsync(context.Endpoint, "获取消息内容失败，可能需要重新发送图片。");
            return;
        }
        if (message.Sections is not [{ Type: "image" } s])
        {
            await api.SendMessageAsync(context.Endpoint, "引用的消息不是单张图片，请重新选择。");
            return;
        }
        if (!s.Data.TryGetValue("url", out var url))
        {
            await api.SendMessageAsync(context.Endpoint, "获取图片 URL 失败。");
            return;
        }
        string? ext;
        try
        {
            using var httpClient = new HttpClient();
            _logger.LogInformation("Image url: {url}", url);
            var imageBytes = await httpClient.GetByteArrayAsync(url);
            var gitHubClient = new GitHubClient(new ProductHeaderValue("LoadBalancerScripts"))
            {
                Credentials = new Credentials(pushData.GitHubToken),
            };

            // get file extension.
            var imageFormat = Image.DetectFormat(imageBytes);
            ext = imageFormat.FileExtensions.FirstOrDefault();

            var createFile = await gitHubClient.Repository.Content.CreateFile(pushData.Repository.Owner, pushData.Repository.Name, Path.Combine(pushData.Path, $"{fileName}.{ext}"), new CreateFileRequest("Bot upload", Convert.ToBase64String(imageBytes), false));
        }
        catch (ImageFormatException e)
        {
            _logger.LogError(e, "图片格式检测失败，URL：{url}", url);
            await api.SendMessageAsync(g.Endpoint, "图片格式检测失败，请联系开发者并提供图片。");
            return;
        }
        catch (ApiValidationException e)
        {
            _logger.LogError(e, "推送图片时出现错误。");
            await api.SendMessageAsync(g.Endpoint, $"推送图片时出现错误，可能是已有同名文件。\r\n{e.Message}");
            return;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "推送图片时出现错误。");
            await api.SendMessageAsync(g.Endpoint, $"推送图片时出现错误：{e.Message}");
            return;
        }

        _logger.LogInformation("Post complete");
        await api.SendMessageAsync(g.Endpoint, $"推送图片成功。{fileName}.{ext}");
    }

    public bool ShouldResponse(MessageContext context)
    {
        var regex = GetCommandRegex();
        var command = context switch
        {
            GroupMessage g => g switch
            {
                { Content.Sections: [{ Type: "reply" }, { Type: "text" } s, ..] } => s.Data["text"],
                { Content.Sections: [{ Type: "reply" }, { Type: "at" }, { Type: "text" } s, ..] } => s.Data["text"],
                _ => default,
            },
            _ => default,
        };
        if (string.IsNullOrWhiteSpace(command))
        {
            return false;
        }
        if (regex.IsMatch(command))
        {
            _command = command;
            return true;
        }
        return false;
    }

    [GeneratedRegex(@"^\s*/post(?:\s|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetCommandRegex();
    [GeneratedRegex("^\\s*/post\\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
    private static partial Regex GetParsingRegex();
}
