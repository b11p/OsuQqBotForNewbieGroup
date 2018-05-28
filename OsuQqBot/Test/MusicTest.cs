using OsuQqBot.AttributedFunctions;
using Sisters.WudiLib;
using System;

namespace OsuQqBot.Test
{
    [Function]
    internal class MusicTest : IMessageCommandable
    {
        private string[] lines;

        public IMessageCommandable Create() => new MusicTest();

        public async void Process(Sisters.WudiLib.Posts.Message message, HttpApiClient api)
        {
            try
            {
                SendingMessage sendingMessage;
                if (lines.Length == 6)
                {
                    sendingMessage = SendingMessage.MusicCustom(lines[1], lines[2], lines[3], lines[4], lines[5]);
                }
                else
                {
                    sendingMessage = SendingMessage.MusicCustom(lines[1], lines[2], lines[3]);
                }
                await OsuQqBot.ApiV2.SendMessageAsync(message.Endpoint, sendingMessage);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message)
        {
            if (message.Source.IsAnonymous || !(message.Source.UserId == OsuQqBot.IdWhoLovesInt100Best))
            {
                return false;
            }
            if (!message.Content.IsPlaintext) return false;
            lines = message.Content.Text.Split("\r\n");
            if ((lines.Length == 4 || lines.Length == 6) && lines[0].Equals("music", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
