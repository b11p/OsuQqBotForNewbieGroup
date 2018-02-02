using System;
using System.Collections.Generic;
using System.Text;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class IntIsMeimei : IStatelessFunction
    {
        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (messageSource.FromQq == 1004121460 && message.Contains("咩羊妹妹"))
            {
                var bot = OsuQqBot.QqApi;
                bot.SendMessageAsync(endPoint, message.Replace("咩羊", "int").Replace("[CQ:at,qq=962549599]", "[CQ:at,qq=1004121460]"));
                return true;
            }
            return false;
        }
    }
}
