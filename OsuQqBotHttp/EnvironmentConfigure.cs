using System;

namespace Bleatingsheep.NewHydrant
{
    public sealed class EnvironmentConfigure
    {
#if DEBUG
        public string Listen => "http://127.0.0.1:8876";
        public string ApiAddress => "http://127.0.0.1:5700";
        public string WSEventAddress => "ws://127.0.0.1:6700/event";
#else
        public string Listen => "http://+:8876";
        public string ApiAddress => Environment.GetEnvironmentVariable("API_ADDRESS"); // 为了旧 bot，必须不以 '/' 结尾。
        public string WSEventAddress => Environment.GetEnvironmentVariable("WS_EVENT_ADDRESS");
#endif
        public string AccessToken => Environment.GetEnvironmentVariable("ACCESS_TOKEN");
    }
}
