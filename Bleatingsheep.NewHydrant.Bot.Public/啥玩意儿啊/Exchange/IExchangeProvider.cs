using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange;
#nullable enable
public interface IExchangeProvider
{
    decimal this[string name] { get; }

    string Target { get; }
}
#nullable restore