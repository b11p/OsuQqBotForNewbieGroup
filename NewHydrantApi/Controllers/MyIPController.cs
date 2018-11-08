using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NewHydrantApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyIPController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MyIPController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-----direct-----");

            var connection = _httpContextAccessor.HttpContext.Connection;
            sb.AppendLine(new IPEndPoint(connection.RemoteIpAddress, connection.RemotePort).ToString());

            var request = _httpContextAccessor.HttpContext.Request;
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