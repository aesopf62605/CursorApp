using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InvoiceApi.Models;
using InvoiceApi.Services;

namespace InvoiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceCalculatorService _calculatorService;

        public InvoiceController(InvoiceCalculatorService calculatorService)
        {
            _calculatorService = calculatorService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] InvoiceCalculationRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");
            try
            {
                var result = await _calculatorService.CalculateAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 