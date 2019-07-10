using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{
    public class ExchangeResponse
    {
        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("time_last_updated")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset TimeLastUpdated { get; set; }

        [JsonProperty("rates")]
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
