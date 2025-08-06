# Prometheus Metrics Implementation

This document describes the Prometheus metrics implementation for the Invoice API.

## Overview

The Invoice API now exposes Prometheus metrics at `/metrics` endpoint, providing comprehensive monitoring capabilities for:

- HTTP request metrics (count, duration, status codes)
- Business-specific metrics (invoice calculations, exchange rate requests)
- System health metrics
- Custom application metrics

## Available Metrics

### HTTP Request Metrics (Built-in)
- `http_requests_total` - Total number of HTTP requests
- `http_request_duration_seconds` - Request duration histogram
- `http_current_requests` - Current number of active requests

### Custom Business Metrics

#### Invoice Calculation Metrics
- `invoice_calculations_total` - Total number of invoice calculations
  - Labels: `currency`, `success`
- `invoice_calculation_duration_seconds` - Duration of invoice calculations
  - Labels: `currency`
- `active_calculations` - Number of active calculations
  - Labels: `currency`

#### Exchange Rate Metrics
- `exchange_rate_requests_total` - Total number of exchange rate requests
  - Labels: `from_currency`, `to_currency`, `success`
- `exchange_rate_request_duration_seconds` - Duration of exchange rate requests
  - Labels: `from_currency`, `to_currency`

#### API Request Metrics
- `api_requests_total` - Total number of API requests
  - Labels: `endpoint`, `status_code`
- `api_request_duration_seconds` - Duration of API requests
  - Labels: `endpoint`

### Health Check Metrics
- `aspnetcore_health_checks` - Health check status
- `aspnetcore_health_checks_duration_seconds` - Health check duration

## Accessing Metrics

### Local Development
```bash
# Start the application
dotnet run

# Access metrics endpoint
curl http://localhost:8080/metrics
```

### Kubernetes Deployment
```bash
# Port forward to access metrics
kubectl port-forward deployment/invoiceapi 8080:8080

# Access metrics endpoint
curl http://localhost:8080/metrics
```

## Example Metrics Output

```
# HELP http_requests_total Total number of HTTP requests
# TYPE http_requests_total counter
http_requests_total{method="POST",endpoint="/api/invoice/calculate",status_code="200"} 15

# HELP invoice_calculations_total Total number of invoice calculations
# TYPE invoice_calculations_total counter
invoice_calculations_total{currency="USD",success="true"} 12
invoice_calculations_total{currency="EUR",success="false"} 3

# HELP invoice_calculation_duration_seconds Duration of invoice calculations
# TYPE invoice_calculation_duration_seconds histogram
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.001"} 2
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.002"} 5
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.004"} 8
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.008"} 10
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.016"} 12
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.032"} 12
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.064"} 12
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.128"} 12
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.256"} 12
invoice_calculation_duration_seconds_bucket{currency="USD",le="0.512"} 12
invoice_calculation_duration_seconds_bucket{currency="USD",le="+Inf"} 12
invoice_calculation_duration_seconds_sum{currency="USD"} 0.045
invoice_calculation_duration_seconds_count{currency="USD"} 12

# HELP active_calculations Number of active invoice calculations
# TYPE active_calculations gauge
active_calculations{currency="USD"} 0
active_calculations{currency="EUR"} 1
```

## Prometheus Configuration

The metrics endpoint is automatically discovered by Prometheus using the following annotations in the Kubernetes deployment:

```yaml
annotations:
  prometheus.io/scrape: "true"
  prometheus.io/port: "8080"
  prometheus.io/path: "/metrics"
```

## Grafana Dashboard Queries

### Request Rate
```
rate(http_requests_total{job="invoiceapi"}[5m])
```

### Error Rate
```
rate(http_requests_total{job="invoiceapi", status=~"5.."}[5m]) / rate(http_requests_total{job="invoiceapi"}[5m])
```

### Invoice Calculation Success Rate
```
rate(invoice_calculations_total{job="invoiceapi", success="true"}[5m]) / rate(invoice_calculations_total{job="invoiceapi"}[5m])
```

### Average Response Time
```
histogram_quantile(0.95, rate(invoice_calculation_duration_seconds_bucket{job="invoiceapi"}[5m]))
```

### Active Calculations
```
active_calculations{job="invoiceapi"}
```

## Alerting Rules

The following Prometheus alerting rules are configured:

```yaml
- alert: InvoiceAPIDown
  expr: up{job="invoiceapi"} == 0
  for: 1m
  labels:
    severity: critical
  annotations:
    summary: "Invoice API is down"
    description: "Invoice API has been down for more than 1 minute"

- alert: InvoiceAPIHighLatency
  expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket{job="invoiceapi"}[5m])) > 1
  for: 2m
  labels:
    severity: warning
  annotations:
    summary: "Invoice API high latency"
    description: "95th percentile latency is above 1 second"

- alert: InvoiceAPIHighErrorRate
  expr: rate(http_requests_total{job="invoiceapi", status=~"5.."}[5m]) / rate(http_requests_total{job="invoiceapi"}[5m]) > 0.05
  for: 2m
  labels:
    severity: warning
  annotations:
    summary: "Invoice API high error rate"
    description: "Error rate is above 5%"
```

## Testing Metrics

### Test Invoice Calculation
```bash
curl -X POST http://localhost:8080/api/invoice/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 100.00,
    "currency": "USD",
    "targetCurrency": "EUR",
    "date": "2024-01-15"
  }'
```

### Check Metrics After Request
```bash
curl http://localhost:8080/metrics | grep invoice_calculations
```

## Troubleshooting

### Metrics Not Appearing
1. Check if the application is running: `curl http://localhost:8080/health`
2. Verify metrics endpoint: `curl http://localhost:8080/metrics`
3. Check application logs for errors
4. Verify Prometheus configuration in Kubernetes

### High Memory Usage
- Monitor `process_resident_memory_bytes` metric
- Consider adjusting Prometheus scrape interval
- Review custom metrics implementation

### Missing Custom Metrics
1. Verify `MetricsService` is registered in DI container
2. Check if metrics are being recorded in controller/service
3. Ensure metrics are properly labeled
4. Restart application after configuration changes

## Performance Considerations

- Metrics collection adds minimal overhead (~1-2ms per request)
- Custom metrics are cached and updated efficiently
- Histogram buckets are optimized for typical response times
- Consider using sampling for high-traffic scenarios

## Security

- Metrics endpoint is exposed on the same port as the API
- No authentication is required for metrics endpoint
- Consider implementing authentication for production environments
- Use network policies to restrict access to metrics endpoint 