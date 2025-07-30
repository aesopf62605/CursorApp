using System;
using System.Threading.Tasks;
using InvoiceApi.Models;
using InvoiceApi.Services;
using InvoiceApi.Clients;
using Moq;
using Xunit;

namespace InvoiceApi.Tests
{
    public class InvoiceCalculatorServiceTests
    {
        private InvoiceCalculatorService CreateService(decimal exchangeRate)
        {
            var mockClient = new Mock<IExchangeRateClient>();
            mockClient.Setup(x => x.GetHistoricalRateAsync(It.IsAny<DateTime>(), "EUR", "USD"))
                .ReturnsAsync(1.187247m);
            mockClient.Setup(x => x.GetHistoricalRateAsync(It.IsAny<DateTime>(), "EUR", "CAD"))
                .ReturnsAsync(1.564839m);
            // EUR to EUR always 1
            mockClient.Setup(x => x.GetHistoricalRateAsync(It.IsAny<DateTime>(), "EUR", "EUR"))
                .ReturnsAsync(1m);
            return new InvoiceCalculatorService(mockClient.Object);
        }

        [Fact]
        public async Task Calculate_USD_TestCase1()
        {
            var service = CreateService(1.187247m);
            var request = new InvoiceCalculationRequest
            {
                InvoiceDate = new DateTime(2020, 8, 5),
                PreTaxAmount = 123.45m,
                PaymentCurrency = "USD"
            };
            var result = await service.CalculateAsync(request);
            Assert.Equal(146.57m, result.PreTaxTotal);
            Assert.Equal(14.66m, result.TaxAmount);
            Assert.Equal(161.23m, result.GrandTotal);
            Assert.Equal(1.187247m, result.ExchangeRate);
            Assert.Equal("USD", result.Currency);
        }

        [Fact]
        public async Task Calculate_EUR_TestCase2()
        {
            var service = CreateService(1m);
            var request = new InvoiceCalculationRequest
            {
                InvoiceDate = new DateTime(2019, 7, 12),
                PreTaxAmount = 1000.00m,
                PaymentCurrency = "EUR"
            };
            var result = await service.CalculateAsync(request);
            Assert.Equal(1000.00m, result.PreTaxTotal);
            Assert.Equal(90.00m, result.TaxAmount);
            Assert.Equal(1090.00m, result.GrandTotal);
            Assert.Equal(1m, result.ExchangeRate);
            Assert.Equal("EUR", result.Currency);
        }

        [Fact]
        public async Task Calculate_CAD_TestCase3()
        {
            var service = CreateService(1.564839m);
            var request = new InvoiceCalculationRequest
            {
                InvoiceDate = new DateTime(2020, 8, 19),
                PreTaxAmount = 6543.21m,
                PaymentCurrency = "CAD"
            };
            var result = await service.CalculateAsync(request);
            Assert.Equal(10239.07m, result.PreTaxTotal);
            Assert.Equal(1126.30m, result.TaxAmount);
            Assert.Equal(11365.37m, result.GrandTotal);
            Assert.Equal(1.564839m, result.ExchangeRate);
            Assert.Equal("CAD", result.Currency);
        }

        [Fact]
        public async Task Calculate_Throws_On_NegativeAmount()
        {
            var service = CreateService(1m);
            var request = new InvoiceCalculationRequest
            {
                InvoiceDate = DateTime.Now,
                PreTaxAmount = -1m,
                PaymentCurrency = "USD"
            };
            await Assert.ThrowsAsync<ArgumentException>(() => service.CalculateAsync(request));
        }

        [Fact]
        public async Task Calculate_Throws_On_UnsupportedCurrency()
        {
            var service = CreateService(1m);
            var request = new InvoiceCalculationRequest
            {
                InvoiceDate = DateTime.Now,
                PreTaxAmount = 100m,
                PaymentCurrency = "GBP"
            };
            await Assert.ThrowsAsync<ArgumentException>(() => service.CalculateAsync(request));
        }
    }
} 