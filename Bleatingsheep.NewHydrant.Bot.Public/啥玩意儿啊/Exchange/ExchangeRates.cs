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
    [Component("ex_rates")]
    public class ExchangeRates : Service, IMessageCommand
    {
        static ExchangeRates()
        {
            HttpApi.Register<IExchangeRate>();
            HttpApi.Register<ICmbcCreditRate>();
            HttpApi.Register<ICibRate>();
            HttpApi.Register<IMasterCardRate>();
            HttpApi.Register<ICmbRateProvider>();
        }

        private static readonly Regex s_regex = new Regex(@"^\s*汇率\s*([A-Za-z]{3})\s*(\d*\.?\d*)\s*$", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_regex2 = new Regex(@"^\s*汇率\s*(\d*\.?\d*)\s*([A-Za-z]{3})\s*$", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly IReadOnlyList<string> s_currencies = new List<string>
        {
            "CNY",
            "JPY",
            "USD",
        }.AsReadOnly();

        private string _text;

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            string @base;
            string amountString;

            if (s_regex.Match(_text) is { Success: true } match)
            {
                @base = match.Groups[1].Value;
                amountString = match.Groups[2].Value;
            }
            else if (s_regex2.Match(_text) is { Success: true } match2)
            {
                @base = match2.Groups[2].Value;
                amountString = match2.Groups[1].Value;
            }
            else
            {
                return;
            }

            if (!decimal.TryParse(amountString, out decimal amount))
            {
                await api.SendMessageAsync(context.Endpoint, "数字格式错误。").ConfigureAwait(false);
                return;
            }

            using var exRateApi = HttpApi.Resolve<IExchangeRate>();
            try
            {
                checked
                {
                    // cmbc
                    var masterCardTask = string.Equals("USD", @base, StringComparison.OrdinalIgnoreCase)
                        ? Task.FromResult<MasterCardRate>(default)
                        : HttpApi.Resolve<IMasterCardRate>().GetRateToUsd(@base);
                    var cmbcTask = HttpApi.Resolve<ICmbcCreditRate>().GetRates();

                    var cmbTask = HttpApi.Resolve<ICmbRateProvider>().GetRatesInCode();

                    // cib
                    var cibTask = HttpApi.Resolve<ICibRate>().GetRates();

                    // boc
                    //var bocTask = BocRateClient.GetExchangeSellingRateAsync(@base);

                    var response = await exRateApi.GetExchangeRates(@base).ConfigureAwait(false);
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

                    MasterCardRate masterCardResult = default;

                    //cmbc
                    try
                    {
                        var cmbcResult = await cmbcTask.ConfigureAwait(false);
                        if (string.Equals("USD", @base, StringComparison.OrdinalIgnoreCase))
                        {
                            var price = cmbcResult?.Data?.FirstOrDefault(d => string.Equals(@base, d?.Remark, StringComparison.OrdinalIgnoreCase))?.Price;
                            if (price != null)
                            {
                                var cny = amount * price.Value;
                                results.Add($"CMBC CNY {cny}");
                            }
                        }
                        else
                        {
                            masterCardResult = await masterCardTask.ConfigureAwait(false);
                            var usdRate = masterCardResult?.Data?.ConversionRate;
                            var usdPrice = cmbcResult?.Data?.FirstOrDefault(d => string.Equals("USD", d?.Remark, StringComparison.OrdinalIgnoreCase))?.Price;
                            if (usdPrice != null && usdRate != null)
                            {
                                var cny = amount * usdRate.Value * usdPrice.Value;
                                results.Add($"MasterCard USD CMBC CNY {cny}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        results.Add("CMBC 查询失败。");
                        Logger.Error(e);
                    }

                    // cmb
                    try
                    {
                        var cmbResult = await cmbTask.ConfigureAwait(false);
                        if (string.Equals("USD", @base, StringComparison.OrdinalIgnoreCase))
                        {
                            var price = cmbResult?.GetValueOrDefault(@base.ToUpperInvariant());
                            if (price != null)
                            {
                                var cny = amount * price.Value;
                                results.Add($"CMB CNY {cny}");
                            }
                        }
                        else
                        {
                            var usdRate = masterCardResult?.Data?.ConversionRate;
                            var usdPrice = cmbResult?.GetValueOrDefault("USD");
                            if (usdPrice != null && usdRate != null)
                            {
                                var cny = amount * usdRate.Value * usdPrice.Value;
                                results.Add($"MasterCard USD CMB CNY {cny}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        results.Add("CMB 查询失败。");
                        Logger.Error(e);
                    }

                    // cib
                    try
                    {
                        var cibResult = await cibTask.ConfigureAwait(false);

                        var price = cibResult[@base];
                        if (price != null)
                        {
                            var cny = amount * price.Value;
                            results.Add($"CIB CNY {cny}");
                        }
                    }
                    catch (Exception e)
                    {
                        results.Add("CIB 查询失败。");
                        Logger.Error(e);
                    }

                    // boc
                    //try
                    //{
                    //    var bocResult = await bocTask.ConfigureAwait(false);
                    //    if (bocResult != null)
                    //    {
                    //        results.Add($"BOC CNY {bocResult * amount}");
                    //    }
                    //}
                    //catch (Exception e)
                    //{
                    //    results.Add("BOC 查询失败。");
                    //    Logger.Error(e);
                    //}

                    await api.SendMessageAsync(context.Endpoint, string.Join("\r\n", results)).ConfigureAwait(false);
                }
            }
            catch (OverflowException)
            {
                await api.SendMessageAsync(context.Endpoint, "数值过大或过小").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await api.SendMessageAsync(context.Endpoint, "查询汇率失败。").ConfigureAwait(false);
                Logger.Error(e);
            }
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context.Content.TryGetPlainText(out _text)
                && _text.TrimStart().StartsWith("汇率", StringComparison.InvariantCulture);
        }
    }
}
