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
        private readonly string _baseUrl = "https://api.apilayer.com/fixer/";

        public FixerExchangeRateClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["FixerApiKey"] ?? throw new ArgumentNullException("FixerApiKey");
        }

        public async Task<decimal> GetHistoricalRateAsync(DateTime date, string baseCurrency, string targetCurrency)
        {
            var dateString = date.ToString("yyyy-MM-dd");
            var url = $"{_baseUrl}{dateString}?base={baseCurrency}&symbols={targetCurrency}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to fetch exchange rate: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("success", out var success) || !success.GetBoolean())
                throw new Exception("Fixer API did not return success.");
            var rate = doc.RootElement.GetProperty("rates").GetProperty(targetCurrency).GetDecimal();
            return rate;
        }
    }
} 