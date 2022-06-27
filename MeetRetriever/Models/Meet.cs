using System;

namespace MeetRetriever.Models
{
    public class Meet
    {
        public string Name { get; }
        public int Id { get; }
        public string Location { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        //TODO: (maybe) include all entered divers

        public Meet(string name, int id, string location, DateTime startDate, DateTime endDate)
        {
            Name = name;
            Id = id;
            Location = location;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
