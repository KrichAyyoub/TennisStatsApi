using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TennisStats.Api.DTOs;
using TennisStats.Api.Exceptions;
using TennisStats.Api.Models;
using TennisStats.Api.Repositories;
using TennisStats.Api.Services;
using Xunit;

namespace TennisStats.Tests.Services
{
    public class PlayerServiceTests
    {
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly PlayerService _playerService;

        public PlayerServiceTests()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            _playerService = new PlayerService(_mockRepository.Object);
        }

        private static List<Player> GetMockPlayers()
        {
            return new List<Player>
            {
                new()
                {
                    Id = 1,
                    Firstname = "Novak",
                    Lastname = "Djokovic",
                    Sex = "M",
                    Country = new CountryInfo { Code = "SRB", Picture = "http://srb.png" },
                    Data = new PlayerData { Rank = 2, Points = 2500, Weight = 80000, Height = 188, Age = 31, Last = new List<int> { 1, 1, 1 } }
                },
                new()
                {
                    Id = 2,
                    Firstname = "Venus",
                    Lastname = "Williams",
                    Sex = "F",
                    Country = new CountryInfo { Code = "USA", Picture = "http://usa.png" },
                    Data = new PlayerData { Rank = 52, Points = 1100, Weight = 74000, Height = 185, Age = 38, Last = new List<int> { 0, 1, 0 } }
                },
                new()
                {
                    Id = 3,
                    Firstname = "Stan",
                    Lastname = "Wawrinka",
                    Sex = "M",
                    Country = new CountryInfo { Code = "SUI", Picture = "http://sui.png" },
                    Data = new PlayerData { Rank = 21, Points = 1700, Weight = 81000, Height = 183, Age = 33, Last = new List<int> { 1, 1, 0 } }
                },
                new()
                {
                    Id = 4,
                    Firstname = "Serena",
                    Lastname = "Williams",
                    Sex = "F",
                    Country = new CountryInfo { Code = "USA", Picture = "http://usa.png" },
                    Data = new PlayerData { Rank = 10, Points = 3500, Weight = 72000, Height = 175, Age = 37, Last = new List<int> { 1, 1, 1 } }
                },
                new()
                {
                    Id = 5,
                    Firstname = "Rafael",
                    Lastname = "Nadal",
                    Sex = "M",
                    Country = new CountryInfo { Code = "ESP", Picture = "http://esp.png" },
                    Data = new PlayerData { Rank = 1, Points = 2000, Weight = 85000, Height = 185, Age = 33, Last = new List<int> { 1, 0, 0 } }
                }
            };
        }

        [Fact]
        public async Task GetPlayersAsync_ShouldReturnPlayersSortedByPointsDescending()
        {
            // Arrange
            var mockPlayers = GetMockPlayers();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(mockPlayers);

            // Act
            var result = (await _playerService.GetPlayersAsync()).ToList();

            // Assert
            Assert.Equal(5, result.Count);
            Assert.Equal("Serena", result[0].Firstname); // 3500 pts
            Assert.Equal("Novak", result[1].Firstname);  // 2500 pts
            Assert.Equal("Rafael", result[2].Firstname); // 2000 pts
            Assert.Equal("Stan", result[3].Firstname);   // 1700 pts
            Assert.Equal("Venus", result[4].Firstname);  // 1100 pts
        }

        [Fact]
        public async Task GetPlayersAsync_WithSexFilter_ShouldReturnFilteredPlayers()
        {
            // Arrange
            var mockPlayers = GetMockPlayers();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(mockPlayers);

            // Act
            var result = (await _playerService.GetPlayersAsync("F")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("F", p.Sex));
        }

