# Cleanup Monitoring Stack for Invoice API
# This script removes all monitoring components

Write-Host "Cleaning up monitoring stack..." -ForegroundColor Green

# Remove Ingress rules
Write-Host "Removing Ingress rules..." -ForegroundColor Yellow
kubectl delete -f monitoring-ingress.yaml --ignore-not-found=true

# Remove Node Exporter
Write-Host "Removing Node Exporter..." -ForegroundColor Yellow
kubectl delete -f node-exporter-deployment.yaml --ignore-not-found=true

# Remove Promtail
Write-Host "Removing Promtail..." -ForegroundColor Yellow
kubectl delete -f promtail-deployment.yaml --ignore-not-found=true

# Remove Loki
Write-Host "Removing Loki..." -ForegroundColor Yellow
kubectl delete -f loki-deployment.yaml --ignore-not-found=true

# Remove Grafana
Write-Host "Removing Grafana..." -ForegroundColor Yellow
kubectl delete -f grafana-deployment.yaml --ignore-not-found=true

# Remove Prometheus
Write-Host "Removing Prometheus..." -ForegroundColor Yellow
kubectl delete -f prometheus-deployment.yaml --ignore-not-found=true
kubectl delete -f prometheus-configmap.yaml --ignore-not-found=true

# Remove monitoring namespace (this will remove all remaining resources)
Write-Host "Removing monitoring namespace..." -ForegroundColor Yellow
kubectl delete namespace monitoring --ignore-not-found=true

# Remove persistent volumes (optional - uncomment if you want to remove storage)
# Write-Host "Removing persistent volumes..." -ForegroundColor Yellow
# kubectl delete pvc prometheus-pvc -n monitoring --ignore-not-found=true
# kubectl delete pvc grafana-pvc -n monitoring --ignore-not-found=true
# kubectl delete pvc loki-pvc -n monitoring --ignore-not-found=true

Write-Host "Monitoring stack cleanup completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Note: Persistent volumes were not deleted by default." -ForegroundColor Cyan
Write-Host "To remove storage, uncomment the PVC deletion lines in this script." -ForegroundColor Cyan 