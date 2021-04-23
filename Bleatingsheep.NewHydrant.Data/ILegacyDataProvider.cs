using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Data
{
    public interface ILegacyDataProvider
    {
        Task<(bool success, int? result)> GetBindingIdAsync(long qq);
    }
}
