using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using WebApiClient;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{
    [Function("ex_rates")]
    public class ExchangeRates : Service, IMessageCommand
    {
        static ExchangeRates()
        {
            HttpApi.Register<IExchangeRate>();
            HttpApi.Register<ICmbcCreditRate>();
        }

        private static readonly IReadOnlyList<string> s_currencies = new List<string>
        {
            "CNY",
            "JPY",
            "USD",
        }.AsReadOnly();

        private string _text;

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            var match = Regex.Match(_text, @"^汇率\s*([A-Za-z]{3})\s*(\d*\.?\d*)$");
            if (!match.Success)
            {
                return;
            }

            var @base = match.Groups[1].Value;
            if (!decimal.TryParse(match.Groups[2].Value, out decimal amount))
            {
                await api.SendMessageAsync(context.Endpoint, "数字格式错误。");
                return;
            }

            var exRateApi = HttpApi.Resolve<IExchangeRate>();
            try
            {
                checked
                {
                    // cmbc
                    var cmbcTask = HttpApi.Resolve<ICmbcCreditRate>().GetRates();

                    var response = await exRateApi.GetExchangeRates(@base);
                    var results = new List<string>(3);
                    foreach (var currency in s_currencies)
                    {
                        if (string.Equals(currency, @base, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                        var rate = response.Rates[currency];
                        results.Add($"{currency} {amount * rate}");
                    }

                    //cmbc
                    try
                    {
                        var cmbcResult = await cmbcTask;
                        var price = cmbcResult?.Data?.First(d => string.Equals(@base, d?.Remark, StringComparison.OrdinalIgnoreCase))?.Price;
                        if (price != null)
                        {
                            var cny = amount * price.Value;
                            results.Add($"CMBC CNY {cny}");
                        }
                    }
                    catch (Exception e)
                    {
                        results.Add("CMBC 查询失败。");
                        Logger.Error(e);
                    }

                    await api.SendMessageAsync(context.Endpoint, string.Join("\r\n", results));
                }
            }
            catch (OverflowException)
            {
                await api.SendMessageAsync(context.Endpoint, "数值过大或过小");
            }
            catch (Exception e)
            {
                await api.SendMessageAsync(context.Endpoint, "查询汇率失败。");
                Logger.Error(e);
            }
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context.Content.TryGetPlainText(out _text)
                && _text.StartsWith("汇率");
        }
    }
}
