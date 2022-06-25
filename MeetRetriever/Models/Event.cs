using System;
using System.Collections.Generic;

namespace MeetRetriever.Models
{
    public class Event
    {
        public enum BoardType
        {
            OneMeter,
            ThreeMeter,
            Platform
        }

        public string Name { get; }
        public int Id { get; }
        public int MeetId { get; }
        public int? EventType { get; }
        public DateTime Date { get; }

        public Event(string name, int id, int meetId, DateTime date)
        {
            Name = name;
            Id = id;
            MeetId = meetId;
            EventType = null;
            Date = date;
        }

        public Event(string name, int id, int meetId, int eventType, DateTime date)
        {
            Name = name;
            Id = id;
            MeetId = meetId;
            EventType = eventType;
            Date = date;
        }
    }
}
