using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TennisStats.Api.DTOs;
using TennisStats.Api.Services;

namespace TennisStats.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(IPlayerService playerService, ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all tennis players, sorted by their points in descending order.
        /// </summary>
        /// <param name="sex">Optional filter by sex ('M' or 'F').</param>
        /// <returns>A list of players sorted by points descending.</returns>
        /// <response code="200">The list of players was successfully retrieved.</response>
        /// <response code="400">The 'sex' query parameter is invalid.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlayerResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponseDto))]
        public async Task<ActionResult<IEnumerable<PlayerResponseDto>>> GetAll([FromQuery] string? sex)
        {
            _logger.LogInformation("Getting all players. Filter sex: {Sex}", sex);
            var players = await _playerService.GetPlayersAsync(sex);
            return Ok(players);
        }

        /// <summary>
        /// Retrieves the global tennis statistics (best win ratio country, average BMI, median height).
        /// </summary>
        /// <returns>Tennis statistics response.</returns>
        /// <response code="200">The statistics were successfully computed and returned.</response>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatsResponseDto))]
        public async Task<ActionResult<StatsResponseDto>> GetStats()
        {
            _logger.LogInformation("Retrieving tennis players statistics.");
            var stats = await _playerService.GetStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Retrieves a specific player by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the player.</param>
        /// <returns>The requested player details.</returns>
        /// <response code="200">The player was found and returned.</response>
        /// <response code="400">The provided ID was not a valid integer.</response>
        /// <response code="404">No player was found with the specified ID.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponseDto))]
        public async Task<ActionResult<PlayerResponseDto>> GetById(int id)
        {
            _logger.LogInformation("Getting player with ID {Id}", id);
            var player = await _playerService.GetPlayerByIdAsync(id);
            return Ok(player);
        }

        /// <summary>
        /// Retrieves the individual win ratio of a player on their last matches.
        /// </summary>
        /// <param name="id">The unique identifier of the player.</param>
        /// <returns>The individual win ratio.</returns>
        /// <response code="200">The win ratio was successfully computed.</response>
        /// <response code="400">The provided ID was not a valid integer.</response>
        /// <response code="404">No player was found with the specified ID.</response>
        [HttpGet("{id}/win-ratio")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponseDto))]
        public async Task<ActionResult> GetWinRatio(int id)
        {
            _logger.LogInformation("Getting win ratio for player with ID {Id}", id);
            var winRatio = await _playerService.GetPlayerWinRatioAsync(id);
            return Ok(new { id, winRatio });
        }

        /// <summary>
        /// Adds a new tennis player.
        /// </summary>
        /// <param name="request">The details of the player to create.</param>
        /// <returns>The created player details.</returns>
        /// <response code="201">The player was successfully created.</response>
        /// <response code="400">The request body validation failed.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PlayerResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponseDto))]
        public async Task<ActionResult<PlayerResponseDto>> Create([FromBody] CreatePlayerRequestDto request)
        {
            _logger.LogInformation("Creating a new player: {Firstname} {Lastname}", request.Firstname, request.Lastname);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdPlayer = await _playerService.CreatePlayerAsync(request);
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = createdPlayer.Id },
                createdPlayer
            );
        }

        /// <summary>
        /// Deletes a tennis player by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the player to delete.</param>
        /// <returns>No content on success.</returns>
        /// <response code="204">The player was successfully deleted.</response>
        /// <response code="400">The provided ID was not a valid integer.</response>
        /// <response code="404">No player was found with the specified ID.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponseDto))]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting player with ID {Id}", id);
            await _playerService.DeletePlayerAsync(id);
            return NoContent();
        }
    }
}
