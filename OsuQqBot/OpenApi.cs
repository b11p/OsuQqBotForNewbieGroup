using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuMixedApi.MotherShip;
using OsuQqBot.Data;
using System.Threading;

namespace OsuQqBot
{
    /// <summary>
    /// 公开给各种组件的单例。
    /// </summary>
    class OpenApi
    {
        private static OpenApi s_instance;

        private OpenApi()
        {
        }

        public static OpenApi Instance => s_instance;

        public IBindingsAsync Bindings { get; private set; }
        public MotherShipApiClient MotherShipApiClient { get; private set; }
        public OsuApiClient OsuApiClient { get; private set; }

        /// <summary>
        /// 初始化<see cref="OpenApi"/>
        /// </summary>
        /// <exception cref="LoadedException"></exception>
        /// <param name="bindings"></param>
        public static void Init(IBindingsAsync bindings, MotherShipApiClient motherShipApiClient, OsuApiClient osuApiClient)
        {
            var instance = new OpenApi
            {
                Bindings = bindings,
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
