# Service Account Troubleshooting Guide

## Overview

The monitoring stack requires several service accounts with specific RBAC permissions to function properly. This guide helps troubleshoot service account related issues.

## Required Service Accounts

### 1. Prometheus Service Account
- **Name**: `prometheus`
- **Namespace**: `monitoring`
- **Purpose**: Allows Prometheus to discover and scrape metrics from Kubernetes resources
- **Defined in**: `prometheus-deployment.yaml`

### 2. Promtail Service Account
- **Name**: `promtail`
- **Namespace**: `monitoring`
- **Purpose**: Allows Promtail to read logs from all pods across the cluster
- **Defined in**: `promtail-deployment.yaml`

### 3. Node Exporter Service Account
- **Name**: `node-exporter`
- **Namespace**: `monitoring`
- **Purpose**: Allows Node Exporter to access system-level metrics
- **Defined in**: `node-exporter-deployment.yaml`

## Common Issues

### Issue 1: Service Account Not Found

**Error Messages:**
```
error validating data: ValidationError(Deployment.spec.template.spec): unknown field "serviceAccountName"
pods "prometheus-xxx" is forbidden: error looking up service account monitoring/prometheus
```

**Solution:**
The service accounts are defined within each deployment manifest. Ensure the complete manifest is deployed:

```bash
# Check if service account exists
kubectl get serviceaccount prometheus -n monitoring

# If missing, redeploy the complete manifest
kubectl apply -f prometheus-deployment.yaml -n monitoring
```

### Issue 2: RBAC Permissions Denied

**Error Messages:**
```
forbidden: User "system:serviceaccount:monitoring:prometheus" cannot list pods
forbidden: User "system:serviceaccount:monitoring:promtail" cannot list nodes
```

**Solution:**
The ClusterRoles and ClusterRoleBindings are missing. Deploy the complete manifests:

```bash
# Check cluster role
kubectl get clusterrole prometheus

# Check cluster role binding
kubectl get clusterrolebinding prometheus

# Redeploy if missing
kubectl apply -f prometheus-deployment.yaml
kubectl apply -f promtail-deployment.yaml
kubectl apply -f node-exporter-deployment.yaml
```

### Issue 3: Namespace Mismatch

**Error Messages:**
```
the namespace from the provided object "monitoring" does not match the namespace "default"
```

**Solution:**
Ensure you specify the correct namespace when deploying:

```bash
kubectl apply -f prometheus-deployment.yaml -n monitoring
```

## Verification Steps

### 1. Check Service Accounts
```bash
# List all service accounts in monitoring namespace
kubectl get serviceaccounts -n monitoring

# Check specific service account
kubectl describe serviceaccount prometheus -n monitoring
```

### 2. Check RBAC Resources
```bash
# Check cluster roles
kubectl get clusterroles | grep -E "(prometheus|promtail|node-exporter)"

# Check cluster role bindings
kubectl get clusterrolebindings | grep -E "(prometheus|promtail|node-exporter)"

# Describe specific resources
kubectl describe clusterrole prometheus
kubectl describe clusterrolebinding prometheus
```

### 3. Test Permissions
```bash
# Test if prometheus service account can list pods
kubectl auth can-i list pods --as=system:serviceaccount:monitoring:prometheus

# Test if promtail service account can list nodes
kubectl auth can-i list nodes --as=system:serviceaccount:monitoring:promtail

# Test if node-exporter service account can list nodes
kubectl auth can-i list nodes --as=system:serviceaccount:monitoring:node-exporter
```

### 4. Check Pod Status
```bash
# Check if pods are running
kubectl get pods -n monitoring

# Check pod events for RBAC errors
kubectl describe pod <pod-name> -n monitoring

# Check pod logs
kubectl logs <pod-name> -n monitoring
```

## Manual Service Account Creation

If service accounts are missing, you can create them manually:

### Prometheus Service Account
```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: prometheus
  namespace: monitoring
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: prometheus
rules:
- apiGroups: [""]
  resources: ["nodes", "nodes/proxy", "services", "endpoints", "pods"]
  verbs: ["get", "list", "watch"]
- apiGroups: ["extensions"]
  resources: ["ingresses"]
  verbs: ["get", "list", "watch"]
- nonResourceURLs: ["/metrics"]
  verbs: ["get"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: prometheus
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: prometheus
subjects:
- kind: ServiceAccount
  name: prometheus
  namespace: monitoring
```

## Automated Verification

Use the verification script:
```bash
.\verify-service-accounts.ps1
```

This script will:
- Check if the monitoring namespace exists
- Verify all required service accounts exist
- Check cluster roles and bindings
- Provide a summary of missing resources

## Azure DevOps Integration

### Pipeline Updates
The Azure DevOps pipeline has been updated to:

1. **Create monitoring namespace first**
2. **Deploy each component separately** (service accounts are included in manifests)
3. **Remove explicit serviceAccount references** from pipeline tasks

### Key Pipeline Changes
```yaml
- task: KubernetesManifest@0
  displayName: Create monitoring namespace
  inputs:
    action: deploy
    manifests: |
      $(Pipeline.Workspace)/k8s-dev/monitoring-namespace.yaml

- task: KubernetesManifest@0
  displayName: Deploy Prometheus
  inputs:
    action: deploy
    namespace: monitoring
    manifests: |
      $(Pipeline.Workspace)/k8s-dev/prometheus-deployment.yaml
```

## Best Practices

1. **Always deploy complete manifests** - Don't split service accounts from deployments
2. **Use consistent namespaces** - Keep related resources in the same namespace
3. **Test RBAC permissions** - Verify service accounts have required permissions
4. **Monitor pod events** - Check for authentication/authorization errors
5. **Use verification scripts** - Automate checks for required resources

## Recovery Procedures

### Full Reset
```bash
# Delete monitoring namespace (removes all resources)
kubectl delete namespace monitoring

# Recreate namespace
kubectl create namespace monitoring

# Redeploy all monitoring components
kubectl apply -f prometheus-deployment.yaml -n monitoring
kubectl apply -f grafana-deployment.yaml -n monitoring
kubectl apply -f loki-deployment.yaml -n monitoring
kubectl apply -f promtail-deployment.yaml -n monitoring
kubectl apply -f node-exporter-deployment.yaml -n monitoring
```

### Selective Reset
```bash
# Delete specific RBAC resources
kubectl delete clusterrole prometheus promtail node-exporter
kubectl delete clusterrolebinding prometheus promtail node-exporter

# Redeploy specific components
kubectl apply -f prometheus-deployment.yaml -n monitoring
```