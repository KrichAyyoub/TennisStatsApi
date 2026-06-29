using System.Collections.Generic;
using System.Threading.Tasks;
using TennisStats.Api.DTOs;

namespace TennisStats.Api.Services
{
    public interface IPlayerService
    {
        Task<IEnumerable<PlayerResponseDto>> GetPlayersAsync(string? sex = null);
        Task<PlayerResponseDto> GetPlayerByIdAsync(int id);
        Task<PlayerResponseDto> CreatePlayerAsync(CreatePlayerRequestDto request);
        Task<StatsResponseDto> GetStatsAsync();
        Task<bool> DeletePlayerAsync(int id);
        Task<double> GetPlayerWinRatioAsync(int id);
    }
}
