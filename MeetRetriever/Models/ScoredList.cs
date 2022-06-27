using System;
using System.Collections.Generic;

namespace MeetRetriever.Models
{
    public class ScoredList
    {
        public string DiverName { get; }
        public int DiverId { get; }
        public List<ScoredDive> DiveList { get; }

        public ScoredList(string diverName, int diverId, List<ScoredDive> diveList)
        {
            DiverName = diverName;
            DiverId = diverId;
            DiveList = diveList;
        }
    }
}
