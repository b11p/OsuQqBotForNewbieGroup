using System;
using Newtonsoft.Json;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{

    public partial class CibRate
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("records")]
        public int Records { get; set; }

        [JsonProperty("sidx")]
        public string Sidx { get; set; }

        [JsonProperty("sord")]
        public string Sord { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("rows")]
        public CibRateData[] Rows { get; set; }

        public decimal? this[string Currency]
        {
            get
            {
                var data = Array.Find(Rows, d => string.Equals(Currency, d.EnglishName, StringComparison.OrdinalIgnoreCase));
                return data.BuyPrice / data.Unit;
            }
        }
    }

    public class CibRateData
    {
        [JsonProperty("cell")]
        public string[] Cell { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        public decimal? Unit => Cell?.Length == 7 && decimal.TryParse(Cell[2], out var result) ? result : default;

        public decimal? BuyPrice => Cell?.Length == 7 && decimal.TryParse(Cell[4], out var result) ? result : default;

        public string EnglishName => Cell?.Length == 7 ? Cell[1] : string.Empty;
    }
}
