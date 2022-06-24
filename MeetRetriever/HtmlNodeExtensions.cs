using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;

namespace MeetRetriever
{
    public static class HtmlAgilityPackExtensions
    {
        public static IEnumerable<HtmlNode> ProceedingSiblings(this HtmlNode htmlNode)
        {
            var siblings = new List<HtmlNode>();
            while (htmlNode.NextSibling != null)
            {
                siblings.Add(htmlNode.NextSibling);
                htmlNode = htmlNode.NextSibling;
            }
            return siblings;
        }

        public static IEnumerable<HtmlNode> Descendants(this HtmlNode htmlNode, string name, int level)
        {
            return htmlNode.Descendants(level).Where(x => x.Name == name);
        }
    }
}
