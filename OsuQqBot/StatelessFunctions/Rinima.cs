using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class Rinima : IStatelessFunction
    {
        private static readonly long QGodQQ = 125647408;

        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (messageSource.FromQq == QGodQQ)
            {
                message = message.ToLowerInvariant();
                if (message.Contains("cnm") || message.Contains("rnm") || message.Contains("日你妈") || message.Contains("操你妈"))
                {
                    OsuQqBot.QqApi.SendMessageAsync(endPoint, OsuQqBot.QqApi.LocalImage(@"C:\Users\Administrator\Desktop\Rinima.jpg"));
                    return true;
                }
            }
            return false;
        }
    }
}
