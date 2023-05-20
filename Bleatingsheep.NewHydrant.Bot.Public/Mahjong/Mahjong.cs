using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Bleatingsheep.NewHydrant.Attributions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Mahjong;

#nullable enable
[Component("mahjong")]
class MahjongSoulAnalyzer : IMessageCommand
{
    private static readonly Regex s_regex = new(@"^\s*雀魂\s*([0-9a-zA-Z-_]+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly IMajsoulAnalyzer s_akochanAnalyzer = new RemoteAkochanReviewer();
    private static readonly MahjongObjectStorage s_storage = new MahjongObjectStorage(
        "/outputs",
        "https://res.bleatingsheep.org/");
    private static readonly MajsoulDanPTProvider s_danPTProvider = new MajsoulDanPTProvider();
    private readonly IConfiguration _configuration;
    private readonly ILogger<MahjongSoulAnalyzer> _logger;
    private string _recordId = string.Empty;

    public MahjongSoulAnalyzer(IConfiguration configuration, ILogger<MahjongSoulAnalyzer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ProcessAsync(MessageContext message, HttpApiClient api)
    {
        // check if record analyzation already exists
        if (await s_storage.GetUriAsync(_recordId + ".html").ConfigureAwait(false) != null)
        {
            await api.SendMessageAsync(message.Endpoint, $"雀魂记录 {_recordId} 已经被分析过了。").ConfigureAwait(false);
            return;
        }
        // check if analyzer busy
        if (!s_akochanAnalyzer.IsIdle)
        {
            await api.SendMessageAsync(message.Endpoint, "当前可用的分析资源都在占用中，请稍后再试。").ConfigureAwait(false);
            return;
        }

        var tensoulBase = _configuration.GetSection(MahjongOptions.Mahjong).Get<MahjongOptions>()?.TensoulBase;
        if (tensoulBase == null)
        {
            _logger.LogError("未找到正确的 TensoulBase 配置");
            return;
        }
        var tensoulUri = new UriBuilder(tensoulBase);
        var query = HttpUtility.ParseQueryString(tensoulUri.Query);
        query.Add("id", _recordId);
        tensoulUri.Query = query.ToString();
        using var httpClient = new HttpClient();
        var tensoulTask = httpClient.GetAsync(tensoulUri.Uri, HttpCompletionOption.ResponseHeadersRead);
        await api.SendMessageAsync(message.Endpoint, "正在获取牌谱，请稍候。").ConfigureAwait(false);
        HttpResponseMessage tensoulResponse_;
        try
        {
            tensoulResponse_ = await tensoulTask.ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            await api.SendMessageAsync(message.Endpoint, "获取牌谱超时，请稍候再试。").ConfigureAwait(false);
            return;
        }
        using var tensoulResponse = tensoulResponse_;
        if ((nint)tensoulResponse.StatusCode is >= 500 and < 600)
        {
            // 5xx
            await api.SendMessageAsync(message.Endpoint, "获取牌谱失败，请联系开发者处理。").ConfigureAwait(false);
            return;
        }
        else if (!tensoulResponse.IsSuccessStatusCode)
        {
            await api.SendMessageAsync(message.Endpoint, "请确保传入了正确的雀魂牌谱ID。").ConfigureAwait(false);
            return;
        }
        var haifuBytes = await tensoulResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        TargetCheckClass? targetCheckClass = JsonSerializer.Deserialize<TargetCheckClass>(haifuBytes);
        var targetActor = targetCheckClass?.TargetActor;
        if (targetActor == null)
        {
            await api.SendMessageAsync(message.Endpoint, "请确保传入了正确的雀魂牌谱ID。").ConfigureAwait(false);
            return;
        }

        int[] danPt;
        try
        {
            danPt = s_danPTProvider.GetPTList(targetCheckClass!.Dan[targetActor.Value], targetCheckClass.Rule.Disp, targetCheckClass.Dan.Length);
        }
        catch (Exception e)
        {
            await api.SendMessageAsync(message.Endpoint, "不支持该等级：" + e.Message).ConfigureAwait(false);
            return;
        }

        await api.SendMessageAsync(message.Endpoint, $"即将开始分析，预计15分钟后可以在 https://tx.b11p.com:1443/{_recordId}.html 查看。").ConfigureAwait(false);
        var resultHtml = await s_akochanAnalyzer.AnalyzeAsync(haifuBytes, targetActor.Value, danPt, 0.1, _recordId).ConfigureAwait(false);
        var resultUri = await s_storage.PutFileAsync(_recordId + ".html", resultHtml).ConfigureAwait(false);
        if (resultUri == null)
        {
            await api.SendMessageAsync(message.Endpoint, "保存分析结果失败，请稍后再试。").ConfigureAwait(false);
            return;
        }
        var atPrefix = (message.Endpoint is Sisters.WudiLib.Posts.GroupEndpoint ? Message.At(message.UserId) + " " : "");
        await api.SendMessageAsync(message.Endpoint, atPrefix + $"雀魂记录 {_recordId} 分析完成，结果已经保存在 {resultUri} 中。").ConfigureAwait(false);
    }

    public bool ShouldResponse(MessageContext message)
    {
        if (!message.Content.TryGetPlainText(out string text))
        {
            return false;
        }
        var match = s_regex.Match(text);
        if (!match.Success)
        {
            return false;
        }
        _recordId = match.Groups[1].Value;
        return true;
    }

    private sealed class TargetCheckClass
    {
        [JsonPropertyName("_target_actor")]
        public int? TargetActor { get; set; }

        [JsonPropertyName("dan")]
        public string[] Dan { get; set; } = default!;

        [JsonPropertyName("rule")]
        public Rule Rule { get; set; } = default!;
    }

    private sealed class Rule
    {
        [JsonPropertyName("disp")]
        public string Disp { get; set; } = default!;
    }
}
#nullable restore