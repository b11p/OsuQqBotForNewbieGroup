using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Microsoft.Extensions.Logging;
using Sisters.WudiLib;
using WebApiClient;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{
#nullable enable
    [Component("ExchangeLogger")]
    public class ExchangeLogger : IMessageMonitor
    {
        private static Stopwatch? s_stopwatch;
        private static long s_nextCheck = 0;
        private static string s_cmbcJson = "";
        private static string s_cibJson = "";
        private static string s_cmbJson = "";
        private static string s_masterCardJson = "";
        private readonly ILogger<ExchangeLogger> _logger;

        public ExchangeLogger(ILogger<ExchangeLogger> logger)
        {
            _logger = logger;
        }

        public async Task OnMessageAsync(MessageContext message, HttpApiClient api)
        {
            if (s_stopwatch == null)
            {
                s_stopwatch = Stopwatch.StartNew();
            }
            if (s_nextCheck < s_stopwatch.ElapsedMilliseconds)
            {
                const int IntervalMilliseconds = 10 * 60_000;
                s_nextCheck = s_stopwatch.ElapsedMilliseconds + IntervalMilliseconds;

                var cmbc = await HttpApi.Resolve<ICmbcCreditRate>().GetRates().ConfigureAwait(false);
                var cmbcJson = JsonSerializer.Serialize(cmbc);
                if (cmbcJson != s_cmbcJson)
                {
                    s_cmbcJson = cmbcJson;
                    _logger.LogInformation($"CMBC Changed: {cmbcJson}");
                }

                var cib = await HttpApi.Resolve<ICibRate>().GetRates().ConfigureAwait(false);
                var cibJson = JsonSerializer.Serialize(cib);
                if (cibJson != s_cibJson)
                {
                    s_cibJson = cibJson;
                    _logger.LogInformation($"CIB Changed: {cibJson}");
                }

                var masterCard = await HttpApi.Resolve<IMasterCardRate>().GetRate("JPY", "USD").ConfigureAwait(false);
                var masterCardJson = JsonSerializer.Serialize(masterCard);
                if (masterCardJson != s_masterCardJson)
                {
                    s_masterCardJson = masterCardJson;
                    _logger.LogInformation($"MasterCard Changed: {masterCardJson}");
                }

                var cmb = await HttpApi.Resolve<ICmbRateProvider>().GetRatesInCode().ConfigureAwait(false);
                var cmbJson = JsonSerializer.Serialize(cmb);
                if (cmbJson != s_cmbJson)
                {
                    s_cmbJson = cmbJson;
                    _logger.LogInformation($"CMB Changed: {cmbJson}");
                }
            }
        }
    }
#nullable restore
}
