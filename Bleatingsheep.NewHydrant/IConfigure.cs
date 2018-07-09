using System;
using System.Collections.Generic;
using System.Text;

namespace Bleatingsheep.NewHydrant
{
    interface IConfigure
    {
        string ApiKey { get; }
        long SuperAdmin { get; }
        string Listen { get; }
        string ApiAddress { get; }
    }
}
