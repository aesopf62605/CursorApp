# Deploy Monitoring Stack with RBAC for Azure DevOps
# This script deploys the monitoring stack with proper RBAC permissions

Write-Host "=== Deploying Monitoring Stack with RBAC ===" -ForegroundColor Green

# Set variables
$NAMESPACE = "monitoring"
$APP_NAMESPACE = "cursorapp-ns-dev"

# Create monitoring namespace if it doesn't exist
Write-Host "Creating monitoring namespace..." -ForegroundColor Yellow
kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

# Deploy RBAC first
Write-Host "Deploying RBAC configuration..." -ForegroundColor Yellow
kubectl apply -f azure-devops-rbac.yaml

# Wait for RBAC to be ready
Write-Host "Waiting for RBAC to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Deploy monitoring components
Write-Host "Deploying Prometheus..." -ForegroundColor Yellow
kubectl apply -f prometheus-configmap.yaml
kubectl apply -f prometheus-deployment.yaml

Write-Host "Deploying Grafana..." -ForegroundColor Yellow
kubectl apply -f grafana-deployment.yaml

Write-Host "Deploying Loki..." -ForegroundColor Yellow
kubectl apply -f loki-deployment.yaml

Write-Host "Deploying Promtail..." -ForegroundColor Yellow
kubectl apply -f promtail-deployment.yaml

Write-Host "Deploying Node Exporter..." -ForegroundColor Yellow
kubectl apply -f node-exporter-deployment.yaml

Write-Host "Deploying monitoring ingress..." -ForegroundColor Yellow
kubectl apply -f monitoring-ingress.yaml

# Wait for all pods to be ready
Write-Host "Waiting for monitoring pods to be ready..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l app=prometheus -n $NAMESPACE --timeout=300s
kubectl wait --for=condition=ready pod -l app=grafana -n $NAMESPACE --timeout=300s
kubectl wait --for=condition=ready pod -l app=loki -n $NAMESPACE --timeout=300s

# Check pod status
Write-Host "`n=== Monitoring Stack Status ===" -ForegroundColor Green
kubectl get pods -n $NAMESPACE

Write-Host "`n=== Services ===" -ForegroundColor Green
kubectl get services -n $NAMESPACE

Write-Host "`n=== Access URLs ===" -ForegroundColor Green
Write-Host "Prometheus: http://prometheus.monitoring.svc.cluster.local:9090" -ForegroundColor Cyan
Write-Host "Grafana: http://grafana.monitoring.svc.cluster.local:3000 (admin/admin123)" -ForegroundColor Cyan
Write-Host "Loki: http://loki.monitoring.svc.cluster.local:3100" -ForegroundColor Cyan

Write-Host "`n=== RBAC Status ===" -ForegroundColor Green
kubectl get serviceaccounts -n $NAMESPACE
kubectl get clusterroles | findstr azure-devops
kubectl get clusterrolebindings | findstr azure-devops

Write-Host "`n=== Monitoring Stack Deployed Successfully! ===" -ForegroundColor Green 