using Bleatingsheep.OsuMixedApi.MotherShip;
using OsuQqBot.Data;
using System;
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

        public IBindings Bindings { get; private set; }
        public MotherShipApiClient MotherShipApiClient { get; private set; }

        /// <summary>
        /// 初始化<see cref="OpenApi"/>
        /// </summary>
        /// <exception cref="LoadedException"></exception>
        /// <param name="bindings"></param>
        public static void Init(IBindings bindings, MotherShipApiClient motherShipApiClient)
        {
            var instance = new OpenApi
            {
                Bindings = bindings,
                MotherShipApiClient = motherShipApiClient,
            };
            var old = Interlocked.CompareExchange(ref s_instance, instance, default(OpenApi));
            if (old != default(OpenApi))
            {
                throw new LoadedException();
            }
        }
    }

    /// <summary>
    /// 已经初始化，不能再次初始化。
    /// </summary>
    [Serializable]
    public class LoadedException : Exception
    {
        public LoadedException() { }
        public LoadedException(string message) : base(message) { }
        public LoadedException(string message, Exception inner) : base(message, inner) { }
        protected LoadedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
