using System;
using System.Threading.Tasks;

namespace InvoiceApi.Clients
{
    public interface IExchangeRateClient
    {
        Task<decimal> GetHistoricalRateAsync(DateTime date, string baseCurrency, string targetCurrency);
    }
} 