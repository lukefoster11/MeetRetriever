using System;

namespace MeetRetriever.Models
{
    public class Result
    {
        public string DiverName { get; }
        public int DiverId { get; }
        public int Id { get; }
        public float Score { get; }
        public string TeamName { get; }
        public int TeamId { get; }

        public Result(string diverName, int diverId, int id, float score, string teamName, int teamId)
        {
            DiverName = diverName;
            DiverId = diverId;
            Id = id;
            Score = score;
            TeamName = teamName;
            TeamId = teamId;
        }
    }
}
