using Sisters.WudiLib.Posts;

namespace OsuQqBot.AttributedFunctions
{
    interface IMessageCommandable
    {
        bool ShouldResponse(Message message);

        void Process(Message message, Sisters.WudiLib.HttpApiClient api);

        IMessageCommandable Create();
    }
}
