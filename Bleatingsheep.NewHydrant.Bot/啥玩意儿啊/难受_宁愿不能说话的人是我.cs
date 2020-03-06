using System;
using System.Collections.Generic;
using System.Text;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    internal class 难受_宁愿不能说话的人是我
    {
        public async void EvilDalou(HttpApiClient api, GroupBanNotice ban)
        {
            try
            {
                if (ban.GroupId == 514661057 && ban.OperatorId == 3082577334 && ban.UserId == 2017642475)
                {
                    if ((ban.Time.Second & 1) == 0)
                    {
                        await api.SendGroupMessageAsync(ban.GroupId, Message.At(1061566571) + new Message(" 快解开啊，白妹妹太可怜了")).ConfigureAwait(false);
                    }
                    else
                    {
                        await api.SendGroupMessageAsync(ban.GroupId, "难受 宁愿不能说话的人是我").ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                // ignore
                Console.WriteLine(e);
            }
        }
    }
}
