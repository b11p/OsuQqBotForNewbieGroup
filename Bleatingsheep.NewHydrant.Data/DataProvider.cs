using System;
using System.Threading.Tasks;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Bleatingsheep.NewHydrant.Data
{
    public class DataProvider : IDataProvider
    {
        private readonly NewbieContext _dbContext;

        public DataProvider(NewbieContext newbieContext)
        {
            _dbContext = newbieContext;
        }

        public async Task<(bool success, BindingInfo? result)> GetBindingInfoAsync(long qq)
        {
            try
            {
                var result = await _dbContext.Bindings.SingleOrDefaultAsync(b => b.UserId == qq).ConfigureAwait(false);
                return (true, result);
            }
            catch (Exception e)
            {
                OnException?.Invoke(e);
                return (false, default);
            }
        }

        public async Task<(bool success, int? result)> GetBindingIdAsync(long qq)
        {
            var (success, bi) = await GetBindingInfoAsync(qq).ConfigureAwait(false);
            return (success, bi?.OsuId);
        }

        public event Action<Exception>? OnException;
    }
}
