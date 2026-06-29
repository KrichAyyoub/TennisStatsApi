using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TennisStats.Api.Models;

namespace TennisStats.Api.Repositories
{
    public class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly List<Player> _players = new();
        private readonly object _lock = new();
        private readonly ILogger<InMemoryPlayerRepository> _logger;

        public InMemoryPlayerRepository(IHostEnvironment env, ILogger<InMemoryPlayerRepository> logger)
        {
            _logger = logger;
            LoadPlayers(env.ContentRootPath);
        }

        private void LoadPlayers(string contentRootPath)
        {
            try
            {
                // Try several paths to be robust (ContentRootPath, AppContext.BaseDirectory, Current Directory)
                var pathsToTry = new[]
                {
                    Path.Combine(contentRootPath, "data", "players.json"),
                    Path.Combine(AppContext.BaseDirectory, "data", "players.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "data", "players.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "TennisStats.Api", "data", "players.json")
                };

                string? filePath = null;
                foreach (var path in pathsToTry)
                {
                    if (File.Exists(path))
                    {
                        filePath = path;
                        break;
                    }
                }

                if (filePath == null)
                {
                    _logger.LogWarning("players.json file not found in any of the expected locations. Initializing empty player repository.");
                    return;
                }

                _logger.LogInformation("Loading player data from {FilePath}", filePath);
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var container = JsonSerializer.Deserialize<PlayersContainer>(json, options);

                if (container?.Players != null)
                {
                    lock (_lock)
                    {
                        _players.Clear();
                        _players.AddRange(container.Players);
                    }
                    _logger.LogInformation("Successfully loaded {Count} players from JSON data.", _players.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load players from json file.");
            }
        }

        public Task<IEnumerable<Player>> GetAllAsync()
        {
            lock (_lock)
            {
                // Return a copy of the list to avoid collection modification exceptions
                return Task.FromResult<IEnumerable<Player>>(_players.ToList());
            }
        }

        public Task<Player?> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                var player = _players.FirstOrDefault(p => p.Id == id);
                return Task.FromResult(player);
            }
        }

        public Task<Player> AddAsync(Player player)
        {
            lock (_lock)
            {
                var maxId = _players.Count > 0 ? _players.Max(p => p.Id) : 0;
                player.Id = maxId + 1;
                _players.Add(player);
                _logger.LogInformation("Added new player: {Firstname} {Lastname} with ID {Id}", player.Firstname, player.Lastname, player.Id);
                return Task.FromResult(player);
            }
        }

        private class PlayersContainer
        {
            public List<Player> Players { get; set; } = new();
        }
    }
}
