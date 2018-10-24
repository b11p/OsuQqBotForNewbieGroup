using OsuQqBot.Commands;
using Sisters.WudiLib.Posts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OsuQqBot.Charts
{
    static class ChartExecution
    {


        public static string Execute(Endpoint endpoint, MessageSource source, string param, out string location)
        {
            IReadOnlyList<string> lines = param.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            string filtedParam = param.Trim();

            if (string.Compare("new", lines[0], true, CultureInfo.InvariantCulture) == 0)
            {

            }

            location = null;
            return "无法识别命令" + lines[0];
        }

        public static string New(IReadOnlyList<string> param, int index)
        {
            var chartInfo = ChartInfo.New(param, index);

            throw new NotImplementedException();
        }
    }
}
