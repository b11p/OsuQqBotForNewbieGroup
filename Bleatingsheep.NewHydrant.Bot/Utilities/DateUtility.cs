using System;

namespace Bleatingsheep.NewHydrant.Utilities
{
    public static class DateUtility
    {
        public static TimeSpan GetError(DateTimeOffset wanted, DateTimeOffset actual)
        {
            var error = wanted - actual;
            if (error < TimeSpan.Zero)
                error = -error;
            return error;
        }
    }
}
