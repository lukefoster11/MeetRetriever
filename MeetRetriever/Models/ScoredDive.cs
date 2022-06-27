using System;
using System.Collections.Generic;

namespace MeetRetriever.Models
{
    public class ScoredDive
    {
        public Dive Dive { get; }
        public IEnumerable<float> Scores { get; }

        public ScoredDive(Dive dive, IEnumerable<float> scores)
        {
            Dive = dive;
            Scores = scores;
        }
    }
}
