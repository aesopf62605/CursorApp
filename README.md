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
  - Sign up at [Fixer.io](https://fixer.io/) to get a free API key
  - Free tier includes latest rates but historical rates require paid plan

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
   
   **Note**: The API will first try to fetch historical rates. If your API key doesn't have access to historical data (free tier limitation), it will automatically fall back to using the latest available rates.

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
- **API Fallback**: If historical rates are unavailable (free tier limitation), automatically falls back to latest rates

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

### Docker Deployment

#### Option 1: Using Docker Compose (Recommended)

1. **Create environment file:**
   ```bash
   cp env.example .env
   # Edit .env and add your Fixer API key
   ```

2. **Build and run:**
   ```bash
   docker-compose up -d
   ```

3. **Access the API:**
   - API: `http://localhost:8080`
   - Swagger UI: `http://localhost:8080/swagger`

#### Option 2: Using Docker directly

1. **Build the image:**
   ```bash
   docker build -t invoice-api .
   ```

2. **Run the container:**
   ```bash
   docker run -d \
     --name invoice-api \
     -p 8080:8080 \
     -e FixerApiKey=your-api-key-here \
     invoice-api
   ```

3. **Access the API:**
   - API: `http://localhost:8080`
   - Swagger UI: `http://localhost:8080/swagger`

#### Docker Commands

```bash
# Build image
docker build -t invoice-api .

# Run container
docker run -d --name invoice-api -p 8080:8080 -e FixerApiKey=your-key invoice-api

# View logs
docker logs invoice-api

# Stop container
docker stop invoice-api

# Remove container
docker rm invoice-api

# Using docker-compose
docker-compose up -d
docker-compose down
docker-compose logs -f
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

## 🔧 Troubleshooting

### Common API Issues

1. **Unauthorized Error (401/403)**
   - Ensure your Fixer API key is valid and active
   - Check if you're using the correct API key in `appsettings.json`
   - Free tier API keys may not have access to historical data

2. **Historical Rates Not Available**
   - The API automatically falls back to latest rates if historical data is unavailable
   - This is normal behavior for free tier API keys

3. **Rate Limiting**
   - Free tier has rate limits (typically 100 requests/month)
   - Consider upgrading to a paid plan for higher limits

4. **Invalid Currency Codes**
   - Ensure you're using valid 3-letter currency codes (EUR, USD, CAD)
   - The API only supports EUR, USD, and CAD currencies

### Testing the API

You can test the API using the Swagger UI at `https://localhost:7001/swagger` or using curl:

```bash
curl -X POST "https://localhost:7001/api/invoice/calculate" \
  -H "Content-Type: application/json" \
  -d '{
    "invoiceDate": "2020-08-05",
    "preTaxAmount": 123.45,
    "paymentCurrency": "USD"
  }'
```

---

**Note**: This API requires a valid Fixer API key for exchange rate functionality. The free tier has limitations on historical data access. 