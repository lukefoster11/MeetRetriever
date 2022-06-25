using System.Collections.Generic;

namespace MeetRetriever.Models
{
    public class Entry
    {
        public string DiverName { get; }
        public int DiverId { get; }
        public List<Dive> DiveList { get; }

        public Entry(string diverName, int diverId, List<Dive> diveList)
        {
            DiverName = diverName;
            DiverId = diverId;
            DiveList = diveList;
        }
    }
}
