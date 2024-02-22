using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Sisters.WudiLib;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊;
#nullable enable
[Component(nameof(真随机数))]
internal partial class 真随机数 : IMessageCommand
{
    private Match _match = default!;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;

    public 真随机数(IMemoryCache memoryCache, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _configuration = configuration;
    }

    public async Task ProcessAsync(MessageContext context, HttpApiClient api)
    {
        int start = 0;
        int end = 100;
        var parameterString = _match.Groups[1].Value;
        if (!string.IsNullOrWhiteSpace(parameterString))
        {
            var splits = parameterString.Split();
            if (splits.Length >= 3)
            {
                goto failed;
            }
            if (!int.TryParse(splits[0], out var num1))
            {
                goto failed;
            }
            int num2 = default;
            if (splits.Length == 1 && num1 <= 1)
            {
                goto failed;
            }
            if (splits.Length >= 2 && (!int.TryParse(splits[1], out num2) || num2 <= num1 || num2 - num1 < 1 || num2 == int.MaxValue))
            {
                goto failed;
            }
            goto success;
        failed:
            await api.SendMessageAsync(context.Endpoint, "/roll min max 或 /roll max 或 /roll");
            return;
        success:
            if (splits.Length == 1)
            {
                end = num1;
            }
            if (splits.Length == 2)
            {
                start = num1;
                end = num2 + 1;
            }
        }
        var val = await GetInt32(start, end);
        await api.SendMessageAsync(context.Endpoint, $"通过量子力学为你生成的 [{start}, {end - 1}] 范围内的随机数是 {val}。");
    }

    private async ValueTask<int> GetInt32(int fromInclusive, int toExclusive)
    {
        Debug.Assert(fromInclusive < toExclusive);
        var apiKey = _configuration["Services:random.org"];

        // The total possible range is [0, 4,294,967,295).
        // Subtract one to account for zero being an actual possibility.
        uint range = (uint)toExclusive - (uint)fromInclusive - 1;

        // If there is only one possible choice, nothing random will actually happen, so return
        // the only possibility.
        if (range == 0)
        {
            return fromInclusive;
        }

        using var http = new HttpClient();
        using var response = await http.PostAsJsonAsync("https://api.random.org/json-rpc/4/invoke", new
        {
            jsonrpc = "2.0",
            method = "generateIntegers",
            @params = new
            {
                apiKey,
                n = 1,
                min = 0,
                max = range,
                replacement = true,
            },
            id = RandomNumberGenerator.GetInt32(0x40000000),
        });
        response.EnsureSuccessStatusCode();
        var r = await response.Content.ReadFromJsonAsync<RandomNumberResponse>();
        return (int)r!.Result.Random.Data[0] + fromInclusive;
    }

    public partial class RandomNumberResponse
    {
        [JsonPropertyName("jsonrpc")]
        public required string Jsonrpc { get; set; }

        [JsonPropertyName("result")]
        public required Result Result { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public sealed class Result
    {
        [JsonPropertyName("random")]
        public required RandomData Random { get; set; }

        [JsonPropertyName("bitsUsed")]
        public int BitsUsed { get; set; }

        [JsonPropertyName("bitsLeft")]
        public int BitsLeft { get; set; }

        [JsonPropertyName("requestsLeft")]
        public int RequestsLeft { get; set; }

        [JsonPropertyName("advisoryDelay")]
        public int AdvisoryDelay { get; set; }
    }

    public sealed class RandomData
    {
        [JsonPropertyName("data")]
        public required List<uint> Data { get; set; }

        // This field is of format: 2023-07-23 15:50:01Z
        // However, System.Text.Json expects 2023-07-23T15:50:01Z
        //[JsonPropertyName("completionTime")]
        //public DateTimeOffset CompletionTime { get; set; }
    }

    public bool ShouldResponse(MessageContext context)
    {
        if (!context.Content.TryGetPlainText(out var text))
        {
            return false;
        }

        var regex = CommandRegex();
        var match = regex.Match(text);
        if (match.Success)
        {
            _match = match;
        }
        return match.Success;
    }

    [GeneratedRegex(@"^\s*/\s*roll\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled)]
    private static partial Regex CommandRegex();
}