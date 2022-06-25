using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MeetRetriever.Models;
using System.Collections.Generic;
using System.Linq;

namespace MeetRetriever.Controllers
{
    [ApiController]
    [Route("meets")]
    public class MeetsController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;
        private readonly IMeetScraper _meetScraper;

        public MeetsController(ILogger<InfoController> logger, IMeetScraper meetScraper)
        {
            _logger = logger;
            _meetScraper = meetScraper;
        }
        
        [HttpGet]
        public IEnumerable<Meet> GetCurrentMeets()
        {
            var currentMeets = _meetScraper.GetMeets("Current Meets");

            _logger.LogInformation($"Successfully retrieved {currentMeets.Count()} current meets");
            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return currentMeets;
        }

        [HttpGet("upcoming")]
        public IEnumerable<Meet> GetUpcomingMeets()
        {
            var upcomingMeets = _meetScraper.GetMeets("Upcoming Meets");

            _logger.LogInformation($"Successfully retrieved {upcomingMeets.Count()} upcoming meets");
            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return upcomingMeets;
        }
    }
}
