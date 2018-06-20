using System.Threading.Tasks;

namespace OsuQqBot.Data
{
    interface IUserGroupAsync
    {
        Task<bool> IsAsync(long qq, string group);
        Task<bool> AddAsync(long qq, string group);
        Task<bool> DeleteAsync(long qq, string group);
        long HardcodedSA { get; }
    }
}
