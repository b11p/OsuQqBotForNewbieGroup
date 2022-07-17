using System.Threading.Tasks;
using Newtonsoft.Json;
using WebApiClient;
using WebApiClient.Attributes;
using WebApiClient.DataAnnotations;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange;
#nullable enable annotations
public interface IMasterCardRate : IHttpApi
{

    [HttpGet("https://www.mastercard.com/settlement/currencyrate/conversion-rate?fxDate=0000-00-00&bankFee=0&transAmt=1")]
    [JsonReturn]
    [Cache(19 * 60_000)]
    Task<MasterCardRate> GetRate([AliasAs("transCurr")][PathQuery] string from, [AliasAs("crdhldBillCurr")][PathQuery] string to);
}

public static class MasterCardRateExtensions
{
    public static Task<MasterCardRate> GetRateToUsd(this IMasterCardRate rate, string from)
    {
        return rate.GetRate(from, "USD");
    }
}

public class MasterCardRateData
{
    [JsonProperty("conversionRate")]
    public decimal ConversionRate { get; set; }

    [JsonProperty("crdhldBillAmt")]
    public decimal CrdhldBillAmt { get; set; }

    [JsonProperty("fxDate")]
    public string FxDate { get; set; }

    [JsonProperty("transCurr")]
    public string TransCurr { get; set; }

    [JsonProperty("crdhldBillCurr")]
    public string CrdhldBillCurr { get; set; }

    [JsonProperty("transAmt")]
    public int TransAmt { get; set; }
}

public class MasterCardRate
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("data")]
    public MasterCardRateData Data { get; set; }
}
#nullable restore