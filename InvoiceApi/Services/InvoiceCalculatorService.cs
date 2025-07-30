using System;
using System.Globalization;
using System.Threading.Tasks;
using InvoiceApi.Models;
using InvoiceApi.Clients;

namespace InvoiceApi.Services
{
    public class InvoiceCalculatorService
    {
        private readonly IExchangeRateClient _exchangeRateClient;

        public InvoiceCalculatorService(IExchangeRateClient exchangeRateClient)
        {
            _exchangeRateClient = exchangeRateClient;
        }

        public async Task<InvoiceCalculationResponse> CalculateAsync(InvoiceCalculationRequest request)
        {
            if (request.PreTaxAmount < 0)
                throw new ArgumentException("Pre-tax amount must be non-negative.");
            if (string.IsNullOrWhiteSpace(request.PaymentCurrency))
                throw new ArgumentException("Payment currency is required.");

            var currency = request.PaymentCurrency.ToUpperInvariant();
            decimal taxRate = currency switch
            {
                "CAD" => 0.11m,
                "USD" => 0.10m,
                "EUR" => 0.09m,
                _ => throw new ArgumentException($"Unsupported currency: {currency}")
            };

            decimal exchangeRate = 1m;
            if (currency != "EUR")
            {
                exchangeRate = await _exchangeRateClient.GetHistoricalRateAsync(request.InvoiceDate, "EUR", currency);
            }

            decimal preTaxTotal = Math.Round(request.PreTaxAmount * exchangeRate, 2, MidpointRounding.AwayFromZero);
            decimal taxAmount = Math.Round(preTaxTotal * taxRate, 2, MidpointRounding.AwayFromZero);
            decimal grandTotal = Math.Round(preTaxTotal + taxAmount, 2, MidpointRounding.AwayFromZero);

            return new InvoiceCalculationResponse
            {
                PreTaxTotal = preTaxTotal,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                ExchangeRate = exchangeRate,
                Currency = currency
            };
        }
    }
} 