        [Fact]
        public async Task GetPlayersAsync_WithInvalidSexFilter_ShouldThrowValidationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _playerService.GetPlayersAsync("Invalid"));
        }

        [Fact]
        public async Task GetPlayerByIdAsync_WithValidId_ShouldReturnPlayer()
        {
            // Arrange
            var mockPlayers = GetMockPlayers();
            var target = mockPlayers[0];
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(target);

            // Act
            var result = await _playerService.GetPlayerByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Novak", result.Firstname);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetPlayerByIdAsync_WithInvalidId_ShouldThrowPlayerNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Player?)null);

            // Act & Assert
            await Assert.ThrowsAsync<PlayerNotFoundException>(() => _playerService.GetPlayerByIdAsync(99));
        }

        [Fact]
        public async Task CreatePlayerAsync_ShouldAddAndReturnCreatedPlayer()
        {
            // Arrange
            var request = new CreatePlayerRequestDto
            {
                Firstname = "Roger",
                Lastname = "Federer",
                Shortname = "R.FED",
                Sex = "M",
                CountryCode = "SUI",
                CountryPicture = "http://sui.png",
                Picture = "http://fed.png",
                Rank = 5,
                Points = 2000,
                Weight = 85000,
                Height = 185,
                Age = 41,
                Last = new List<int> { 1, 1, 1 }
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Player>()))
                .ReturnsAsync((Player p) =>
                {
                    p.Id = 6;
                    return p;
                });

            // Act
            var result = await _playerService.CreatePlayerAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6, result.Id);
            Assert.Equal("Roger", result.Firstname);
            Assert.Equal("SUI", result.Country.Code);
            Assert.Equal(2000, result.Data.Points);
            _mockRepository.Verify(r => r.AddAsync(It.Is<Player>(pl => pl.Firstname == "Roger")), Times.Once);
        }

        [Fact]
        public async Task CreatePlayerAsync_WithNullRequest_ShouldThrowValidationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _playerService.CreatePlayerAsync(null!));
        }

        [Fact]
        public async Task GetStatsAsync_ShouldCalculateAverageBMICorrectly()
        {
            // Arrange
            // We use the 5 mock players. Let's calculate their BMIs:
            // 1. Novak: weight = 80kg, height = 1.88m -> BMI = 80 / (1.88^2) = 22.634676
            // 2. Venus: weight = 74kg, height = 1.85m -> BMI = 74 / (1.85^2) = 21.621621
            // 3. Stan: weight = 81kg, height = 1.83m -> BMI = 81 / (1.83^2) = 24.187046
            // 4. Serena: weight = 72kg, height = 1.75m -> BMI = 72 / (1.75^2) = 23.510204
            // 5. Rafael: weight = 85kg, height = 1.85m -> BMI = 85 / (1.85^2) = 24.835646
            // Average BMI = (22.634676 + 21.621621 + 24.187046 + 23.510204 + 24.835646) / 5 = 23.3578386...
            // Rounded to 2 decimals = 23.36
            var mockPlayers = GetMockPlayers();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(mockPlayers);

            // Act
            var result = await _playerService.GetStatsAsync();

            // Assert
            Assert.Equal(23.36, result.AverageBMI);
        }

        [Fact]
        public async Task GetStatsAsync_WithOddNumberOfPlayers_ShouldCalculateMedianHeightCorrectly()
        {
            // Arrange
            // Heights: 188, 185, 183, 175, 185
            // Sorted: 175, 183, 185, 185, 188
            // Median for 5 items is index 2 -> 185
            var mockPlayers = GetMockPlayers();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(mockPlayers);

            // Act
            var result = await _playerService.GetStatsAsync();

            // Assert
            Assert.Equal(185.0, result.MedianHeight);
        }

        [Fact]
        public async Task GetStatsAsync_WithEvenNumberOfPlayers_ShouldCalculateMedianHeightCorrectly()
        {
            // Arrange
            // Heights: 188, 185, 183, 175
            // Sorted: 175, 183, 185, 188
            // Median for 4 items is average of index 1 (183) and index 2 (185) -> (183 + 185) / 2 = 184
            var mockPlayers = GetMockPlayers().Take(4).ToList();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(mockPlayers);

            // Act
            var result = await _playerService.GetStatsAsync();

            // Assert
            Assert.Equal(184.0, result.MedianHeight);
        }

        [Fact]
        public async Task GetStatsAsync_ShouldCalculateBestWinRatioCountryCorrectly()
        {
            // Arrange
            // Win ratio by country code:
            // SRB (Novak): 1,1,1 -> 3 wins / 3 matches = 1.0 (100%)
            // USA (Venus, Serena):
            //   Venus: 0,1,0 -> 1 win / 3 matches
            //   Serena: 1,1,1 -> 3 wins / 3 matches
            //   Aggregated: 4 wins / 6 matches = 0.67 (67%)
            // SUI (Stan): 1,1,0 -> 2 wins / 3 matches = 0.67 (67%)
            // ESP (Rafael): 1,0,0 -> 1 win / 3 matches = 0.33 (33%)
            // Best country should be SRB with ratio 1.0
            var mockPlayers = GetMockPlayers();
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(mockPlayers);

            // Act
            var result = await _playerService.GetStatsAsync();

            // Assert
            Assert.NotNull(result.CountryWithBestWinRatio);
            Assert.Equal("SRB", result.CountryWithBestWinRatio.CountryCode);
            Assert.Equal(1.0, result.CountryWithBestWinRatio.WinRatio);
        }

        [Fact]
        public async Task GetStatsAsync_WithRatioTies_ShouldBreakTieAlphabetically()
        {
            // Arrange
            // We set up SUI and USA with the same ratio (e.g. 100% win ratio)
            // SUI should win the tie-breaker alphabetically (SUI before USA)
            var mockPlayers = new List<Player>
            {
                new()
                {
                    Id = 1,
                    Country = new CountryInfo { Code = "USA" },
                    Data = new PlayerData { Weight = 70000, Height = 180, Last = new List<int> { 1 } }
                },
                new()
                {
                    Id = 2,
                    Country = new CountryInfo { Code = "SUI" },
                    Data = new PlayerData { Weight = 75000, Height = 182, Last = new List<int> { 1 } }
                }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(mockPlayers);

            // Act
            var result = await _playerService.GetStatsAsync();

            // Assert
            Assert.Equal("SUI", result.CountryWithBestWinRatio.CountryCode);
            Assert.Equal(1.0, result.CountryWithBestWinRatio.WinRatio);
        }

        [Fact]
        public async Task DeletePlayerAsync_WithValidId_ShouldCallRepositoryDelete()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _playerService.DeletePlayerAsync(1);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeletePlayerAsync_WithInvalidId_ShouldThrowPlayerNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<PlayerNotFoundException>(() => _playerService.DeletePlayerAsync(99));
        }

        [Fact]
        public async Task GetPlayerWinRatioAsync_WithValidId_ShouldCalculateIndividualRatioCorrectly()
        {
            // Arrange
            var mockPlayers = GetMockPlayers();
            var target = mockPlayers[2]; // Stan (SUI): 1,1,0 -> 2 wins / 3 matches = 0.67
            _mockRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(target);

            // Act
            var result = await _playerService.GetPlayerWinRatioAsync(3);

            // Assert
            Assert.Equal(0.67, result);
        }
    }
}
