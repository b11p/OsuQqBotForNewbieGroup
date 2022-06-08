using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Mahjong;

#nullable enable
[Component("mahjong")]
class MahjongSoulAnalyzer : IMessageCommand
{
    private const string TensoulBase = "https://tensoul.b11p.com/convert";
    private static readonly Regex s_regex = new(@"^\s*雀魂\s+(.+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly IMajsoulAnalyzer s_akochanAnalyzer = new LocalAkochanReviewer(
        "/akochan-reviewer",
        "/akochan-reviewer/target/release/akochan-reviewer");
    private static readonly MahjongObjectStorage s_storage = new MahjongObjectStorage(
        "/outputs",
        "https://res.bleatingsheep.org/");
    private static readonly MajsoulDanPTProvider s_danPTProvider = new MajsoulDanPTProvider();

    private string _recordId = string.Empty;

    public async Task ProcessAsync(MessageContext message, HttpApiClient api)
    {
        // check if record analyzation already exists
        if (await s_storage.GetUriAsync(_recordId + ".html").ConfigureAwait(false) != null)
        {
            await api.SendMessageAsync(message.Endpoint, "雀魂记录 {_recordId} 已经被分析过了。").ConfigureAwait(false);
            return;
        }
        // check if analyzer busy
        if (!s_akochanAnalyzer.IsIdle)
        {
            await api.SendMessageAsync(message.Endpoint, "当前可用的分析资源都在占用中，请稍后再试。").ConfigureAwait(false);
            return;
        }

        var tensoulUri = new UriBuilder(TensoulBase);
        var query = HttpUtility.ParseQueryString(tensoulUri.Query);
        query.Add("id", _recordId);
        tensoulUri.Query = query.ToString();
        using var httpClient = new HttpClient();
        var tensoulResponse = await httpClient.GetAsync(tensoulUri.Uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        if (!tensoulResponse.IsSuccessStatusCode)
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

        var danPt = s_danPTProvider.GetPTList(targetCheckClass!.Dan[targetActor.Value], targetCheckClass.Rule.Disp, targetCheckClass.Dan.Length);
        var resultHtml = await s_akochanAnalyzer.AnalyzeAsync(haifuBytes, targetActor.Value, danPt, 0.15).ConfigureAwait(false);
        var resultUri = await s_storage.PutFileAsync(_recordId + ".html", resultHtml).ConfigureAwait(false);
        if (resultUri == null)
        {
            await api.SendMessageAsync(message.Endpoint, "保存分析结果失败，请稍后再试。").ConfigureAwait(false);
            return;
        }
        await api.SendMessageAsync(message.Endpoint, $"雀魂记录 {_recordId} 分析完成，结果已经保存在 {resultUri} 中。").ConfigureAwait(false);
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