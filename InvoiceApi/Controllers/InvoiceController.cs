using System;
using System.Diagnostics;
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
        private readonly IMetricsService _metricsService;

        public InvoiceController(InvoiceCalculatorService calculatorService, IMetricsService metricsService)
        {
            _calculatorService = calculatorService;
            _metricsService = metricsService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] InvoiceCalculationRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var currency = request?.PaymentCurrency ?? "unknown";
            
            try
            {
                _metricsService.IncrementActiveCalculations(currency);
                
                if (request == null)
                {
                    _metricsService.RecordApiRequest("calculate", 400, stopwatch.Elapsed);
                    return BadRequest("Request body is required.");
                }

                var result = await _calculatorService.CalculateAsync(request);
                
                stopwatch.Stop();
                _metricsService.RecordInvoiceCalculation(currency, (double)request.PreTaxAmount, stopwatch.Elapsed, true);
                _metricsService.RecordApiRequest("calculate", 200, stopwatch.Elapsed);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                stopwatch.Stop();
                _metricsService.RecordInvoiceCalculation(currency, request?.PreTaxAmount != null ? (double)request.PreTaxAmount : 0, stopwatch.Elapsed, false);
                _metricsService.RecordApiRequest("calculate", 400, stopwatch.Elapsed);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _metricsService.RecordInvoiceCalculation(currency, request?.PreTaxAmount != null ? (double)request.PreTaxAmount : 0, stopwatch.Elapsed, false);
                _metricsService.RecordApiRequest("calculate", 500, stopwatch.Elapsed);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                _metricsService.DecrementActiveCalculations(currency);
            }
        }
    }
} 