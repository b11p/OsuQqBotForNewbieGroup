using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IPController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IPController(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        [HttpGet]
        public ActionResult<string> AutoRoute(string ip)
        {
            try
            {
                var isEmpty = string.IsNullOrWhiteSpace(ip);
                IPAddress ipAddress = isEmpty
                    ? HttpContext.Connection.RemoteIpAddress ?? IPAddress.IPv6None
                    : Bleatingsheep.IPLocation.IPv6Locator.GetPureIP(IPAddress.Parse(ip));

                HttpContext.Response.Redirect(ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    ? $"https://www.ipip.net/ip/{ipAddress}.html"
                    : $"https://ip.zxinc.org/ipquery/?ip={System.Web.HttpUtility.UrlEncode(ipAddress.ToString())}", !isEmpty);
            }
            catch (Exception)
            {
                HttpContext.Response.Redirect(string.IsNullOrWhiteSpace(ip) ? "https://www.ipip.net/ip.html" : $"https://www.ipip.net/ip/{ip}.html", true);
            }
            return string.Empty;
        }
    }
}