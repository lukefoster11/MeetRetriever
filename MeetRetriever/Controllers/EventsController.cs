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
    public class EventsController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;
        private readonly IMeetScraper _meetScraper;

        public EventsController(ILogger<InfoController> logger, IMeetScraper meetScraper)
        {
            _logger = logger;
            _meetScraper = meetScraper;
        }

        [HttpGet("{meetId}/events")]
        public IEnumerable<Event> GetEvents(int meetId)
        {
            var events = _meetScraper.GetEvents(meetId);

            _logger.LogInformation($"Successfully retrieved {events.Count()} events");
            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return events;
        }

        [HttpGet("{meetId}/{eventId}")]
        public IEnumerable<Event> GetEventInfo(int meetId, int eventId)
        {
            var _event = _meetScraper.GetEventInfo(meetId, eventId);

            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return _event;
        }

        [HttpGet("{meetId}/{eventId}/{eventType}")]
        public Event GetEventInfo(int meetId, int eventId, int eventType)
        {
            var _event = _meetScraper.GetEventInfo(meetId, eventId, eventType);

            if (_meetScraper.errors > 0)
            {
                _logger.LogError($"Encountered {_meetScraper.errors} errors");
            }

            return _event;
        }
    }
}