using Outage.Common;
using System.Web.Http;

namespace OMS.Web.API.Controllers
{
    public class LogController : ApiController
    {
        private readonly ILogger _logger;

        public LogController()
        {
        }

        public LogController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public string Post([FromBody]string message)
        {
            _logger.LogInfo(message);
            return message;
        }
    }
}
