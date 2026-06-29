using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TennisStats.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Retrieves the API health status.
        /// </summary>
        /// <returns>A status JSON indicating the service is healthy.</returns>
        /// <response code="200">The service is healthy.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetHealth()
        {
            return Ok(new { status = "ok" });
        }
    }
}
