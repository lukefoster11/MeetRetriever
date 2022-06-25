using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;
using MeetRetriever.Models;

namespace MeetRetriever
{
    public class MeetScraper : IMeetScraper
    {
        public HtmlDocument GetHtmlDocument(string url)
        {
            var web = new HtmlWeb();
            return web.Load(url);
        }

        public HtmlNode GetMeetInfoNode()
        {
            var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/index.php#");
            var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
            return contentNode.Descendants("div").Where(x => x.Descendants("table").Count() > 0).FirstOrDefault();
        }

        public (DateTime, DateTime) ParseDates(string dates)
        {
            var sep = dates.Split(", ");
            var year = sep.LastOrDefault();
            sep = sep.FirstOrDefault().Split(" - ");
            var startDate = DateTime.Parse($"{sep.FirstOrDefault()}, {year}");
            var endDate = DateTime.Parse($"{sep.LastOrDefault()}, {year}");
            return (startDate, endDate);
        }

        public IEnumerable<HtmlNode> GetMeetInfoOfType(string meetType)
        {
            var meetInfoNode = GetMeetInfoNode();

            var meetsDiv = meetInfoNode
                .Descendants("div")
                .Where(x => x.InnerText == meetType)
                .FirstOrDefault();
            if (meetsDiv == null)
            {
                return null;
            }

            var meetsTable = meetsDiv
                .ProceedingSiblings()
                .Where(x => x.OriginalName == "table")
                .FirstOrDefault();
            if (meetsTable == null)
            {
                return null;
            }

            var meetsTableRows = meetsTable
                .ChildNodes
                .Where(x => x.OriginalName == "tr");
            if (meetsTableRows == null)
            {
                return null;
            }

            var meetsInfo = meetsTableRows
                .Select(x => x.Descendants("table")
                .LastOrDefault());

            return meetsInfo;
        }

        public IEnumerable<Meet> GetMeets(IEnumerable<HtmlNode> meetInfoNodes)
        {
            var meets = new List<Meet>();

            if (meetInfoNodes == null)
            {
                return meets;
            }

            foreach (var meetNode in meetInfoNodes)
            {
                var nameAndIdNode = meetNode
                    .Descendants("a")
                    .Where(x => x.GetAttributeValue("href", "").Contains("meetnum="))
                    .FirstOrDefault();
                var name = nameAndIdNode.InnerText;
                var id = int.Parse(nameAndIdNode
                    .GetAttributeValue("href", "")
                    .Split("meetnum=")
                    .LastOrDefault());
                var locationAndDateNode = meetNode
                    .Descendants("tr")
                    .LastOrDefault();
                var locationNode = locationAndDateNode.FirstChild;
                var location = locationNode.InnerText;
                var dates = ParseDates(locationNode.NextSibling.InnerText);

                meets.Add(new Meet(name, id, location, dates.Item1, dates.Item2));
            }

            return meets;
        }

        public HtmlNode GetEventInfoTable(int meetId)
        {
            var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/meetinfoext.php?meetnum=" + meetId.ToString());
            var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
            var tableNode = contentNode.ChildNodes.Where(x => x.Name == "table").FirstOrDefault();
            if (tableNode == null)
            {
                return null;
            }

            return tableNode.Descendants("table").LastOrDefault();
        }


        public IEnumerable<Event> GetEvents(int meetId, HtmlNode table)
        {
            var tableRows = table.ChildNodes.Where(x => x.OriginalName == "tr");

            var rowsOfInterest = tableRows.Where(x => x.Descendants("a")
                .Where(x => x.GetAttributeValue("href", "").Contains("event"))
                .Count() > 0 || x.Descendants("strong").Count() > 0);

            var events = new List<Event>();
            var date = DateTime.Today;

            foreach (var row in rowsOfInterest)
            {
                var infoNode = row.Descendants("a").FirstOrDefault();

                if (infoNode != null)
                {
                    var eventInfoNode = infoNode.ParentNode;
                    var nameNode = eventInfoNode.Descendants("#text").FirstOrDefault();
                    var name = nameNode.InnerText.Split("&nbsp").FirstOrDefault().Trim();

                    var hasEntries = eventInfoNode.Descendants("a").Where(x => x.InnerText.Contains("Entries")).Count() > 0;
                    if (hasEntries)
                    {
                        var linkNode = eventInfoNode.Descendants("a").Where(x => x.InnerText.Contains("Entries")).FirstOrDefault();
                        int.TryParse(linkNode.GetAttributeValue("href", "").Split("&eventtype").FirstOrDefault().Split("eventnum=").LastOrDefault(), out int id);

                        int.TryParse(linkNode.GetAttributeValue("href", "").Split("eventtype=").LastOrDefault(), out int eventType);

                        events.Add(new Event(name, id, meetId, eventType, date));
                    }
                    else
                    {
                        var linkNode = eventInfoNode.Descendants("a").Where(x => x.InnerText == "Rule").FirstOrDefault();
                        int.TryParse(linkNode.GetAttributeValue("href", "").Split("event=").LastOrDefault(), out int id);

                        events.Add(new Event(name, id, meetId, date));
                    }
                }
                else
                {
                    infoNode = row.Descendants("strong").FirstOrDefault();
                    if (infoNode != null)
                    {
                        var tempDate = date;
                        if (DateTime.TryParse(infoNode.InnerText, out tempDate))
                        {
                            date = tempDate;
                        }
                    }
                }
            }

            return events;
        }
    }
}
