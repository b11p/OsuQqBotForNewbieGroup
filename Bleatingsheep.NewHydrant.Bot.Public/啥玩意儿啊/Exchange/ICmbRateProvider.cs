using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiClient;
using WebApiClient.Attributes;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange;
#nullable enable annotations
public interface ICmbRateProvider : IHttpApi
{
    [HttpGet("https://m.cmbchina.com/api/rate/getfxrate")]
    [JsonReturn]
    [Cache(19 * 60_000)]
    Task<CmbRateResponse> GetRates();
}

public class CmbRateData
{
    [JsonProperty("ZCcyNbr")]
    public string ZCcyNbr { get; set; }

    [JsonProperty("ZRtbBid")]
    public string ZRtbBid { get; set; }

    /// <summary>
    /// 现汇卖出价（银行->客户）
    /// </summary>
    [JsonProperty("ZRthOfr")]
    public string ZRthOfr { get; set; }

    [JsonProperty("ZRtcOfr")]
    public string ZRtcOfr { get; set; }

    [JsonProperty("ZRthBid")]
    public string ZRthBid { get; set; }

    [JsonProperty("ZRtcBid")]
    public string ZRtcBid { get; set; }

    [JsonProperty("ZRatTim")]
    public string ZRatTim { get; set; }

    [JsonProperty("ZRatDat")]
    public string ZRatDat { get; set; }

    [JsonProperty("ZCcyExc")]
    public string ZCcyExc { get; set; }
}

public class CmbRateResponse
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("data")]
    public List<CmbRateData> Data { get; set; }

    [JsonProperty("ctime")]
    public string Ctime { get; set; }
}

public static class CmbRateProviderExtensions
{
    private static IReadOnlyDictionary<string, string> s_chineseToCodeMap = new Dictionary<string, string>
    {
        ["人民币"] = "CNY",
        ["美元"] = "USD",
        ["欧元"] = "EUR",
        ["英镑"] = "GBP",
        ["港币"] = "HKD",
        ["日元"] = "JPY",
        ["澳大利亚元"] = "AUD",
        ["加拿大元"] = "CAD",
        // ["澳门元"] = "MOP",
        ["瑞士法郎"] = "CHF",
        // ["瑞典克朗"] = "SEK",
        // ["丹麦克朗"] = "DKK",
        // ["挪威克朗"] = "NOK",
        // ["韩元"] = "KRW",
        ["新加坡元"] = "SGD",
        // ["泰铢"] = "THB",
        ["新西兰元"] = "NZD",
        // ["马来西亚林吉特"] = "MYR",
        // ["菲律宾比索"] = "PHP",
        // ["印尼卢比"] = "IDR",
        // ["越南盾"] = "VND",
        // ["柬埔寨瑞尔"] = "KHR",
        // ["文莱元"] = "BND",
        // ["印度卢比"] = "INR",
        // ["巴基斯坦卢比"] = "PKR",
        // ["阿联酋迪拉姆"] = "AED",
        // ["新台币"] = "TWD",
    };

    public static async Task<Dictionary<string, decimal>> GetRatesInCode(this ICmbRateProvider provider)
    {
        var response = await provider.GetRates().ConfigureAwait(false);
        if (response.Status != 0)
        {
            throw new Exception("获取汇率失败");
        }
        var result = new Dictionary<string, decimal>();
        foreach (var data in response.Data)
        {
            if (s_chineseToCodeMap.TryGetValue(data.ZCcyNbr, out var code))
            {
                result[code] = decimal.Parse(data.ZRthOfr);
            }
        }
        return result;
    }
}
#nullable restore