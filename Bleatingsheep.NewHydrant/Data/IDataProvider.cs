using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Data
{
    interface IDataProvider
    {
        Task<(bool success, int? result)> GetBindingIdAsync(long qq);
    }
}
