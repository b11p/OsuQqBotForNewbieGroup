using System;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.OsuQqBot.Database.Execution
{
    public sealed class NewbieDatabase : NewbieDatabaseBase
    {
        private static async Task<IExecutingResult<T>> TryExecuteAsync<T>(Func<NewbieContext, Task<T>> func)
        {
            try
            {
                using (var context = new NewbieContext())
                {
                    return new ExecutingResult<T>(await func(context));
                }
            }
            catch (Exception e)
            {
                return new ExecutingResult<T>(e);
            }
        }

        public override async Task<IExecutingResult<BindingInfo>> GetBindingInfoAsync(long qq)
            => await TryExecuteAsync(async context => await context.Bindings.SingleOrDefaultAsync(b => b.UserId == qq));
    }
}
