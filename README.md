# Invoice Calculation API

A .NET 8 Web API solution that calculates invoice totals with currency conversion and tax calculations using historical exchange rates.

## 📋 Overview

This solution provides a REST API for calculating invoice totals with the following features:

- **Multi-currency support**: EUR, USD, and CAD
- **Historical exchange rates**: Uses the Fixer API to get historical exchange rates
- **Tax calculations**: Different tax rates for each supported currency
- **Comprehensive testing**: Unit tests with mocked exchange rate client
- **Swagger documentation**: Auto-generated API documentation

## 🏗️ Architecture

### Solution Structure

```
InvoiceApi/
├── InvoiceApi/                    # Main API project
│   ├── Controllers/              # API endpoints
│   ├── Models/                   # Request/Response DTOs
│   ├── Services/                 # Business logic
│   ├── Clients/                  # External API clients
│   └── Program.cs               # Application startup
├── InvoiceApi.Tests/             # Unit tests
└── InvoiceApi.sln               # Solution file
```

### Key Components

#### 1. **InvoiceController** (`Controllers/InvoiceController.cs`)
- REST API endpoint for invoice calculations
- Handles HTTP POST requests to `/api/invoice/calculate`
- Provides proper error handling and HTTP status codes

#### 2. **InvoiceCalculatorService** (`Services/InvoiceCalculatorService.cs`)
- Core business logic for invoice calculations
- Handles currency conversion and tax calculations
- Supports EUR, USD, and CAD currencies

#### 3. **Exchange Rate Client** (`Clients/`)
- `IExchangeRateClient`: Interface for exchange rate operations
- `FixerExchangeRateClient`: Implementation using Fixer API
- Fetches historical exchange rates for currency conversion

#### 4. **Models** (`Models/`)
- `InvoiceCalculationRequest`: Input model with invoice date, amount, and currency
- `InvoiceCalculationResponse`: Output model with calculated totals and rates

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Fixer API key (for exchange rate data)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd InvoiceApi
   ```

2. **Configure API Key**
   Add your Fixer API key to `appsettings.json`:
   ```json
   {
     "FixerApiKey": "your-api-key-here"
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run --project InvoiceApi
   ```

5. **Access the API**
   - API: `https://localhost:7001`
   - Swagger UI: `https://localhost:7001/swagger`

## 📖 API Usage

### Calculate Invoice

**Endpoint:** `POST /api/invoice/calculate`

**Request Body:**
```json
{
  "invoiceDate": "2020-08-05",
  "preTaxAmount": 123.45,
  "paymentCurrency": "USD"
}
```

**Response:**
```json
{
  "preTaxTotal": 146.57,
  "taxAmount": 14.66,
  "grandTotal": 161.23,
  "exchangeRate": 1.187247,
  "currency": "USD"
}
```

### Supported Currencies

| Currency | Code | Tax Rate |
|----------|------|----------|
| Euro     | EUR  | 9%       |
| US Dollar| USD  | 10%      |
| Canadian Dollar | CAD | 11% |

## 🧪 Testing

### Run Tests
```bash
dotnet test
```

### Test Coverage

The solution includes comprehensive unit tests covering:

- **USD Calculations**: Test case with 123.45 EUR → USD conversion
- **EUR Calculations**: Test case with 1000.00 EUR (no conversion)
- **CAD Calculations**: Test case with 6543.21 EUR → CAD conversion
- **Error Handling**: Negative amounts and unsupported currencies
- **Mocked Dependencies**: Exchange rate client is mocked for reliable testing

### Test Examples

```csharp
[Fact]
public async Task Calculate_USD_TestCase1()
{
    // Arrange
    var service = CreateService(1.187247m);
    var request = new InvoiceCalculationRequest
    {
        InvoiceDate = new DateTime(2020, 8, 5),
        PreTaxAmount = 123.45m,
        PaymentCurrency = "USD"
    };
    
    // Act
    var result = await service.CalculateAsync(request);
    
    // Assert
    Assert.Equal(146.57m, result.PreTaxTotal);
    Assert.Equal(14.66m, result.TaxAmount);
    Assert.Equal(161.23m, result.GrandTotal);
}
```

## 🔧 Configuration

### App Settings

The application uses the following configuration:

```json
{
  "FixerApiKey": "your-fixer-api-key",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Dependencies

**Main API Project:**
- `Microsoft.AspNetCore.OpenApi` (8.0.15)
- `Swashbuckle.AspNetCore` (9.0.3)

**Test Project:**
- `Moq` (4.20.72) - Mocking framework
- `xunit` (2.9.2) - Testing framework
- `coverlet.collector` (6.0.2) - Code coverage

## 🏛️ Design Patterns

### Dependency Injection
- Services are registered in `Program.cs`
- `IExchangeRateClient` is injected into `InvoiceCalculatorService`
- Enables easy testing and loose coupling

### Interface Segregation
- `IExchangeRateClient` interface allows for different exchange rate providers
- Easy to swap implementations (e.g., for testing or different APIs)

### Repository Pattern
- Exchange rate client abstracts external API calls
- Business logic is separated from data access

## 🔒 Error Handling

The API provides comprehensive error handling:

- **400 Bad Request**: Invalid input (negative amounts, unsupported currencies)
- **500 Internal Server Error**: External API failures or unexpected errors
- **Input Validation**: Request body validation and business rule enforcement

## 📊 Business Logic

### Calculation Process

1. **Input Validation**
   - Pre-tax amount must be non-negative
   - Payment currency must be supported (EUR, USD, CAD)

2. **Currency Conversion**
   - If currency is EUR, no conversion needed (rate = 1.0)
   - Otherwise, fetch historical exchange rate from Fixer API
   - Convert pre-tax amount using the exchange rate

3. **Tax Calculation**
   - Apply currency-specific tax rate
   - Round to 2 decimal places using `MidpointRounding.AwayFromZero`

4. **Total Calculation**
   - Grand total = Pre-tax total + Tax amount
   - All amounts rounded to 2 decimal places

### Tax Rates by Currency

- **EUR**: 9% tax rate
- **USD**: 10% tax rate  
- **CAD**: 11% tax rate

## 🚀 Deployment

### Development
```bash
dotnet run --project InvoiceApi
```

### Production
```bash
dotnet publish -c Release
```

### Docker (if needed)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InvoiceApi/InvoiceApi.csproj", "InvoiceApi/"]
RUN dotnet restore "InvoiceApi/InvoiceApi.csproj"
COPY . .
WORKDIR "/src/InvoiceApi"
RUN dotnet build "InvoiceApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InvoiceApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvoiceApi.dll"]
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📝 License

This project is licensed under the MIT License.

## 🆘 Support

For issues and questions:
1. Check the existing issues
2. Create a new issue with detailed information
3. Include error messages and reproduction steps

---

**Note**: This API requires a valid Fixer API key for exchange rate functionality. The free tier has limitations on historical data access. 