using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TennisStats.Api.DTOs;
using Xunit;

namespace TennisStats.Tests.IntegrationTests
{
    public class PlayersApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public PlayersApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetStats_ShouldReturnSuccessAndCorrectJsonStructure()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/players/stats");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var stats = await response.Content.ReadFromJsonAsync<StatsResponseDto>();
            Assert.NotNull(stats);
            Assert.NotNull(stats.CountryWithBestWinRatio);
            Assert.NotEmpty(stats.CountryWithBestWinRatio.CountryCode);
            Assert.True(stats.AverageBMI > 0);
            Assert.True(stats.MedianHeight > 0);
        }

        [Fact]
        public async Task GetPlayers_ShouldReturnSortedList()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/players");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var players = await response.Content.ReadFromJsonAsync<List<PlayerResponseDto>>();
            Assert.NotNull(players);
            Assert.True(players.Count >= 5); // Seed dataset contains 5 players
            
            // Check that they are sorted by points descending
            for (int i = 0; i < players.Count - 1; i++)
            {
                Assert.True(players[i].Data.Points >= players[i + 1].Data.Points, 
                    $"Player {players[i].Firstname} ({players[i].Data.Points} pts) is before " +
                    $"{players[i+1].Firstname} ({players[i+1].Data.Points} pts) but points are not sorted descending.");
            }
        }

        [Fact]
        public async Task GetPlayerById_WithInvalidId_ShouldReturn404()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/players/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            
            var error = await response.Content.ReadFromJsonAsync<ErrorResponseDto>();
            Assert.NotNull(error);
            Assert.Equal(404, error.StatusCode);
            Assert.Equal("Not Found", error.Error);
            Assert.Contains("99999 was not found", error.Message);
        }


    }
}
