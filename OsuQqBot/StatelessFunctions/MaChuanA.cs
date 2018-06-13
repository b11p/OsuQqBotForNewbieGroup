using System;
using System.Collections.Generic;
using System.Text;
using OsuQqBot.Api;
using OsuQqBot.LocalData;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class MaChuanA : IStatelessFunction
    {
        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            message = message.Trim();
            if (message == "妈船？" || message == "妈船?")
            {
                long? uid = Query.Querying.Instance.GetUserBind(messageSource.FromQq).Result;
                if (!uid.HasValue) return true;
                string url = MotherShipApi.GetStatUrl(uid.Value);
                var api = OsuQqBot.QqApi;
                string content = api.OnlineImage(url, true);
                api.SendMessageAsync(endPoint, content);
            }
            return false;
        }
    }
}
