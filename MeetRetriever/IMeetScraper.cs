using System.Collections.Generic;
using HtmlAgilityPack;
using MeetRetriever.Models;

namespace MeetRetriever
{
    public interface IMeetScraper
    {
        int errors { get; }

        IEnumerable<Meet> GetMeets(string meetType);

        Meet GetMeetInfo(int meetId);

        IEnumerable<Event> GetEvents(int meetId);

        Event GetEventInfo(int meetId, int eventId, int eventType);

        IEnumerable<Entry> GetEntries(int meetId, int eventId, int eventType);

        IEnumerable<Result> GetResults(int meetId, int eventId, int eventType);
    }
}