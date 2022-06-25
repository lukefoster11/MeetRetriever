using System.Collections.Generic;
using HtmlAgilityPack;
using MeetRetriever.Models;

namespace MeetRetriever
{
    public interface IMeetScraper
    {
        int errors { get; }

        IEnumerable<Meet> GetMeets(string meetType);

        IEnumerable<Event> GetEvents(int meetId);

        IEnumerable<Entry> GetEntries(int meetId, int eventId, int eventType);
    }
}