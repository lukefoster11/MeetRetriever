using System.Collections.Generic;
using HtmlAgilityPack;
using MeetRetriever.Models;

namespace MeetRetriever
{
    public interface IMeetScraper
    {
        IEnumerable<HtmlNode> GetMeetInfoOfType(string meetType);
        IEnumerable<Meet> GetMeets(IEnumerable<HtmlNode> htmlNodes);
    }
}