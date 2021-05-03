using Newtonsoft.Json;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange
{
    public class CmbcRate
    {
        [JsonProperty("retCode")]
        public string RetCode { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("data")]
        public CmbcRateData[] Data { get; set; }
    }

    public class CmbcRateData
    {
        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("wapUrl")]
        public string WapUrl { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("pcUrl")]
        public string PcUrl { get; set; }
    }
}
