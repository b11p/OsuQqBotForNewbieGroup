using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Mahjong;

[Component("mahjong")]
class MahjongSoulAnalyzer : IMessageCommand
{
    private const string TensoulBase = "https://tensoul.b11p.com/convert";
    private static readonly Regex s_regex = new(@"^\s*雀魂\s+(.+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private string _recordId;

    public async Task ProcessAsync(MessageContext message, HttpApiClient api)
    {
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
        var targetActor = JsonSerializer.Deserialize<TargetCheckClass>(haifuBytes)?.TargetActor;
        if (targetActor == null)
        {
            await api.SendMessageAsync(message.Endpoint, "请确保传入了正确的雀魂牌谱ID。").ConfigureAwait(false);
            return;
        }

        await api.SendMessageAsync(message.Endpoint, "检测通过，功能施工中。").ConfigureAwait(false);
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
    }
}
