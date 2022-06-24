using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeetRetriever.Controllers
{
    [ApiController]
    [Route("")]
    public class InfoController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;

        public InfoController(ILogger<InfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string GetInfo()
        {
            return "Welcome to MeetRetriever API";
        }
    }
}
