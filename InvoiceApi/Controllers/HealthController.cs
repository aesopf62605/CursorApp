using Microsoft.AspNetCore.Mvc;

namespace InvoiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("metrics")]
        public IActionResult GetMetrics()
        {
            return Ok(new { message = "Metrics endpoint is available at /metrics" });
        }
    }
} 