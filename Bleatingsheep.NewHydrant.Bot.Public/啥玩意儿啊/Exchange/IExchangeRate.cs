using WebApiClient;
using WebApiClient.Attributes;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{
    [HttpHost("https://api.exchangerate-api.com/v4/")]
    public interface IExchangeRate : IHttpApi
    {
        [HttpGet("latest/{base}")]
        [Cache(20 * 60_000)]
        ITask<ExchangeResponse> GetExchangeRates(string @base);
    }
}
