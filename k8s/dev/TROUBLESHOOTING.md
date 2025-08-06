# Troubleshooting Guide for Azure DevOps Monitoring Deployment

## Common Error: Namespace Mismatch

### Error Message
```
the namespace from the provided object "monitoring" does not match the namespace "cursorapp-ns-dev". You must pass '--namespace=monitoring' to perform this operation.
```

### Root Cause
The Azure DevOps pipeline environment is configured to deploy to the `cursorapp-ns-dev` namespace, but the monitoring manifests are configured for the `monitoring` namespace. This creates a conflict when trying to deploy namespace-scoped resources.

### Solution

#### Option 1: Updated Pipeline Configuration (Recommended)
The pipeline has been updated to handle namespace deployment correctly:

1. **Namespace Creation**: Deploy the monitoring namespace first without namespace specification
2. **RBAC Deployment**: Deploy RBAC resources to the monitoring namespace
3. **Component Deployment**: Deploy each monitoring component separately to the monitoring namespace

#### Option 2: Manual Deployment Script
Use the robust deployment script:
```powershell
.\deploy-monitoring-robust.ps1
```

#### Option 3: Manual Commands
```bash
# 1. Create monitoring namespace
kubectl create namespace monitoring

# 2. Deploy RBAC
kubectl apply -f azure-devops-rbac.yaml -n monitoring

# 3. Deploy monitoring components
kubectl apply -f prometheus-configmap.yaml -n monitoring
kubectl apply -f prometheus-deployment.yaml -n monitoring
kubectl apply -f grafana-deployment.yaml -n monitoring
kubectl apply -f loki-deployment.yaml -n monitoring
kubectl apply -f promtail-deployment.yaml -n monitoring
kubectl apply -f node-exporter-deployment.yaml -n monitoring
kubectl apply -f monitoring-ingress.yaml -n monitoring
```

### Verification Steps

1. **Check Namespace Exists**:
   ```bash
   kubectl get namespace monitoring
   ```

2. **Check RBAC Resources**:
   ```bash
   kubectl get serviceaccounts -n monitoring
   kubectl get clusterroles | grep azure-devops
   kubectl get clusterrolebindings | grep azure-devops
   ```

3. **Check Monitoring Pods**:
   ```bash
   kubectl get pods -n monitoring
   ```

4. **Check Services**:
   ```bash
   kubectl get services -n monitoring
   ```

### Alternative Solutions

#### Option A: Deploy Everything to cursorapp-ns-dev
If you prefer to keep everything in the same namespace:

1. Update all monitoring manifests to use `cursorapp-ns-dev` namespace
2. Update Prometheus configuration to target local services
3. Update Azure Pipelines to deploy without namespace specification

#### Option B: Use Helm Charts
Deploy monitoring stack using Helm charts which handle namespace management better:

```bash
# Add Prometheus Helm repository
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts

# Install Prometheus stack
helm install monitoring prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  --create-namespace
```

### Prevention

1. **Consistent Namespace Strategy**: Decide on namespace strategy early
2. **Pipeline Environment**: Configure pipeline environment to match deployment strategy
3. **Testing**: Test deployment scripts locally before pushing to Azure DevOps
4. **Documentation**: Document namespace requirements clearly

### Debugging Commands

```bash
# Check current context
kubectl config current-context

# Check available namespaces
kubectl get namespaces

# Check pipeline environment
echo $KUBERNETES_NAMESPACE

# Test namespace creation
kubectl create namespace monitoring --dry-run=client -o yaml

# Check RBAC permissions
kubectl auth can-i create deployments --as=system:serviceaccount:monitoring:azure-devops-sa -n monitoring
```

### Rollback Procedure

If deployment fails, clean up and retry:

```bash
# Clean up monitoring namespace
kubectl delete namespace monitoring --ignore-not-found=true

# Clean up RBAC resources
kubectl delete clusterrole azure-devops-monitoring-role --ignore-not-found=true
kubectl delete clusterrolebinding azure-devops-monitoring-binding --ignore-not-found=true

# Retry deployment
.\deploy-monitoring-robust.ps1
``` 