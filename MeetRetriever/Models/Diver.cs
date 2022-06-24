using System;

namespace MeetRetriever.Models
{
    public class Diver
    {
        public string Name { get; private set; }
        public int Id { get; private set; }

        public Diver(string name, int id)
        {
            Name = name;
            Id = id;
        }
    }
}
