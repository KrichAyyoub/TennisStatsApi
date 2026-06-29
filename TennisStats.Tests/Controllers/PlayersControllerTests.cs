using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TennisStats.Api.Controllers;
using TennisStats.Api.DTOs;
using TennisStats.Api.Exceptions;
using TennisStats.Api.Services;
using Xunit;

namespace TennisStats.Tests.Controllers
{
    public class PlayersControllerTests
    {
        private readonly Mock<IPlayerService> _mockService;
        private readonly Mock<ILogger<PlayersController>> _mockLogger;
        private readonly PlayersController _controller;

        public PlayersControllerTests()
        {
            _mockService = new Mock<IPlayerService>();
            _mockLogger = new Mock<ILogger<PlayersController>>();
            _controller = new PlayersController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWithPlayers()
        {
            // Arrange
            var expectedPlayers = new List<PlayerResponseDto>
            {
                new() { Id = 1, Firstname = "Novak" },
                new() { Id = 2, Firstname = "Rafael" }
            };
            _mockService.Setup(s => s.GetPlayersAsync(null)).ReturnsAsync(expectedPlayers);

            // Act
            var result = await _controller.GetAll(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var players = Assert.IsAssignableFrom<IEnumerable<PlayerResponseDto>>(okResult.Value);
            Assert.Equal(expectedPlayers, players);
        }

        [Fact]
        public async Task GetById_WhenFound_ShouldReturnOkWithPlayer()
        {
            // Arrange
            var expectedPlayer = new PlayerResponseDto { Id = 1, Firstname = "Novak" };
            _mockService.Setup(s => s.GetPlayerByIdAsync(1)).ReturnsAsync(expectedPlayer);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var player = Assert.IsType<PlayerResponseDto>(okResult.Value);
            Assert.Equal(expectedPlayer, player);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ShouldPropagateException()
        {
            // Arrange
            _mockService.Setup(s => s.GetPlayerByIdAsync(99)).ThrowsAsync(new PlayerNotFoundException(99));

            // Act & Assert
            // The controllers do not contain try-catch blocks; exceptions bubble up to the global middleware.
            await Assert.ThrowsAsync<PlayerNotFoundException>(() => _controller.GetById(99));
        }

        [Fact]
        public async Task Create_WithValidModel_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var request = new CreatePlayerRequestDto
            {
                Firstname = "Roger",
                Lastname = "Federer",
                Shortname = "R.FED",
                Sex = "M",
                CountryCode = "SUI",
                Rank = 5,
                Points = 2000,
                Weight = 85000,
                Height = 185,
                Age = 41
            };

            var createdDto = new PlayerResponseDto
            {
                Id = 6,
                Firstname = "Roger",
                Lastname = "Federer"
            };

            _mockService.Setup(s => s.CreatePlayerAsync(request)).ReturnsAsync(createdDto);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PlayersController.GetById), createdResult.ActionName);
            Assert.Equal(6, createdResult.RouteValues?["id"]);
            
            var returnedValue = Assert.IsType<PlayerResponseDto>(createdResult.Value);
            Assert.Equal(createdDto, returnedValue);
        }

        [Fact]
        public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreatePlayerRequestDto();
            _controller.ModelState.AddModelError("Firstname", "First name is required.");

            // Act
            var result = await _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent()
        {
            // Arrange
            _mockService.Setup(s => s.DeletePlayerAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.DeletePlayerAsync(1), Times.Once);
        }
    }
}
