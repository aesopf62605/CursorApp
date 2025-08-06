# Deploy Monitoring Stack for Invoice API
# This script deploys Prometheus, Grafana, Loki, and related monitoring components

Write-Host "Deploying monitoring stack..." -ForegroundColor Green

# Create monitoring namespace
Write-Host "Creating monitoring namespace..." -ForegroundColor Yellow
kubectl apply -f monitoring-namespace.yaml

# Deploy Prometheus
Write-Host "Deploying Prometheus..." -ForegroundColor Yellow
kubectl apply -f prometheus-configmap.yaml
kubectl apply -f prometheus-deployment.yaml

# Deploy Grafana
Write-Host "Deploying Grafana..." -ForegroundColor Yellow
kubectl apply -f grafana-deployment.yaml

# Deploy Loki
Write-Host "Deploying Loki..." -ForegroundColor Yellow
kubectl apply -f loki-deployment.yaml

# Deploy Promtail
Write-Host "Deploying Promtail..." -ForegroundColor Yellow
kubectl apply -f promtail-deployment.yaml

# Deploy Node Exporter
Write-Host "Deploying Node Exporter..." -ForegroundColor Yellow
kubectl apply -f node-exporter-deployment.yaml

# Deploy Ingress rules
Write-Host "Deploying Ingress rules..." -ForegroundColor Yellow
kubectl apply -f monitoring-ingress.yaml

# Update Invoice API deployment with monitoring annotations
Write-Host "Updating Invoice API deployment..." -ForegroundColor Yellow
kubectl apply -f invoiceapi-deployment.yaml

# Wait for all pods to be ready
Write-Host "Waiting for pods to be ready..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l app=prometheus -n monitoring --timeout=300s
kubectl wait --for=condition=ready pod -l app=grafana -n monitoring --timeout=300s
kubectl wait --for=condition=ready pod -l app=loki -n monitoring --timeout=300s

Write-Host "Monitoring stack deployment completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Access URLs:" -ForegroundColor Cyan
Write-Host "  Prometheus: http://prometheus.local" -ForegroundColor White
Write-Host "  Grafana: http://grafana.local (admin/admin123)" -ForegroundColor White
Write-Host "  Invoice API: http://invoiceapi.local" -ForegroundColor White
Write-Host ""
Write-Host "To check pod status:" -ForegroundColor Yellow
Write-Host "  kubectl get pods -n monitoring" -ForegroundColor White
Write-Host ""
Write-Host "To view logs:" -ForegroundColor Yellow
Write-Host "  kubectl logs -f deployment/prometheus -n monitoring" -ForegroundColor White
Write-Host "  kubectl logs -f deployment/grafana -n monitoring" -ForegroundColor White 