using System;
using System.Linq;
using System.Threading.Tasks;
using Bleatingsheep.Osu.PerformancePlus;
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

        public override async Task<IExecutingResult> AddNewBindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName)
        {
            return await TryExecuteAsync(async context =>
            {
                var bindResult = await context.Bindings.AddAsync(new BindingInfo { UserId = qq, OsuId = osuId, Source = source });
                var history = await context.Histories.AddAsync(new OperationHistory
                {
                    Operation = Operation.Binding,
                    UserId = qq,
                    User = osuName,
                    OperatorId = operatorId,
                    Operator = operatorName,
                    Remark = $"osu! ID: {osuId}; source: {source}",
                });
                await context.SaveChangesAsync();
                return (object)null;
            });
        }

        public override async Task<IExecutingResult> AddPlusHistoryAsync(UserPlus userPlus)
        {
            return await TryExecuteAsync(async context =>
            {
                await context.PlusHistories.AddAsync(new PlusHistory(userPlus));
                await context.SaveChangesAsync();
                return (object)null;
            });
        }

        public override async Task<IExecutingResult<BindingInfo>> GetBindingInfoAsync(long qq)
            => await TryExecuteAsync(async context => await context.Bindings.SingleOrDefaultAsync(b => b.UserId == qq));
        public override async Task<IExecutingResult<PlusHistory>> GetRecentPlusHistory(int osuId)
        {
            return await TryExecuteAsync(
                async context => await context.PlusHistories
                    .Where(ph => ph.Id == osuId)
                    .OrderByDescending(ph => ph.Date)
                    .FirstOrDefaultAsync()
            );
        }
    }
}
