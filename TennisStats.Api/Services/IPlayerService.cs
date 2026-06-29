using System.Collections.Generic;
using System.Threading.Tasks;
using TennisStats.Api.DTOs;

namespace TennisStats.Api.Services
{
    public interface IPlayerService
    {
        Task<IEnumerable<PlayerResponseDto>> GetPlayersAsync();
        Task<PlayerResponseDto> GetPlayerByIdAsync(int id);
        Task<PlayerResponseDto> CreatePlayerAsync(CreatePlayerRequestDto request);
        Task<StatsResponseDto> GetStatsAsync();
    }
}
