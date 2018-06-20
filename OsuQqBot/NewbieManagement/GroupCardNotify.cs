using OsuQqBot.AttributedFunctions;
using Sisters.WudiLib;

namespace OsuQqBot.NewbieManagement
{
    //[Function]
    class GroupCardNotify : IMessageMonitor
    {
        public IMessageMonitor Create() => new GroupCardNotify();

        public void OnMessage(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            throw new System.NotImplementedException();
        }
    }
}
