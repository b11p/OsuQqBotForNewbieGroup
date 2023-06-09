namespace Bleatingsheep.NewHydrant.啥玩意儿啊.Exchange;
#nullable enable
public interface IExchangeService
{
    IExchangeProvider? GetProvider(string name, string target);
}
#nullable restore