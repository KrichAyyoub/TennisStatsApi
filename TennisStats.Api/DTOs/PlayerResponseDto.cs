using System.Collections.Generic;

namespace TennisStats.Api.DTOs
{
    public class CountryDto
    {
        public string Picture { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class PlayerDataDto
    {
        public int Rank { get; set; }
        public int Points { get; set; }
        public int Weight { get; set; } // in grams
        public int Height { get; set; } // in cm
        public int Age { get; set; }
        public List<int> Last { get; set; } = new();
    }

    public class PlayerResponseDto
    {
        public int Id { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Shortname { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty; // "M" or "F"
        public CountryDto Country { get; set; } = new();
        public string Picture { get; set; } = string.Empty;
        public PlayerDataDto Data { get; set; } = new();
    }
}
