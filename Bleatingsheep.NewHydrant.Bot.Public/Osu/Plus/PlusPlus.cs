using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Osu;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using HttpApi = WebApiClient.HttpApi;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;
using System.Globalization;
using System.Threading;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.NewHydrant.Core;

namespace Bleatingsheep.NewHydrant.Osu.Plus
{
    [Component("plus_plus")]
    class PlusPlus : Service, IMessageCommand
    {
        private static int s_initialized = 0;
        private static readonly object s_initializingObject = new object();

        public PlusPlus(ILegacyDataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public string Name { get; }
        private ILegacyDataProvider DataProvider { get; }

        private static void InitializeIfNecessary()
        {
            if (s_initialized == 0)
            {
                lock (s_initializingObject)
                {
                    if (s_initialized == 0)
                    {
                        HttpApi.Register<IPlusApi>();
                        s_initialized = 1;
                    }
                }
            }
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            InitializeIfNecessary();
            var id = await DataProvider.EnsureGetBindingIdAsync(context.UserId);
            var myWebApi = HttpApi.Resolve<IPlusApi>();
            var user = await myWebApi.GetUserAsync(id);
            var userPlus = user?.Data;
            var responseMessage = $@"{userPlus.UserName} 的 PP+ 数据
Performance: {userPlus.Performance}
Aim (Jump): {userPlus.AimJump}
Aim (Flow): {userPlus.AimFlow}
Precision: {userPlus.Precision}
Speed: {userPlus.Speed}
Stamina: {userPlus.Stamina}
Accuracy: {userPlus.Accuracy}";
            await api.SendMessageAsync(context.Endpoint, responseMessage);
        }

        public bool ShouldResponse(MessageContext context)
        {
            if (context is GroupMessage g && g.GroupId == 595985887)
                return false; // ignored in newbie group.
            return context.Content.TryGetPlainText(out var text) && text == "++";
        }
    }
}
