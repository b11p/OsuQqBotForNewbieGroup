using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    private static string EncodeFileName(string fileName)
    {
        var encoded = HttpUtility.UrlEncode(fileName); // UrlPathEncode won't replace characters like '?' '#'
        return encoded.Replace("+", "%20", StringComparison.Ordinal); // UrlPathEncode replace space ' ' with '+'. However, "%20" is expected.
    }

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
        if (string.IsNullOrWhiteSpace(_command))
        {
            _logger.LogWarning("Raw message: {raw}", context.Content.Raw);
            _logger.LogWarning("Sections: {sections}", JsonConvert.SerializeObject(context.Content.Sections));
            _logger.LogWarning("Merged sections: {mergedSections}", JsonConvert.SerializeObject(context.Content.MergeContinuousTextSections().Sections));
            return;
        }

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

        // 获取推送信息
        await using var db = _dbContextFactory.CreateDbContext();
        if (context is not GroupMessage g)
        {
            Debug.Fail(null);
            return;
        }
        // this is null-safe. Dispose() is only called on non-null objects
        // https://stackoverflow.com/questions/2522822/will-dispose-be-called-in-a-using-statement-with-a-null-object
        using var info = await db.BotGroupFields.AsNoTracking().FirstOrDefaultAsync(f => f.GroupId == g.GroupId && f.FieldName == "MemePostInformation");
        if (info?.Data is null || info.Data.Deserialize<MemePostInformation>() is not MemePostInformation pushData)
        {
            // for now, silently return.
            _logger.LogInformation("Group {GroupId} tried to post, but no information found.", g.GroupId);
            return;
        }

        _logger.LogInformation("Run command /post {fileName}, to group {groupId}", fileName, info.GroupId);

        // 获取图片
        var url = await CitedImageUrlUtility.GetCitedImageUrlAsync(context, api, _logger);
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
            if (ext is null || !GetExtensionNameCheckingRegex().IsMatch(ext))
            {
                await api.SendMessageAsync(g.Endpoint, $"图片格式{ext}未知，请联系开发者并提供图片。");
                return;
            }

            var createFile = await gitHubClient.Repository.Content.CreateFile(pushData.Repository.Owner, pushData.Repository.Name, Path.Combine(pushData.Path, $"{EncodeFileName(fileName)}.{ext}"), new CreateFileRequest($"Bot upload. Group {g.GroupId}, User {g.UserId}", Convert.ToBase64String(imageBytes), false));
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
        var sb = new StringBuilder();
        sb.Append($"推送图片成功。{fileName}.{ext}");
        if (pushData.HomePage is not null)
        {
            sb.AppendLine();
            sb.Append($"更多精彩尽在 {pushData.HomePage}");
        }

        await api.SendMessageAsync(g.Endpoint, sb.ToString());
    }

    public bool ShouldResponse(MessageContext context)
    {
        var regex = GetCommandRegex();
        var command = context switch
        {
            GroupMessage g => g.Content.MergeContinuousTextSections().Sections.Where(s => s.Type != Section.TextType || !string.IsNullOrWhiteSpace(s.Data[Section.TextParamName])).ToList() switch
            {
                [{ Type: "reply" }, { Type: "at" }, { Type: "at" }, { Type: "text" } s, ..] => s.Data["text"],
                [{ Type: "reply" }, { Type: "at" }, { Type: "text" } s, ..] => s.Data["text"],
                [{ Type: "reply" }, { Type: "text" } s, ..] => s.Data["text"],
                _ => default,
            },
            _ => default,
        };
        if (string.IsNullOrWhiteSpace(command))
        {
            if (context.Content.Raw.Contains("/post", StringComparison.InvariantCultureIgnoreCase))
            {
                // only for debug.
                return true;
            }
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
    [GeneratedRegex(@"^\s*/post\s*(.*?)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline, "zh-CN")]
    private static partial Regex GetParsingRegex();
    [GeneratedRegex(@"^(?:jpg|png|jfif|webp|gif)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex GetExtensionNameCheckingRegex();
}
