using System;
using NLog;

namespace Bleatingsheep.NewHydrant.Core
{
    public class Service
    {
        private readonly Lazy<Logger> _logger;

        public Service()
            => _logger = new Lazy<Logger>(
                () => LogFactory?.GetCurrentClassLogger() ?? LogManager.CreateNullLogger()
            );

        internal LogFactory LogFactory { private get; set; }

        protected Logger Logger => _logger.Value;
    }
}
