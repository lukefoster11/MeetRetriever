using System;
using System.Collections.Generic;

namespace MeetRetriever.Models
{
    public class Event
    {
        public string Name { get; }
        public int Id { get; }
        public int? EventType { get; }
        public DateTime Date { get; }
        public int NumEntries { get; }

        public Event(string name, int id, int? eventType, DateTime date, int numEntries)
        {
            Name = name;
            Id = id;
            EventType = eventType;
            Date = date;
            NumEntries = numEntries;
        }
    }
}
