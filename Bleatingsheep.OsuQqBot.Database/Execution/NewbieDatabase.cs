using System;
using System.Collections.Generic;
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

        public override async Task<IExecutingResult<BindingInfo>> AddNewBindAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName)
        {
            return await TryExecuteAsync(async context =>
            {
                BindingInfo binding = CreateBindingInfo(qq, osuId, source);
                var bindResult = await context.Bindings.AddAsync(binding);
                var history = await context.Histories.AddAsync(CreateBindingHistory(qq, osuId, osuName, source, operatorId, operatorName));
                await context.SaveChangesAsync();
                return binding;
            });
        }

        private static OperationHistory CreateBindingHistory(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName)
        {
            return new OperationHistory
            {
                Operation = Operation.Binding,
                UserId = qq,
                User = osuName,
                OperatorId = operatorId,
                Operator = operatorName,
                Remark = $"osu! ID: {osuId}; source: {source}",
            };
        }

        private static OperationHistory CreateBindingHistory(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName, string reason)
        {
            return new OperationHistory
            {
                Operation = Operation.Binding,
                UserId = qq,
                User = osuName,
                OperatorId = operatorId,
                Operator = operatorName,
                Remark = $"osu! ID: {osuId}; source: {source}; reason: {reason}",
            };
        }

        private static BindingInfo CreateBindingInfo(long qq, int osuId, string source) => new BindingInfo { UserId = qq, OsuId = osuId, Source = source };

        public override async Task<IExecutingResult> AddPlusHistoryAsync(IUserPlus userPlus)
        {
            return await TryExecuteAsync(async context =>
            {
                await context.PlusHistories.AddAsync(new PlusHistory(userPlus));
                await context.SaveChangesAsync();
                return (object)null;
            });
        }

        public override async Task<IExecutingResult> AddPlusHistoryRangeAsync(IEnumerable<IUserPlus> userPluses)
        {
            return await TryExecuteAsync(async context =>
            {
                await context.PlusHistories.AddRangeAsync(userPluses.Select(up => new PlusHistory(up)));
                await context.SaveChangesAsync();
                return (object)null;
            });
        }

        public override async Task<IExecutingResult<BindingInfo>> GetBindingInfoAsync(long qq)
            => await TryExecuteAsync(async context => await context.Bindings.SingleOrDefaultAsync(b => b.UserId == qq));
        public override async Task<IExecutingResult<IList<int>>> GetPlusRecordedUsersAsync()
            => await TryExecuteAsync(async context => await context.PlusHistories.Select(ph => ph.Id).Distinct().ToListAsync());

        public override async Task<IExecutingResult<PlusHistory>> GetRecentPlusHistory(int osuId)
        {
            return await TryExecuteAsync(
                async context => await context.PlusHistories
                    .Where(ph => ph.Id == osuId)
                    .OrderByDescending(ph => ph.Date)
                    .FirstOrDefaultAsync()
            );
        }

        public override async Task<IExecutingResult<int?>> ResetBindingAsync(long qq, int osuId, string osuName, string source, long? operatorId, string operatorName, string reason)
        {
            return await TryExecuteAsync(async context =>
            {
                var binding = await context.Bindings.Where(bi => bi.UserId == qq).SingleOrDefaultAsync();
                bool add;
                var oldUid = binding?.OsuId;
                if (add = binding == null)
                {
                    binding = CreateBindingInfo(qq, osuId, source);
                    await context.Bindings.AddAsync(binding);
                }
                else if (binding.OsuId == osuId)
                {
                    return binding.OsuId;
                }
                else
                {
                    binding.OsuId = osuId;
                    binding.Source = source;
                    context.Bindings.Update(binding);
                }
                var historyEntry = await context.Histories.AddAsync(CreateBindingHistory(qq, osuId, osuName, source, operatorId, operatorName, reason));
                await context.SaveChangesAsync();
                return oldUid;
            });
        }
    }
}
