using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.Admin
{
    internal interface IVerifier
    {
        Task<bool> IsAdminAsync(long qq);
    }
}
