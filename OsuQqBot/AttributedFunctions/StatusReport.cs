using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.AttributedFunctions
{
    [Function]
    class StatusReport : IRegularly
    {
        private static readonly TimeSpan ZeroInChina = new TimeSpan(0, 16, 0, 0, 1);
        public TimeSpan? OnUtc => ZeroInChina;
        private static readonly TimeSpan TS8 = new TimeSpan(8, 0, 0);
        public TimeSpan? Every => TS8;

        public void Run()
        {
            var api = OsuQqBot.ApiV2;
            string message = $"{DateTimeOffset.Now.ToOffset(new TimeSpan(8, 0, 0)):H:mm}消防栓工作正常。";
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            api.SendGroupMessageAsync(514661057, message);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }
    }
}
