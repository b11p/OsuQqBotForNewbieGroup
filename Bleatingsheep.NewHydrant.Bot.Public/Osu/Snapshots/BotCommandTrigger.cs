using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.NewHydrant.Data;
using Bleatingsheep.Osu;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots
{
    [Component("speaking_trigger_for_snapshot")]
    public class BotCommandTrigger : IMessageMonitor
    {
        private static readonly IReadOnlyCollection<string> s_baicaiCommands = new List<string>
        {
            "bpme",
            "recent",
            "pr",
            "statme",
        }.AsReadOnly();
        private readonly DataMaintainer _dataMaintainer;
        private readonly IDataProvider _dataProvider;

        public BotCommandTrigger(DataMaintainer dataMaintainer, IDataProvider dataProvider)
        {
            _dataMaintainer = dataMaintainer;
            _dataProvider = dataProvider;
        }

        public async Task OnMessageAsync(MessageContext message, HttpApiClient api)
        {
            if (!s_baicaiCommands.Any(c => message.Content.Text.Contains(c)))
            {
                return;
            }
            var uid = await _dataProvider.GetOsuIdAsync(message.UserId).ConfigureAwait(false);
            Mode? mode = null;
            // TODO: Use binding from mothership database first.
            if (uid != null)
            {
                if (mode != null)
                {
                    await _dataMaintainer.UpdateAsync(uid.Value, mode.Value).ConfigureAwait(false);
                }
                else
                {
                    foreach (Mode m in Enum.GetValues(typeof(Mode)))
                    {
                        await _dataMaintainer.UpdateAsync(uid.Value, m).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
