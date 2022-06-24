using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;
using MeetRetriever.Models;

namespace MeetRetriever
{
    public class MeetScraper : IMeetScraper
    {
        public HtmlDocument GetHtmlDocument()
        {
            var url = @"https://secure.meetcontrol.com/divemeets/system/index.php#";
            var web = new HtmlWeb();
            return web.Load(url);
        }

        public HtmlNode GetMeetInfoNode()
        {
            var doc = GetHtmlDocument();
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
    }
}
