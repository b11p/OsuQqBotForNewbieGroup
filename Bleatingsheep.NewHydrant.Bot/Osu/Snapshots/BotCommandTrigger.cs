using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.Osu;
using Microsoft.EntityFrameworkCore;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots
{
    [Component("speaking_trigger_for_snapshot")]
    public class BotCommandTrigger : OsuFunction, IMessageMonitor
    {
        private static readonly IReadOnlyCollection<string> s_baicaiCommands = new List<string>
        {
            "bpme",
            "recent",
            "pr",
            "statme",
        }.AsReadOnly();

        public async Task OnMessageAsync(MessageContext message, HttpApiClient api)
        {
            if (!s_baicaiCommands.Any(c => message.Content.Text.Contains(c)))
            {
                return;
            }
            var (success, uid) = await DataProvider.GetBindingIdAsync(message.UserId).ConfigureAwait(false);
            if (success && uid == null)
                return;
            Mode? mode = null;
            // TODO: Use binding from mothership database first.
            var snapshotUtility = new SnapshotUtility();
            if (uid != null)
            {
                if (mode != null)
                {
                    await snapshotUtility.UpdateAsync(uid.Value, mode.Value).ConfigureAwait(false);
                }
                else
                {
                    foreach (Mode m in Enum.GetValues(typeof(Mode)))
                    {
                        await snapshotUtility.UpdateAsync(uid.Value, m).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
