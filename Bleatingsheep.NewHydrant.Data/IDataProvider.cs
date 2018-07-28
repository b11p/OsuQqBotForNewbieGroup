using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Data
{
    public interface IDataProvider
    {
        Task<(bool success, int? result)> GetBindingIdAsync(long qq);
    }
}
