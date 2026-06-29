namespace TennisStats.Api.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Shortname { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty; // "M" or "F"
        public CountryInfo Country { get; set; } = new();
        public string Picture { get; set; } = string.Empty;
        public PlayerData Data { get; set; } = new();
    }
}
