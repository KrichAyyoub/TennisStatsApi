namespace TennisStats.Api.DTOs
{
    public class CountryWinRatioDto
    {
        public string CountryCode { get; set; } = string.Empty;
        public double WinRatio { get; set; }
    }

    public class StatsResponseDto
    {
        public CountryWinRatioDto CountryWithBestWinRatio { get; set; } = new();
        public double AverageBMI { get; set; }
        public double MedianHeight { get; set; }
    }
}
