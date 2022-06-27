using System.Collections.Generic;
using HtmlAgilityPack;
using MeetRetriever.Models;

namespace MeetRetriever
{
    public interface IMeetScraper
    {
        int errors { get; }

        IEnumerable<MeetSummary> GetMeets(string meetType);

        MeetSummary GetMeetInfo(int meetId);

        IEnumerable<Event> GetEvents(int meetId);

        IEnumerable<Event> GetEventInfo(int meetId, int eventId);
        Event GetEventInfo(int meetId, int eventId, int eventType);

        IEnumerable<Entry> GetEntries(int meetId, int eventId, int eventType);

        IEnumerable<Result> GetResults(int meetId, int eventId, int eventType);
    }
}