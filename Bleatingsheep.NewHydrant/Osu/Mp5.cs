using System;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu.PerformancePlus;
using Sisters.WudiLib;
using static System.Math;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Function("mp5_sy")]
    internal class Mp5 : OsuFunction, IMessageCommand
    {
        private static readonly PerformancePlusSpider s_spider = new PerformancePlusSpider();

        public async Task ProcessAsync(Sisters.WudiLib.Posts.Message message, HttpApiClient api, ExecutingInfo executingInfo)
        {
            try
            {
                var id = await EnsureGetBindingIdAsync(message.UserId);
                var plus = await s_spider.GetUserPlusAsync(id);
                ExecutingException.Ensure(plus != null, "被办了。");
                var ev = CostOf(plus);

                await api.SendMessageAsync(message.Endpoint, $"{plus.Name}：{ev:0.00}\r\n请化学式结一下有关费用。");
            }
            catch (ExceptionPlus)
            {
                await api.SendMessageAsync(message.Endpoint, "查询PP+失败。");
                return;
            }
        }

        public static double CostOf(IUserPlus plus)
        {
            return 10 * Sqrt((Atan((2 * plus.AimJump - (1700 + 1300)) / (1700 - 1300)) + PI / 2 + 8) * (Atan((2 * plus.AimFlow - (450 + 200)) / (450 - 200)) + PI / 2 + 3))
+ (Atan((2 * plus.Precision - (400 + 200)) / (400 - 200)) + PI / 2)
+ 7 * (Atan((2 * plus.Speed - (1250 + 950)) / (1250 - 950)) + PI / 2)
+ 3 * (Atan((2 * plus.Stamina - (1000 + 600)) / (1000 - 600)) + PI / 2)
+ 10 * (Atan((2 * plus.Accuracy - (1200 + 600)) / (1200 - 600)) + PI / 2);
        }

        public bool ShouldResponse(Sisters.WudiLib.Posts.Message message) => message.Content.IsPlaintext && " mp5".Equals(message.Content.Text, StringComparison.InvariantCultureIgnoreCase);
    }
}
