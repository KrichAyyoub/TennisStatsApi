using System.Collections.Generic;

namespace TennisStats.Api.Models
{
    public class PlayerData
    {
        public int Rank { get; set; }
        public int Points { get; set; }
        public int Weight { get; set; } // in grams
        public int Height { get; set; } // in cm
        public int Age { get; set; }
        public List<int> Last { get; set; } = new();
    }
}
