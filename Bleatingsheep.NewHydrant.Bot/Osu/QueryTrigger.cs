using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Core;
using Bleatingsheep.Osu;
using Bleatingsheep.Osu.ApiClient;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu
{
    [Component("query")]
    public class QueryTrigger : Service, IMessageCommand
    {
        private static readonly Regex s_selfRegex = new Regex(@"^(?<trigger>~)\s*[,，]?\s*(?<mode>\S*)\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex s_whereRegex = new Regex($@"^(?<trigger>where)\s*(?<name>{OsuHelper.UsernamePattern})\s*[,，]?\s*(?<mode>\S*)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly Lazy<IOsuApiClient> _osuApiLazy;
        private readonly Lazy<NewbieContext> _newbieContext;
        private readonly Lazy<QueryHelper> _queryHelper;
        private IOsuApiClient OsuApi => _osuApiLazy.Value;
        private NewbieContext NewbieContext => _newbieContext.Value;
        private QueryHelper QueryHelper => _queryHelper.Value;

        #region Parameters

        [Parameter("trigger")]
        public string Trigger { get; set; }

        [Parameter("name")]
        public string Name { get; set; }

        [Parameter("mode")]
        public string ModeString { get; set; }

        #endregion

        public QueryTrigger(Lazy<IOsuApiClient> osuApiLazy, Lazy<NewbieContext> newbieContext, Lazy<QueryHelper> queryHelper)
        {
            _osuApiLazy = osuApiLazy;
            _newbieContext = newbieContext;
            _queryHelper = queryHelper;
        }

        public bool ShouldResponse(MessageContext context)
        {
            return context.Content.TryGetPlainText(out string text) &&
                (RegexCommand(s_selfRegex, text)
                || RegexCommand(s_whereRegex, text));
        }

        public async Task ProcessAsync(MessageContext context, HttpApiClient api)
        {
            if (context is not GroupMessage g || g.GroupId == 851868928)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    var bindingInfo = await NewbieContext.Bindings.Where(b => b.UserId == context.UserId).FirstOrDefaultAsync().ConfigureAwait(false);
                    if (bindingInfo is null)
                    {
                        await api.SendMessageAsync(context.Endpoint, "未绑定 osu! 账号。").ConfigureAwait(false);
                        return;
                    }
                    var osuId = bindingInfo.OsuId;
                    Mode? mode;
                    try
                    {
                        mode = ModeExtensions.Parse(ModeString);
                    }
                    catch
                    {
                        mode = default;
                    }
                    var message = await QueryHelper.QueryByUserId(osuId, mode).ConfigureAwait(false);
                    await api.SendMessageAsync(context.Endpoint, message).ConfigureAwait(false);
                }
            }
        }
    }
}
