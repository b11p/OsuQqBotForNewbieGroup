using Newtonsoft.Json;

namespace Bleatingsheep.OsuMixedApi.MotherShip
{
    public class MotherShipResponse<T>
    {
        public const int SuccessStatusCode = 0;
        public const string SuccessStatusMessage = "success";

        [JsonProperty("code")]
        public int StatusCode { get; private set; }
        [JsonProperty("status")]
        public string Message { get; private set; }
        [JsonProperty("data")]
        public T Data { get; private set; }

        /// <summary>
        /// Throws if <see cref="StatusCode"/> is not <see cref="SuccessStatusCode"/>.
        /// </summary>
        /// <exception cref="OsuApiFailedException"></exception>
        public void EnsureSuccess()
        {
            if (!IsSuccessStatusCode()) throw new OsuApiFailedException();
        }

        public bool IsSuccessStatusCode() => StatusCode == SuccessStatusCode;
    }
}
