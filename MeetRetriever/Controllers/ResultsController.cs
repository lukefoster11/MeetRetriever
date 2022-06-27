using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MeetRetriever.Models;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;

namespace MeetRetriever.Controllers
{
    [ApiController]
    [Route("")]
    public class ResultsController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;
        private readonly IMeetScraper _meetScraper;

        public ResultsController(ILogger<InfoController> logger, IMeetScraper meetScraper)
        {
            _logger = logger;
            _meetScraper = meetScraper;
        }

        [HttpGet("{meetId}/{eventId}/{eventType}/results")]
        public IEnumerable<Result> GetResults(int meetId, int eventId, int eventType)
        {
            var results = _meetScraper.GetResults(meetId, eventId, eventType);

            _logger.LogInformation($"Successfully retrieved {results.Count()} results");
            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return results;
        }

        /*
        [HttpGet("{meetId}/results")]
        public IEnumerable<Result> GetAllResults(int meetId)
        {
            var results = _meetScraper.GetAllResults(meetId);

            _logger.LogInformation($"Successfully retrieved {results.Count()} results");
            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return results;
        }
        */

        /*
        [HttpGet("{meetId}/teamresults")]
        public IEnumerable<Result> GetTeamResults(int meetId)
        {
            var results = _meetScraper.GetTeamResults(meetId);

            _logger.LogInformation($"Successfully retrieved {results.Count()} results");
            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return results;
        }
        */

        /*
        [HttpGet("{meetId}/highpointresults")]
        public IEnumerable<Result> GetHighPointResults(int meetId)
        {
            var results = _meetScraper.GetHighPointResults(meetId);

            _logger.LogInformation($"Successfully retrieved {results.Count()} results");
            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return results;
        }
        */
    }
}