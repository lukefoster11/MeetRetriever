using System;
namespace MeetRetriever.Models
{
    public class Dive
    {
        public string Code { get; }
        public int Height { get; }

        public Dive(string code, int height)
        {
            Code = code;
            Height = height;
        }
    }
}
