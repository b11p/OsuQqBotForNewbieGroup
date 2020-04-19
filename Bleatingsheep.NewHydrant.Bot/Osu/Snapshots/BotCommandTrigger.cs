using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.NewHydrant.Attributions;
using Bleatingsheep.Osu;
using Microsoft.EntityFrameworkCore;
using MotherShipDatabase;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots
{
    [Function("speaking_trigger_for_snapshot")]
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
            int uidFromBaicai;
            Mode? mode = null;
            var snapshotUtility = new SnapshotUtility();
            using (var mothership = new OsuContext())
            {
                try
                {
                    var user = await mothership.Userrole.FirstOrDefaultAsync(u => u.Qq == message.UserId).ConfigureAwait(false);
                    if (user?.UserId == uid)
                    {
                        mode = (Mode?)user?.Mode;
                    }
                    else if (uid != null)
                    {
                        var userFromId = await mothership.Userrole.FirstOrDefaultAsync(u => u.UserId == uid).ConfigureAwait(false);
                        mode = (Mode?)userFromId?.Mode;
                    }
                    if (user?.UserId != uid && user?.UserId != null)
                    {
                        uidFromBaicai = user.UserId.Value;
                        await snapshotUtility.UpdateAsync(uidFromBaicai, (Mode)user.Mode).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Logger.Info(e);
                }
            }
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
