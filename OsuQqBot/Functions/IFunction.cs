using OsuQqBot.QqBot;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsuQqBot.Functions
{
    interface IFunction
    {
        string Title { get; set; }

        string GetHelp(EndPoint endPoint);
        bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message);

    }
}
