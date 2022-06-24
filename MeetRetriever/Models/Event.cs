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
        public BoardType Height { get; }
        public List<Diver> Divers { get; private set; }
        public int NumDives { get; }
        public DateTime Date { get; }

        public Event(string name, int id, int meetId, BoardType height, int numDives, DateTime date)
        {
            Name = name;
            Id = id;
            MeetId = meetId;
            Height = height;
            Divers = new List<Diver>();
            NumDives = numDives;
            Date = date;
        }

        public Event(string name, int id, int meetId, BoardType height, List<Diver> divers, int numDives, DateTime date)
        {
            Name = name;
            Id = id;
            MeetId = meetId;
            Height = height;
            Divers = divers;
            NumDives = numDives;
            Date = date;
        }

        public void Add_Diver(Diver diver)
        {
            Divers.Add(diver);
        }

        public void Add_Divers(IEnumerable<Diver> divers)
        {
            Divers.AddRange(divers);
        }
    }
}
