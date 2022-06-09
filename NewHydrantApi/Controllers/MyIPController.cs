using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyIPController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MyIPController> _logger;

        public MyIPController(IHttpContextAccessor httpContextAccessor, ILogger<MyIPController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            _logger.LogInformation(new EventId(12, "myipevent"), "get myip");
            _logger.LogInformation("get myip pure");
            _logger.LogInformation("access from {0}", HttpContext.Connection.RemoteIpAddress);

            var sb = new StringBuilder();
            sb.AppendLine("-----direct-----");

            var connection = HttpContext.Connection;
            sb.AppendLine(new IPEndPoint(connection.RemoteIpAddress ?? IPAddress.IPv6None, connection.RemotePort).ToString());

            var request = HttpContext.Request;
            sb.AppendLine(request.Scheme);
            sb.AppendLine(request.Host.ToString());
            sb.AppendLine(request.Protocol);
            sb.AppendLine(request.IsHttps.ToString());
            sb.AppendLine(request.Method);

            sb.AppendLine("-----forwarded-----");

            var headers = request.Headers;
            sb.AppendLine(headers["X-Forwarded-For"]);
            sb.AppendLine(headers["X-Forwarded-Proto"]);
            sb.AppendLine(headers["X-Forwarded-Host"]);

            sb.AppendLine("-----original-----");
            sb.AppendLine(headers["X-Original-For"]);
            sb.AppendLine(headers["X-Original-Proto"]);
            sb.AppendLine(headers["X-Original-Host"]);

            return sb.ToString();
        }
    }
}