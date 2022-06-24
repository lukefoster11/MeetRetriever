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
            var currentMeetInfo = _meetScraper.GetMeetInfoOfType("Current Meets");
            var currentMeets = _meetScraper.GetMeets(currentMeetInfo);

            _logger.LogInformation($"Successfully retrieved {currentMeets.Count()} current meets");

            return currentMeets;
        }

        [HttpGet("upcoming")]
        public IEnumerable<Meet> GetUpcomingMeets()
        {
            var upcomingMeetInfo = _meetScraper.GetMeetInfoOfType("Upcoming Meets");
            var upcomingMeets = _meetScraper.GetMeets(upcomingMeetInfo);

            _logger.LogInformation($"Successfully retrieved {upcomingMeets.Count()} upcoming meets");

            return upcomingMeets;
        }
    }
}
