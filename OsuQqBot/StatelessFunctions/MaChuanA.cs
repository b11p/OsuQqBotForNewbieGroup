using OsuQqBot.QqBot;
using OsuQqBot.Query;

namespace OsuQqBot.StatelessFunctions
{
    class MaChuanA : IStatelessFunction
    {
        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            message = message.Trim();
            if (message == "妈船？" || message == "妈船?")
            {
                long? uid = Querying.Instance.GetUserBind(messageSource.FromQq).Result;
                if (!uid.HasValue) return true;
                string url = OpenApi.Instance.MotherShipApiClient.GetStatUrl((int)uid);
                var api = OsuQqBot.QqApi;
                string content = api.OnlineImage(url, true);
                api.SendMessageAsync(endPoint, content);
            }
            return false;
        }
    }
}
