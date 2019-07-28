using System.Threading.Tasks;
using WebApiClient;
using WebApiClient.Attributes;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{
    /// <summary>
    /// 民生银行信用卡汇率。
    /// </summary>
    interface ICmbcCreditRate : IHttpApi
    {
        [HttpGet("https://creditcard.cmbc.com.cn/fe//op_exchange_rate/list.gsp")]
        [JsonReturn]
        Task<CmbcRate> GetRates();
    }
}
