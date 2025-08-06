using Prometheus;
using System.Diagnostics;

namespace InvoiceApi.Services
{
    public interface IMetricsService
    {
        void RecordInvoiceCalculation(string currency, double amount, TimeSpan duration, bool success);
        void RecordExchangeRateRequest(string fromCurrency, string toCurrency, bool success);
        void RecordExchangeRateRequest(string fromCurrency, string toCurrency, bool success, TimeSpan duration);
        void RecordApiRequest(string endpoint, int statusCode, TimeSpan duration);
        void IncrementActiveCalculations(string currency);
        void DecrementActiveCalculations(string currency);
    }

    public class MetricsService : IMetricsService
    {
        private readonly Counter _invoiceCalculationsTotal;
        private readonly Histogram _invoiceCalculationDuration;
        private readonly Counter _exchangeRateRequestsTotal;
        private readonly Histogram _exchangeRateRequestDuration;
        private readonly Counter _apiRequestsTotal;
        private readonly Histogram _apiRequestDuration;
        private readonly Gauge _activeCalculations;

        public MetricsService()
        {
            // Invoice calculation metrics
            _invoiceCalculationsTotal = Metrics.CreateCounter(
                "invoice_calculations_total",
                "Total number of invoice calculations",
                new CounterConfiguration
                {
                    LabelNames = new[] { "currency", "success" }
                });

            _invoiceCalculationDuration = Metrics.CreateHistogram(
                "invoice_calculation_duration_seconds",
                "Duration of invoice calculations",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "currency" },
                    Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
                });

            // Exchange rate request metrics
            _exchangeRateRequestsTotal = Metrics.CreateCounter(
                "exchange_rate_requests_total",
                "Total number of exchange rate requests",
                new CounterConfiguration
                {
                    LabelNames = new[] { "from_currency", "to_currency", "success" }
                });

            _exchangeRateRequestDuration = Metrics.CreateHistogram(
                "exchange_rate_request_duration_seconds",
                "Duration of exchange rate requests",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "from_currency", "to_currency" },
                    Buckets = Histogram.ExponentialBuckets(0.01, 2, 10)
                });

            // API request metrics
            _apiRequestsTotal = Metrics.CreateCounter(
                "api_requests_total",
                "Total number of API requests",
                new CounterConfiguration
                {
                    LabelNames = new[] { "endpoint", "status_code" }
                });

            _apiRequestDuration = Metrics.CreateHistogram(
                "api_request_duration_seconds",
                "Duration of API requests",
                new HistogramConfiguration
                {
                    LabelNames = new[] { "endpoint" },
                    Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
                });

            // Active calculations gauge
            _activeCalculations = Metrics.CreateGauge(
                "active_calculations",
                "Number of active invoice calculations",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "currency" }
                });
        }

        public void RecordInvoiceCalculation(string currency, double amount, TimeSpan duration, bool success)
        {
            _invoiceCalculationsTotal
                .WithLabels(currency, success.ToString().ToLower())
                .Inc();

            _invoiceCalculationDuration
                .WithLabels(currency)
                .Observe(duration.TotalSeconds);
        }

        public void RecordExchangeRateRequest(string fromCurrency, string toCurrency, bool success)
        {
            _exchangeRateRequestsTotal
                .WithLabels(fromCurrency, toCurrency, success.ToString().ToLower())
                .Inc();
        }

        public void RecordExchangeRateRequest(string fromCurrency, string toCurrency, bool success, TimeSpan duration)
        {
            RecordExchangeRateRequest(fromCurrency, toCurrency, success);
            
            _exchangeRateRequestDuration
                .WithLabels(fromCurrency, toCurrency)
                .Observe(duration.TotalSeconds);
        }

        public void RecordApiRequest(string endpoint, int statusCode, TimeSpan duration)
        {
            _apiRequestsTotal
                .WithLabels(endpoint, statusCode.ToString())
                .Inc();

            _apiRequestDuration
                .WithLabels(endpoint)
                .Observe(duration.TotalSeconds);
        }

        public void IncrementActiveCalculations(string currency)
        {
            _activeCalculations
                .WithLabels(currency)
                .Inc();
        }

        public void DecrementActiveCalculations(string currency)
        {
            _activeCalculations
                .WithLabels(currency)
                .Dec();
        }
    }
} 