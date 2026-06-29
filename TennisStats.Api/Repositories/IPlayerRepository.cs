using System.Collections.Generic;
using System.Threading.Tasks;
using TennisStats.Api.Models;

namespace TennisStats.Api.Repositories
{
    public interface IPlayerRepository
    {
        Task<IEnumerable<Player>> GetAllAsync();
        Task<Player?> GetByIdAsync(int id);
        Task<Player> AddAsync(Player player);
    }
}
