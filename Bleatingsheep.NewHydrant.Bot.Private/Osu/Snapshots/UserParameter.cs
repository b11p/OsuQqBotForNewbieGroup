using Bleatingsheep.Osu;

namespace Bleatingsheep.NewHydrant.Osu.Snapshots
{
    internal struct UserParameter
    {
        public UserParameter(int userId, Mode mode)
        {
            UserId = userId;
            Mode = mode;
        }

        public int UserId { get; set; }
        public Mode Mode { get; set; }
    }
}
