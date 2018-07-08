using System;
using System.Threading.Tasks;

namespace OsuQqBot.Data
{
    internal class EFGroup : IUserGroupAsync
    {
        public long HardcodedSA => 962549599;

        public Task<bool> AddAsync(long qq, string group) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(long qq, string group) => throw new NotImplementedException();
        public bool Is(long qq, string group) => throw new NotImplementedException();
    }
}
