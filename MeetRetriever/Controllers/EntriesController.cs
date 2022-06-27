using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MeetRetriever.Models;
using System.Collections.Generic;
using System.Linq;

namespace MeetRetriever.Controllers
{
    [ApiController]
    [Route("")]
    public class EntriesController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;
        private readonly IMeetScraper _meetScraper;

        public EntriesController(ILogger<InfoController> logger, IMeetScraper meetScraper)
        {
            _logger = logger;
            _meetScraper = meetScraper;
        }

        [HttpGet("{meetId}/{eventId}/{eventType}")]
        public IEnumerable<Entry> GetEntries(int meetId, int eventId, int eventType)
        {
            var entries = _meetScraper.GetEntries(meetId, eventId, eventType);

            _logger.LogInformation($"Successfully retrieved {entries.Count()} entries");
            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return entries;
        }
    }
}