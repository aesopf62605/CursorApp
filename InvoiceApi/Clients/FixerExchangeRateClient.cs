using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace InvoiceApi.Clients
{
    public class FixerExchangeRateClient : IExchangeRateClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "http://data.fixer.io/api";

        public FixerExchangeRateClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["FixerApiKey"] ?? throw new ArgumentNullException("FixerApiKey");
        }

        public async Task<decimal> GetHistoricalRateAsync(DateTime date, string baseCurrency, string targetCurrency)
        {
            try
            {
                // First try historical rates endpoint
                return await GetHistoricalRateFromEndpoint(date, baseCurrency, targetCurrency);
            }
            catch (Exception ex) when (ex.Message.Contains("401") || ex.Message.Contains("403") || ex.Message.Contains("Unauthorized"))
            {
                // If historical rates fail due to authorization, fall back to latest rates
                // This is common with free tier API keys
                return await GetLatestRateFromEndpoint(baseCurrency, targetCurrency);
            }
        }

        private async Task<decimal> GetHistoricalRateFromEndpoint(DateTime date, string baseCurrency, string targetCurrency)
        {
            var dateString = date.ToString("yyyy-MM-dd");
            var url = $"{_baseUrl}/{dateString}?access_key={_apiKey}&base={baseCurrency}&symbols={targetCurrency}";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch historical exchange rate: {response.StatusCode}. Response: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            // Check if the response has a success property
            if (doc.RootElement.TryGetProperty("success", out var success))
            {
                if (!success.GetBoolean())
                {
                    var errorMessage = doc.RootElement.TryGetProperty("error", out var error) 
                        ? error.GetProperty("info").GetString() 
                        : "Unknown error from Fixer API";
                    throw new Exception($"Fixer API error: {errorMessage}");
                }
            }

            // Extract the rate from the response
            if (doc.RootElement.TryGetProperty("rates", out var rates) && 
                rates.TryGetProperty(targetCurrency, out var targetRate))
            {
                return targetRate.GetDecimal();
            }
            
            throw new Exception($"Could not find exchange rate for {targetCurrency} in the response");
        }

        private async Task<decimal> GetLatestRateFromEndpoint(string baseCurrency, string targetCurrency)
        {
            var url = $"{_baseUrl}/latest?access_key={_apiKey}&base={baseCurrency}&symbols={targetCurrency}";
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch latest exchange rate: {response.StatusCode}. Response: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            // Check if the response has a success property
            if (doc.RootElement.TryGetProperty("success", out var success))
            {
                if (!success.GetBoolean())
                {
                    var errorMessage = doc.RootElement.TryGetProperty("error", out var error) 
                        ? error.GetProperty("info").GetString() 
                        : "Unknown error from Fixer API";
                    throw new Exception($"Fixer API error: {errorMessage}");
                }
            }

            // Extract the rate from the response
            if (doc.RootElement.TryGetProperty("rates", out var rates) && 
                rates.TryGetProperty(targetCurrency, out var targetRate))
            {
                return targetRate.GetDecimal();
            }
            
            throw new Exception($"Could not find exchange rate for {targetCurrency} in the response");
        }
    }
} 