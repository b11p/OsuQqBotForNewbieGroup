using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebApiClient;
using WebApiClient.Attributes;
using WebApiClient.DataAnnotations;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{
    internal interface ICibRate : IHttpApi
    {
        [HttpGet("https://personalbank.cib.com.cn/pers/main/pubinfo/ifxQuotationQuery.do")]
        Task<HttpResponseMessage> Prepare();

        [HttpGet("https://personalbank.cib.com.cn/pers/main/pubinfo/ifxQuotationQuery!list.do?_search=false&dataSet.rows=80&dataSet.page=1&dataSet.sidx=&dataSet.sord=asc")]
        [JsonReturn]
        Task<CibRate> GetRates([AliasAs("dataSet.nd")][PathQuery] long timestamp);
    }

    static class CibRateExtensions
    {
        public async static Task<CibRate> GetRates(this ICibRate cibRate)
        {
            await cibRate.Prepare().ConfigureAwait(false);
            return await cibRate.GetRates(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).ConfigureAwait(false);
        }
    }
}
