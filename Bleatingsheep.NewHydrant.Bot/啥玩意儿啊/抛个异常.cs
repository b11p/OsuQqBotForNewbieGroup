using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Sisters.WudiLib;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊
{
    [Function("ex")]
    class 抛个异常 : IMessageCommand
    {
        public Task ProcessAsync(Sisters.WudiLib.Posts.Message context, HttpApiClient api) => throw new NotImplementedException();
        public bool ShouldResponse(Sisters.WudiLib.Posts.Message context)
        {
            if (context.UserId != 962549599)
                return false;
            else
            {
                switch (context.Content.Text)
                {
                    case "ex1":
                        throw new Exception();
                    case "ex2":
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }
    }
}
