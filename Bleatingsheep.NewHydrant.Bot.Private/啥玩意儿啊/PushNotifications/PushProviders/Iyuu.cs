using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.PushNotifications.PushProviders;
#nullable enable
internal class Iyuu : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public Iyuu(HttpClient httpClient, string token)
    {
        _httpClient = httpClient;
        _token = token;
    }

    public async ValueTask Push(string text, string desp)
    {
        var args = new { text, desp };
        await _httpClient.PostAsJsonAsync($"https://iyuu.cn/{_token}.send", args).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
#nullable restore