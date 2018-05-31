using Sisters.WudiLib.Posts;

namespace OsuQqBot.AttributedFunctions
{
    interface IMessageMonitor
    {
        void OnMessage(Message message, Sisters.WudiLib.HttpApiClient api);

        IMessageMonitor Create();
    }
}
