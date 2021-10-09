using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{
#nullable enable
    public static class BocRateClient
    {
        public static async Task<decimal?> GetExchangeSellingRateAsync(string currencyName)
        {
            string html = await new HttpClient().GetStringAsync("https://www.boc.cn/sourcedb/whpj/sjmfx_1621.html").ConfigureAwait(false);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes("/html/body/article/div/table/tbody/tr");
            var currency = nodes.Select(n => new
            {
                Name = n.SelectSingleNode("td[1]").InnerText,
                ExchangeBuy = n.SelectSingleNode("td[2]").InnerText,
                CashBuy = n.SelectSingleNode("td[3]").InnerText,
                ExchangeSell = n.SelectSingleNode("td[4]").InnerText,
                CashSell = n.SelectSingleNode("td[5]").InnerText,
            }).FirstOrDefault(i => currencyName.Equals(i.Name, StringComparison.OrdinalIgnoreCase));
            return currency is null
                ? null
                : decimal.TryParse((string)currency.ExchangeSell, out var rate)
                ? rate / 100
                : null;
        }
    }
#nullable restore
}
