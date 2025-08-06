# Cleanup Monitoring Stack with RBAC
# This script removes the monitoring stack and RBAC resources

Write-Host "=== Cleaning up Monitoring Stack with RBAC ===" -ForegroundColor Red

# Set variables
$NAMESPACE = "monitoring"

# Delete monitoring components
Write-Host "Deleting monitoring components..." -ForegroundColor Yellow
kubectl delete -f monitoring-ingress.yaml --ignore-not-found=true
kubectl delete -f node-exporter-deployment.yaml --ignore-not-found=true
kubectl delete -f promtail-deployment.yaml --ignore-not-found=true
kubectl delete -f loki-deployment.yaml --ignore-not-found=true
kubectl delete -f grafana-deployment.yaml --ignore-not-found=true
kubectl delete -f prometheus-deployment.yaml --ignore-not-found=true
kubectl delete -f prometheus-configmap.yaml --ignore-not-found=true

# Delete RBAC resources
Write-Host "Deleting RBAC resources..." -ForegroundColor Yellow
kubectl delete -f azure-devops-rbac.yaml --ignore-not-found=true

# Delete namespace
Write-Host "Deleting monitoring namespace..." -ForegroundColor Yellow
kubectl delete namespace $NAMESPACE --ignore-not-found=true

Write-Host "`n=== Monitoring Stack Cleanup Complete! ===" -ForegroundColor Green 