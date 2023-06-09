using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component("wakatime")]
public class WakaTime : IMessageCommand
{
    private static readonly Regex s_regex = new Regex(@"^\s*(?:开卷\s*(.*?)|谁在卷)\s*$", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private readonly ILogger<WakaTime> _logger;
    private readonly IDbContextFactory<NewbieContext> _contextFactory;

    private string? _url;

    public WakaTime(ILogger<WakaTime> logger, IDbContextFactory<NewbieContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        if (!string.IsNullOrWhiteSpace(_url))
        {
            try
            {
                var uri = new Uri(_url);
                if (!uri.Host.EndsWith("wakatime.com", StringComparison.OrdinalIgnoreCase))
                {
                    await api.SendMessageAsync(context.Endpoint, "请提供来自 wakatime.com 的分享链接。").ConfigureAwait(false);
                }
                _url = uri.AbsoluteUri;
            }
            catch (Exception)
            {
                await api.SendMessageAsync(context.Endpoint, "链接格式错误").ConfigureAwait(false);
            }
        }

        // get user url
        using var db = _contextFactory.CreateDbContext();
        var info = await db.BotUserFields.FirstOrDefaultAsync(x => x.UserId == context.UserId && x.FieldName == "wakatime_url").ConfigureAwait(false);
        // if user url set, do not allow to set again
        if (info != null)
        {
            if (!string.IsNullOrWhiteSpace(_url))
            {
                await api.SendMessageAsync(context.Endpoint, "已经设置过 waka 链接了，请勿重复设置。").ConfigureAwait(false);
                return;
            }
        }
        else
        {
            // Invoke a method to add url info.
            // if (string.IsNullOrWhiteSpace(_url))
            // {
            //     await api.SendMessageAsync(context.Endpoint, "请设置链接").ConfigureAwait(false);
            //     return;
            // }
            // else
            // {
            //     await db.BotUserFields.AddAsync(new BotUserField
            //     {
            //         UserId = context.UserId,
            //         FieldName = "wakatime_url",
            //         FieldValue = _url,
            //     }).ConfigureAwait(false);
            // }
        }
    }

    public bool ShouldResponse(MessageContext context)
    {
        if (context.Content.TryGetPlainText(out var text))
        {
            var match = s_regex.Match(text);
            if (match.Success)
            {
                _url = match.Groups[1].Value;
                return true;
            }
        }
        return false;
    }

    public async ValueTask CreateUserProfileAsync(MessageContext context, HttpApiClient api)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(_url!, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode
            || !response.Headers.TryGetValues("content-type", out var contentType)
            || !contentType.Any(x => x.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)))
        {
            await api.SendMessageAsync(context.Endpoint, "无法获取wakatime信息，请检查链接，稍后再试。").ConfigureAwait(false);
            return;
        }
        await using var stream = response.Content.ReadAsStream();
        var shareContent = await JsonSerializer.DeserializeAsync<WakaShareResponse>(stream).ConfigureAwait(false);
        if (!CheckShareContent(shareContent))
        {
            await api.SendMessageAsync(context.Endpoint, "请检查链接是否正确。").ConfigureAwait(false);
            return;
        }

    }

    public bool CheckShareContent(WakaShareResponse? wakaShareResponse)
    {
        if (wakaShareResponse is null)
        {
            return false;
        }
        // check error
        if (wakaShareResponse.Error != null)
        {
            return false;
        }
        // check scheme
        if (wakaShareResponse.Data == null || wakaShareResponse.Data.Count == 0)
        {
            return false;
        }
        return true;
    }
}

public class WakaDayData
{
    [JsonPropertyName("grand_total")]
    public GrandTotal GrandTotal { get; set; } = default!;

    [JsonPropertyName("range")]
    public Range Range { get; set; } = default!;
}

public class GrandTotal
{
    [JsonPropertyName("decimal")]
    public double Decimal { get; set; }

    [JsonPropertyName("digital")]
    public string Digital { get; set; } = default!;

    [JsonPropertyName("hours")]
    public int Hours { get; set; }

    [JsonPropertyName("minutes")]
    public int Minutes { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = default!;

    [JsonPropertyName("total_seconds")]
    public double TotalSeconds { get; set; }
}

public class Range
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = default!;

    [JsonPropertyName("end")]
    public DateTimeOffset End { get; set; }

    [JsonPropertyName("start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = default!;

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = default!;
}

public class WakaShareResponse
{
    [JsonPropertyName("data")]
    public List<WakaDayData>? Data { get; set; }
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
#nullable restore