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
    public class StatsController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly ILogger<StatsController> _logger;

        public StatsController(IPlayerService playerService, ILogger<StatsController> logger)
        {
            _playerService = playerService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the global tennis statistics (best win ratio country, average BMI, median height).
        /// This is an alias endpoint for GET /api/players/stats.
        /// </summary>
        /// <returns>Tennis statistics response.</returns>
        /// <response code="200">The statistics were successfully computed and returned.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatsResponseDto))]
        public async Task<ActionResult<StatsResponseDto>> GetStats()
        {
            _logger.LogInformation("Retrieving tennis statistics from StatsController.");
            var stats = await _playerService.GetStatsAsync();
            return Ok(stats);
        }
    }
}
