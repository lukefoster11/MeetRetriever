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


        // MEETS CONTROLLER

        public IEnumerable<MeetSummary> GetMeets(string meetType)
        {
            var meets = new List<MeetSummary>();

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

        public MeetSummary GetMeetInfo(int meetId)
        {
            try
            {
                var meetInfoNodes = GetMeetInfoNodes(meetId);

                var nameNode = meetInfoNodes.Where(x => x.Descendants("td").FirstOrDefault().InnerText.Replace("\t", "").Replace("\n", "").Replace("\r", "").ToLower() == "name:").FirstOrDefault();
                var name = nameNode.Descendants("strong").LastOrDefault().InnerText;

                var locationNode = meetInfoNodes.Where(x => x.Descendants("td").FirstOrDefault().InnerText.Replace("\t", "").Replace("\n", "").Replace("\r", "").ToLower() == "pool:").FirstOrDefault();
                var location = locationNode.Descendants("strong").LastOrDefault().Descendants("#text").ElementAtOrDefault(2).InnerText;

                var startDateNode = meetInfoNodes.Where(x => x.Descendants("td").FirstOrDefault().InnerText.Replace("\t", "").Replace("\n", "").Replace("\r", "").ToLower() == "start date:").FirstOrDefault();
                DateTime.TryParse(startDateNode.Descendants("strong").LastOrDefault().InnerText, out DateTime startDate);

                var endDateNode = meetInfoNodes.Where(x => x.Descendants("td").FirstOrDefault().InnerText.Replace("\t", "").Replace("\n", "").Replace("\r", "").ToLower() == "end date:").FirstOrDefault();
                DateTime.TryParse(endDateNode.Descendants("strong").LastOrDefault().InnerText, out DateTime endDate);

                return new MeetSummary(name, meetId, location, startDate, endDate);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }


        // EVENTS CONTROLLER

        public IEnumerable<Event> GetEvents(int meetId)
        {
            var events = new List<Event>();

            try
            {
                var eventInfoNodes = GetEventInfoNodes(meetId);

                var date = DateTime.Today;

                foreach (var row in eventInfoNodes)
                {
                    var infoNode = row.Descendants("a").FirstOrDefault();

                    if (infoNode != null)
                    {
                        events = AddEventInfoToList(infoNode, date, events);
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

        public IEnumerable<Event> GetEventInfo(int meetId, int eventId)
        {
            var events = new List<Event>();

            try
            {
                var eventInfoNodes = GetEventInfoNodes(meetId);

                eventInfoNodes = eventInfoNodes.Where(x => x.Descendants("a").Where(y => y.GetAttributeValue("href", "").Contains($"event={eventId}")).Count() > 0);

                foreach (var eventInfoNode in eventInfoNodes)
                {
                    try
                    {
                        var nameNode = eventInfoNode.Descendants("a").FirstOrDefault().ParentNode.Descendants("#text").FirstOrDefault();
                        var name = nameNode.InnerText.Split("&nbsp").FirstOrDefault().Trim();

                        var date = GetRecentDate(eventInfoNode);

                        var hasEntries = eventInfoNode.Descendants("a").Where(x => x.InnerText.Contains("Entries")).Count() > 0;
                        if (hasEntries)
                        {
                            var linkNode = eventInfoNode.Descendants("a").Where(x => x.InnerText.Contains("Entries")).FirstOrDefault();

                            int.TryParse(linkNode.GetAttributeValue("href", "").Split("eventtype=").LastOrDefault(), out int eventType);

                            int.TryParse(linkNode.InnerText.Split("Entries").FirstOrDefault().Trim(), out int numEntries);

                            events.Add(new Event(name, eventId, eventType, date, numEntries));
                        }
                        else
                        {
                            events.Add(new Event(name, eventId, null, date, 0));
                        }
                    }
                    catch (NullReferenceException)
                    {
                        errors++;
                    }
                }
            }
            catch (NullReferenceException) { }

            return events;
        }

        public Event GetEventInfo(int meetId, int eventId, int eventType)
        {
            try
            {
                var eventInfoNodes = GetEventInfoNodes(meetId);

                var eventInfoNode = eventInfoNodes.Where(x => x.Descendants("a").Where(y => y.GetAttributeValue("href", "").Contains($"event={eventId}&eventtype={eventType}")).Count() > 0).FirstOrDefault();

                var nameNode = eventInfoNode.Descendants("a").FirstOrDefault().ParentNode.Descendants("#text").FirstOrDefault();
                var name = nameNode.InnerText.Split("&nbsp").FirstOrDefault().Trim();

                var date = GetRecentDate(eventInfoNode);

                var linkNode = eventInfoNode.Descendants("a").Where(x => x.InnerText.Contains("Entries")).FirstOrDefault();
                int.TryParse(linkNode.GetAttributeValue("href", "").Split("&eventtype").FirstOrDefault().Split("eventnum=").LastOrDefault(), out int id);

                int.TryParse(linkNode.InnerText.Split("Entries").FirstOrDefault().Trim(), out int numEntries);

                return new Event(name, id, eventType, date, numEntries);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }


        // ENTRIES CONTROLLER

        public IEnumerable<Entry> GetEntries(int meetId, int eventId, int eventType)
        {
            var entries = new List<Entry>();

            try
            {
                var tableRows = GetEntryInfoNodes(meetId, eventId, eventType);

                var diverName = "";
                var diverId = 0;
                var dives = new List<Dive>();

                foreach (var row in tableRows)
                {
                    try
                    {
                        if (row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("profile.php?number=")).Count() > 0)
                        {
                            UpdateDiver(row, out diverName, out diverId, out dives);
                            
                        }
                        else if (row.Descendants("strong").Count() == 0)
                        {
                            dives.Add(ExtractDiveFromRow(row));
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
                        errors++;
                    }

                }
            }
            catch (NullReferenceException) { }

            return entries;
        }


        // RESULTS CONTROLLER

        public IEnumerable<Result> GetResults(int meetId, int eventId, int eventType)
        {
            var results = new List<Result>();

            try
            {
                var tableRows = GetResultInfoNodes(meetId, eventId, eventType);

                foreach (var row in tableRows)
                {
                    results = AddResultInfoToList(row, results);
                }
            }
            catch (NullReferenceException) { }

            return results;
        }


        // HELPERS
        // general

        public HtmlDocument GetHtmlDocument(string url)
        {
            var web = new HtmlWeb();
            return web.Load(url);
        }

        // meets

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

        public IEnumerable<HtmlNode> GetMeetInfoNodes(int meetId)
        {
            try
            {
                var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/meetinfoext.php?meetnum=" + meetId.ToString());
                var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
                var tableNode = contentNode.ChildNodes.Where(x => x.Name == "table").FirstOrDefault();
                return tableNode.ChildNodes.Where(x => x.Name == "tr");
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

        public List<MeetSummary> AddMeetInfoToList(HtmlNode meetNode, List<MeetSummary> meets)
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

                meets.Add(new MeetSummary(name, id, location, dates.Item1, dates.Item2));
            }
            catch (NullReferenceException)
            {
                errors++;
            }

            return meets;
        }

        // events

        public IEnumerable<HtmlNode> GetEventInfoNodes(int meetId)
        {
            try
            {
                var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/meetinfoext.php?meetnum=" + meetId.ToString());
                var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
                var tableNode = contentNode.ChildNodes.Where(x => x.Name == "table").FirstOrDefault();

                var table = tableNode.Descendants("table").LastOrDefault();

                var tableRows = table.ChildNodes.Where(x => x.OriginalName == "tr");

                return tableRows.Where(x => x.Descendants("a")
                    .Where(x => x.GetAttributeValue("href", "").Contains("event"))
                    .Count() > 0 || x.Descendants("strong").Count() > 0);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public List<Event> AddEventInfoToList(HtmlNode infoNode, DateTime date, List<Event> events)
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
                    int.TryParse(linkNode.InnerText.Split("Entries").FirstOrDefault().Trim(), out int numEntries);

                    events.Add(new Event(name, id, eventType, date, numEntries));
                }
                else
                {
                    var linkNode = eventInfoNode.Descendants("a").Where(x => x.InnerText == "Rule").FirstOrDefault();
                    int.TryParse(linkNode.GetAttributeValue("href", "").Split("event=").LastOrDefault(), out int id);

                    events.Add(new Event(name, id, null, date, 0));
                }
            }
            catch (NullReferenceException)
            {
                errors++;
            }

            return events;
        }

        public DateTime UpdateDate(HtmlNode row, DateTime date)
        {
            try
            {
                var infoNode = row.Descendants("strong").FirstOrDefault();
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
                errors++;
            }

            return date;
        }

        public DateTime GetRecentDate(HtmlNode row)
        {
            var reachedDate = false;
            DateTime date = DateTime.MinValue;
            while (!reachedDate)
            {
                if (row.Descendants("strong").Count() > 0)
                {
                    var infoNode = row.Descendants("strong").FirstOrDefault();
                    if (DateTime.TryParse(infoNode.InnerText, out date))
                    {
                        reachedDate = true;
                    } else
                    {
                        row = row.PreviousSibling;
                    }
                }
                else
                {
                    row = row.PreviousSibling;
                }
            }
            return date;
        }

        // entries

        public IEnumerable<HtmlNode> GetEntryInfoNodes(int meetId, int eventId, int eventType)
        {
            try
            {
                var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/divesheetext.php?meetnum=" + meetId.ToString() + @"&eventnum=" + eventId.ToString() + @"&eventtype=" + eventType.ToString());
                var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
                var tableNode = contentNode.ChildNodes.Where(x => x.Name == "table").FirstOrDefault();

                var table = tableNode.Descendants("table").LastOrDefault();
                return table.ChildNodes.Where(x => x.OriginalName == "tr");
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public void UpdateDiver(HtmlNode row, out string diverName, out int diverId, out List<Dive> dives)
        {
            diverName = row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("profile.php?number=")).FirstOrDefault().InnerText;
            int.TryParse(row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("profile.php?number=")).FirstOrDefault().GetAttributeValue("href", "").Split("number=").LastOrDefault(), out diverId);
            dives = new List<Dive>();
        }

        public Dive ExtractDiveFromRow(HtmlNode row)
        {
            var diveInfo = row.Descendants("td");
            var diveCode = diveInfo.ElementAt(1).InnerText;
            diveCode = diveCode.Split("\t").FirstOrDefault();
            var diveHeightString = diveInfo.ElementAt(2).InnerText;
            diveHeightString = diveHeightString.Split("M").FirstOrDefault();
            int.TryParse(diveHeightString, out int diveHeight);
            return new Dive(diveCode, diveHeight);
        }

        // results

        public IEnumerable<HtmlNode> GetResultInfoNodes(int meetId, int eventId, int eventType)
        {
            var doc = GetHtmlDocument(@"https://secure.meetcontrol.com/divemeets/system/eventresultsext.php?meetnum=" + meetId.ToString() + @"&eventnum=" + eventId.ToString() + @"&eventtype=" + eventType.ToString());
            var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='dm_content']");
            var tableNode = contentNode.ChildNodes.Where(x => x.Name == "table").FirstOrDefault();
            return tableNode.Descendants("tr").Where(x => x.Descendants("a").Where(y => y.GetAttributeValue("href", "").Contains("profile.php?number=")).Count() > 0);
        }

        public List<Result> AddResultInfoToList(HtmlNode row, List<Result> results)
        {
            try
            {
                var nameAndIdInfoNode = row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("profile.php?number=")).FirstOrDefault();
                var diverName = nameAndIdInfoNode.Descendants("#text").FirstOrDefault().InnerText;
                int.TryParse(nameAndIdInfoNode.GetAttributeValue("href", "").Split("number=").LastOrDefault(), out int diverId);

                var idAndScoreInfoNode = row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("divesheetresultsext.php?")).FirstOrDefault();
                int.TryParse(idAndScoreInfoNode.GetAttributeValue("href", "").Split("&sts=").LastOrDefault(), out int id);
                float.TryParse(idAndScoreInfoNode.Descendants("#text").FirstOrDefault().InnerText, out float score);

                var teamInfoNode = row.Descendants("a").Where(x => x.GetAttributeValue("href", "").Contains("profilet.php?number=")).FirstOrDefault();
                var teamName = teamInfoNode.Descendants("#text").FirstOrDefault().InnerText;
                int.TryParse(teamInfoNode.GetAttributeValue("href", "").Split("number=").LastOrDefault(), out int teamId);

                results.Add(new Result(diverName, diverId, id, score, teamName, teamId));
            }
            catch (NullReferenceException)
            {
                errors++;
            }
            return results;
        }
    }
}
