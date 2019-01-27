using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.NewHydrant.Osu;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;

namespace Bleatingsheep.NewHydrant
{
    //[Function("logger_test")]
    public class LoggerTest : Service, IMessageMonitor, IMessageCommand
    {
        public Task OnMessageAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            if (message.UserId == 962549599)
            {
                var nLog = Logger;
                Logger.Info(message.Content.Text);
            }
            return Task.CompletedTask;
        }

        public Task ProcessAsync(Sisters.WudiLib.Posts.Message context, HttpApiClient api)
        {
            Logger.Info("消息处理。");
            return Task.CompletedTask;
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message context)
        {
            if (context.UserId == 962549599)
            {
                Logger.Info("进入判断。");
                return true;
            }
            return false;
        }
    }
}
