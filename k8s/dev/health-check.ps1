# Health Check for Monitoring Stack
# This script verifies that all monitoring components are running correctly

Write-Host "Checking monitoring stack health..." -ForegroundColor Green

# Check if monitoring namespace exists
Write-Host "`nChecking namespace..." -ForegroundColor Yellow
$namespace = kubectl get namespace monitoring --no-headers --output=name 2>$null
if ($namespace) {
    Write-Host "✓ Monitoring namespace exists" -ForegroundColor Green
} else {
    Write-Host "✗ Monitoring namespace not found" -ForegroundColor Red
    exit 1
}

# Check pod status
Write-Host "`nChecking pod status..." -ForegroundColor Yellow
$pods = kubectl get pods -n monitoring --no-headers
$allReady = $true

foreach ($pod in $pods) {
    $parts = $pod -split '\s+'
    $name = $parts[0]
    $ready = $parts[1]
    $status = $parts[2]
    
    if ($ready -like "*/*" -and $ready -notlike "*/0") {
        Write-Host "✓ $name - $status" -ForegroundColor Green
    } else {
        Write-Host "✗ $name - $status" -ForegroundColor Red
        $allReady = $false
    }
}

# Check services
Write-Host "`nChecking services..." -ForegroundColor Yellow
$services = kubectl get svc -n monitoring --no-headers
foreach ($service in $services) {
    $parts = $service -split '\s+'
    $name = $parts[0]
    $type = $parts[1]
    $clusterIP = $parts[2]
    
    Write-Host "✓ $name ($type) - $clusterIP" -ForegroundColor Green
}

# Check persistent volumes
Write-Host "`nChecking persistent volumes..." -ForegroundColor Yellow
$pvcs = kubectl get pvc -n monitoring --no-headers
foreach ($pvc in $pvcs) {
    $parts = $pvc -split '\s+'
    $name = $parts[0]
    $status = $parts[1]
    $capacity = $parts[2]
    
    if ($status -eq "Bound") {
        Write-Host "✓ $name - $capacity" -ForegroundColor Green
    } else {
        Write-Host "✗ $name - $status" -ForegroundColor Red
    }
}

# Test Prometheus endpoint
Write-Host "`nTesting Prometheus endpoint..." -ForegroundColor Yellow
try {
    $prometheusResponse = kubectl exec -n monitoring deployment/prometheus -- wget -qO- http://localhost:9090/api/v1/status/config 2>$null
    if ($prometheusResponse) {
        Write-Host "✓ Prometheus is responding" -ForegroundColor Green
    } else {
        Write-Host "✗ Prometheus is not responding" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Could not test Prometheus endpoint" -ForegroundColor Red
}

# Test Grafana endpoint
Write-Host "`nTesting Grafana endpoint..." -ForegroundColor Yellow
try {
    $grafanaResponse = kubectl exec -n monitoring deployment/grafana -- wget -qO- http://localhost:3000/api/health 2>$null
    if ($grafanaResponse) {
        Write-Host "✓ Grafana is responding" -ForegroundColor Green
    } else {
        Write-Host "✗ Grafana is not responding" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Could not test Grafana endpoint" -ForegroundColor Red
}

# Check if Invoice API has monitoring annotations
Write-Host "`nChecking Invoice API monitoring setup..." -ForegroundColor Yellow
$invoiceapiPod = kubectl get pods -l app=invoiceapi --no-headers 2>$null
if ($invoiceapiPod) {
    $parts = $invoiceapiPod -split '\s+'
    $name = $parts[0]
    $ready = $parts[1]
    $status = $parts[2]
    
    if ($ready -like "*/*" -and $ready -notlike "*/0") {
        Write-Host "✓ Invoice API pod is running" -ForegroundColor Green
        
        # Check for monitoring annotations
        $annotations = kubectl get pod $name -o jsonpath='{.metadata.annotations}' 2>$null
        if ($annotations -like "*prometheus.io/scrape*") {
            Write-Host "✓ Invoice API has monitoring annotations" -ForegroundColor Green
        } else {
            Write-Host "✗ Invoice API missing monitoring annotations" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ Invoice API pod not ready" -ForegroundColor Red
    }
} else {
    Write-Host "✗ Invoice API pod not found" -ForegroundColor Red
}

Write-Host "`nHealth check completed!" -ForegroundColor Green

if ($allReady) {
    Write-Host "`n🎉 All monitoring components are healthy!" -ForegroundColor Green
    Write-Host "`nAccess URLs:" -ForegroundColor Cyan
    Write-Host "  Prometheus: http://prometheus.local" -ForegroundColor White
    Write-Host "  Grafana: http://grafana.local (admin/admin123)" -ForegroundColor White
    Write-Host "  Invoice API: http://invoiceapi.local" -ForegroundColor White
} else {
    Write-Host "`n⚠️  Some components are not healthy. Check the logs above." -ForegroundColor Yellow
    Write-Host "`nTroubleshooting commands:" -ForegroundColor Cyan
    Write-Host "  kubectl get pods -n monitoring" -ForegroundColor White
    Write-Host "  kubectl logs -f deployment/prometheus -n monitoring" -ForegroundColor White
    Write-Host "  kubectl logs -f deployment/grafana -n monitoring" -ForegroundColor White
} 