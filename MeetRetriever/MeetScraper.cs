using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;
using MeetRetriever.Models;

namespace MeetRetriever
{
    public class MeetScraper : IMeetScraper
    {
        public int errors { get; private set; }

        public IEnumerable<Meet> GetMeets(string meetType)
        {
            var meets = new List<Meet>();

            try
            {
                var meetInfoNodes = GetMeetInfoOfType(meetType);

                foreach (var meetNode in meetInfoNodes)
                {
                    meets = AddMeetInfoToList(meetNode, meets);
                }
            }
            catch (NullReferenceException) { }

            return meets;
        }

        public IEnumerable<Event> GetEvents(int meetId)
        {
            var events = new List<Event>();

            try
            {
                var table = GetEventInfoTable(meetId);
                var tableRows = table.ChildNodes.Where(x => x.OriginalName == "tr");

                var rowsOfInterest = tableRows.Where(x => x.Descendants("a")
                    .Where(x => x.GetAttributeValue("href", "").Contains("event"))
                    .Count() > 0 || x.Descendants("strong").Count() > 0);

                var date = DateTime.Today;

                foreach (var row in rowsOfInterest)
                {
                    var infoNode = row.Descendants("a").FirstOrDefault();

                    if (infoNode != null)
                    {
                        events = AddEventInfoToList(infoNode, meetId, date, events);
                    }
                    else
                    {
                        date = UpdateDate(row, date);
                    }
                }
            }
            catch (NullReferenceException) { }

            return events;
        }

        public IEnumerable<Entry> GetEntries(int meetId, int eventId, int eventType)
        {
            var entries = new List<Entry>();

            try
            {
                var table = GetEntryInfoTable(meetId, eventId, eventType);

                var tableRows = table.ChildNodes.Where(x => x.OriginalName == "tr");

                var diverName = "";
                var diverId = 0;
                var dives = new List<Dive>();

                foreach (var row in tableRows)
                {
                    try
                    {
                        if (row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("profile.php?number=")).Count() > 0)
                        {
                            diverName = row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("profile.php?number=")).FirstOrDefault().InnerText;
                            int.TryParse(row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("profile.php?number=")).FirstOrDefault().GetAttributeValue("href", "").Split("number=").LastOrDefault(), out diverId);
                            dives = new List<Dive>();
                        }
                        else if (row.Descendants("strong").Count() == 0)
                        {
                            var diveInfo = row.Descendants("td");
                            var diveCode = diveInfo.ElementAt(1).InnerText;
                            diveCode = diveCode.Split("\t").FirstOrDefault();
                            var diveHeightString = diveInfo.ElementAt(2).InnerText;
                            diveHeightString = diveHeightString.Split("M").FirstOrDefault();
                            int.TryParse(diveHeightString, out int diveHeight);
                            dives.Add(new Dive(diveCode, diveHeight));
                        }

                        else if (row.Descendants("strong").Where(x => x.InnerText.Contains("DD Total")).Count() > 0)
                        {
                            if (diverName != "" && diverId != 0)
                            {
                                entries.Add(new Entry(diverName, diverId, dives));
                            }
                        }
                    }
                    catch (NullReferenceException)
                    {
                        errors += 1;
                    }

                }
            }
            catch (NullReferenceException) { }

            return entries;
        }

        public HtmlDocument GetHtmlDocument(string url)
        {
            var web = new HtmlWeb();
            return web.Load(url);
        }

        public HtmlNode GetMeetInfoNode()
        {
            try
            {
                var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/index.php#");
                var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
                return contentNode.Descendants("div").Where(x => x.Descendants("table").Count() > 0).FirstOrDefault();
            }
            catch (NullReferenceException)
            {
                return null;
            }
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
            try
            {
                var meetInfoNode = GetMeetInfoNode();

                var meetsDiv = meetInfoNode
                    .Descendants("div")
                    .Where(x => x.InnerText == meetType)
                    .FirstOrDefault();
                var meetsTable = meetsDiv
                    .ProceedingSiblings()
                    .Where(x => x.OriginalName == "table")
                    .FirstOrDefault();
                var meetsTableRows = meetsTable
                    .ChildNodes
                    .Where(x => x.OriginalName == "tr");
                var meetsInfo = meetsTableRows
                    .Select(x => x.Descendants("table")
                    .LastOrDefault());

                return meetsInfo;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public List<Meet> AddMeetInfoToList(HtmlNode meetNode, List<Meet> meets)
        {
            try
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
            catch (NullReferenceException)
            {
                errors += 1;
            }

            return meets;
        }

        public HtmlNode GetEventInfoTable(int meetId)
        {
            try
            {
                var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/meetinfoext.php?meetnum=" + meetId.ToString());
                var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
                var tableNode = contentNode.ChildNodes.Where(x => x.Name == "table").FirstOrDefault();

                return tableNode.Descendants("table").LastOrDefault();
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public List<Event> AddEventInfoToList(HtmlNode infoNode, int meetId, DateTime date, List<Event> events)
        {
            try
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

                    events.Add(new Event(name, id, eventType, date));
                }
                else
                {
                    var linkNode = eventInfoNode.Descendants("a").Where(x => x.InnerText == "Rule").FirstOrDefault();
                    int.TryParse(linkNode.GetAttributeValue("href", "").Split("event=").LastOrDefault(), out int id);

                    events.Add(new Event(name, id, null, date));
                }
            }
            catch (NullReferenceException)
            {
                errors += 1;
            }

            return events;
        }

        public DateTime UpdateDate(HtmlNode row, DateTime date)
        {
            try
            {
                HtmlNode infoNode = row.Descendants("strong").FirstOrDefault();
                if (infoNode != null)
                {
                    var tempDate = date;
                    if (DateTime.TryParse(infoNode.InnerText, out tempDate))
                    {
                        date = tempDate;
                    }
                }
            }
            catch (NullReferenceException)
            {
                errors += 1;
            }

            return date;
        }

        public HtmlNode GetEntryInfoTable(int meetId, int eventId, int eventType)
        {
            try
            {
                var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/divesheetext.php?meetnum=" + meetId.ToString() + @"&eventnum=" + eventId.ToString() + @"&eventtype=" + eventType.ToString());
                var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
                var tableNode = contentNode.ChildNodes.Where(x => x.Name == "table").FirstOrDefault();

                return tableNode.Descendants("table").LastOrDefault();
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }
    }
}
