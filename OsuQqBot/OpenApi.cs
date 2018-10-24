using System.Threading;
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;

namespace OsuQqBot
{
    /// <summary>
    /// 公开给各种组件的单例。
    /// </summary>
    public class OpenApi
    {
        private static OpenApi s_instance;

        private OpenApi()
        {
        }

        public static OpenApi Instance => s_instance;
        
        public MotherShipApiClient MotherShipApiClient { get; private set; }
        public OsuApiClient OsuApiClient { get; private set; }

        /// <summary>
        /// 初始化<see cref="OpenApi"/>
        /// </summary>
        /// <exception cref="LoadedException"></exception>
        /// <param name="bindings"></param>
        public static void Init(MotherShipApiClient motherShipApiClient, OsuApiClient osuApiClient)
        {
            var instance = new OpenApi
            {
                MotherShipApiClient = motherShipApiClient,
                OsuApiClient = osuApiClient,
            };
            var old = Interlocked.CompareExchange(ref s_instance, instance, default(OpenApi));
            if (old != default(OpenApi))
            {
                throw new LoadedException();
            }
        }
    }
}
