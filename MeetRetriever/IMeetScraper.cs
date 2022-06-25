using System.Collections.Generic;
using HtmlAgilityPack;
using MeetRetriever.Models;

namespace MeetRetriever
{
    public interface IMeetScraper
    {
        int errors { get; }

        IEnumerable<HtmlNode> GetMeetInfoOfType(string meetType);
        IEnumerable<Meet> GetMeets(IEnumerable<HtmlNode> htmlNodes);

        HtmlNode GetEventInfoTable(int meetId);
        IEnumerable<Event> GetEvents(int meetId, HtmlNode table);
    }
}