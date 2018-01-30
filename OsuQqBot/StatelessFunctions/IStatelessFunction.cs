using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    interface IStatelessFunction
    {
        bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message);
    }
}
