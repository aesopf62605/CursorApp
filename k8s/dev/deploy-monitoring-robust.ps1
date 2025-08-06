# Robust Monitoring Stack Deployment for Azure DevOps
# This script handles namespace creation and RBAC deployment properly

Write-Host "=== Robust Monitoring Stack Deployment ===" -ForegroundColor Green

# Set variables
$MONITORING_NAMESPACE = "monitoring"
$APP_NAMESPACE = "cursorapp-ns-dev"

# Step 1: Create monitoring namespace first (without namespace specification)
Write-Host "Step 1: Creating monitoring namespace..." -ForegroundColor Yellow
kubectl create namespace $MONITORING_NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

# Wait for namespace to be ready
Write-Host "Waiting for namespace to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Step 2: Deploy RBAC resources to the monitoring namespace
Write-Host "Step 2: Deploying RBAC resources..." -ForegroundColor Yellow
kubectl apply -f azure-devops-rbac.yaml -n $MONITORING_NAMESPACE

# Wait for RBAC to be ready
Write-Host "Waiting for RBAC to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Step 3: Deploy monitoring components
Write-Host "Step 3: Deploying monitoring components..." -ForegroundColor Yellow

Write-Host "Deploying Prometheus..." -ForegroundColor Cyan
kubectl apply -f prometheus-configmap.yaml -n $MONITORING_NAMESPACE
kubectl apply -f prometheus-deployment.yaml -n $MONITORING_NAMESPACE

Write-Host "Deploying Grafana..." -ForegroundColor Cyan
kubectl apply -f grafana-deployment.yaml -n $MONITORING_NAMESPACE

Write-Host "Deploying Loki..." -ForegroundColor Cyan
kubectl apply -f loki-deployment.yaml -n $MONITORING_NAMESPACE

Write-Host "Deploying Promtail..." -ForegroundColor Cyan
kubectl apply -f promtail-deployment.yaml -n $MONITORING_NAMESPACE

Write-Host "Deploying Node Exporter..." -ForegroundColor Cyan
kubectl apply -f node-exporter-deployment.yaml -n $MONITORING_NAMESPACE

Write-Host "Deploying monitoring ingress..." -ForegroundColor Cyan
kubectl apply -f monitoring-ingress.yaml -n $MONITORING_NAMESPACE

# Wait for all pods to be ready
Write-Host "Waiting for monitoring pods to be ready..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l app=prometheus -n $MONITORING_NAMESPACE --timeout=300s
kubectl wait --for=condition=ready pod -l app=grafana -n $MONITORING_NAMESPACE --timeout=300s
kubectl wait --for=condition=ready pod -l app=loki -n $MONITORING_NAMESPACE --timeout=300s

# Check pod status
Write-Host "`n=== Monitoring Stack Status ===" -ForegroundColor Green
kubectl get pods -n $MONITORING_NAMESPACE

Write-Host "`n=== Services ===" -ForegroundColor Green
kubectl get services -n $MONITORING_NAMESPACE

Write-Host "`n=== Access URLs ===" -ForegroundColor Green
Write-Host "Prometheus: http://prometheus.$MONITORING_NAMESPACE.svc.cluster.local:9090" -ForegroundColor Cyan
Write-Host "Grafana: http://grafana.$MONITORING_NAMESPACE.svc.cluster.local:3000 (admin/admin123)" -ForegroundColor Cyan
Write-Host "Loki: http://loki.$MONITORING_NAMESPACE.svc.cluster.local:3100" -ForegroundColor Cyan

Write-Host "`n=== RBAC Status ===" -ForegroundColor Green
kubectl get serviceaccounts -n $MONITORING_NAMESPACE
kubectl get clusterroles | findstr azure-devops
kubectl get clusterrolebindings | findstr azure-devops

Write-Host "`n=== Monitoring Stack Deployed Successfully! ===" -ForegroundColor Green 