# RBAC Configuration for Azure DevOps Monitoring Deployment

This document explains the RBAC (Role-Based Access Control) configuration for deploying the monitoring stack through Azure DevOps.

## Overview

The monitoring stack requires specific permissions to deploy and manage resources in the Kubernetes cluster. The RBAC configuration provides the necessary permissions for Azure DevOps service accounts.

## Components

### 1. Service Account
- **Name**: `azure-devops-sa`
- **Namespace**: `monitoring`
- **Purpose**: Service account used by Azure DevOps for deploying monitoring components

### 2. Cluster Role
- **Name**: `azure-devops-monitoring-role`
- **Scope**: Cluster-wide
- **Permissions**:
  - Namespaces: create, get, list, watch, update, patch, delete
  - Pods, Services, ConfigMaps, PVCs, Secrets: full access
  - Deployments, DaemonSets, StatefulSets: full access
  - Ingresses: full access
  - RBAC resources: full access

### 3. Cluster Role Binding
- **Name**: `azure-devops-monitoring-binding`
- **Purpose**: Binds the cluster role to the service account

### 4. Namespace Role
- **Name**: `azure-devops-monitoring-role`
- **Namespace**: `monitoring`
- **Permissions**: Same as cluster role but scoped to the monitoring namespace

### 5. Role Binding
- **Name**: `azure-devops-monitoring-binding`
- **Namespace**: `monitoring`
- **Purpose**: Binds the namespace role to the service account

## Deployment Order

1. **Namespace**: Create the monitoring namespace
2. **RBAC**: Deploy service account, roles, and bindings
3. **Monitoring Stack**: Deploy Prometheus, Grafana, Loki, etc.

## Azure DevOps Integration

### Pipeline Configuration
The Azure Pipelines YAML includes:
```yaml
- task: KubernetesManifest@0
  displayName: Deploy monitoring namespace and RBAC
  inputs:
    action: deploy
    manifests: |
      $(Pipeline.Workspace)/k8s-dev/monitoring-namespace.yaml
      $(Pipeline.Workspace)/k8s-dev/azure-devops-rbac.yaml

- task: KubernetesManifest@0
  displayName: Deploy monitoring stack
  inputs:
    action: deploy
    namespace: monitoring
    serviceAccount: azure-devops-sa
    manifests: |
      $(Pipeline.Workspace)/k8s-dev/prometheus-configmap.yaml
      # ... other monitoring manifests
```

### Service Account Usage
- The monitoring deployment uses `azure-devops-sa` service account
- This provides the necessary permissions for creating and managing monitoring resources

## Troubleshooting

### Common Issues

1. **Permission Denied Errors**
   - Ensure the RBAC resources are deployed before the monitoring stack
   - Check that the service account exists and has proper bindings

2. **Namespace Mismatch**
   - The monitoring stack is deployed to the `monitoring` namespace
   - The Invoice API is deployed to the `cursorapp-ns-dev` namespace
   - Prometheus is configured to scrape the Invoice API across namespaces

3. **Service Account Not Found**
   - Verify the service account exists: `kubectl get serviceaccounts -n monitoring`
   - Check role bindings: `kubectl get clusterrolebindings | grep azure-devops`

### Verification Commands

```bash
# Check RBAC resources
kubectl get serviceaccounts -n monitoring
kubectl get clusterroles | grep azure-devops
kubectl get clusterrolebindings | grep azure-devops

# Check permissions
kubectl auth can-i create deployments --as=system:serviceaccount:monitoring:azure-devops-sa -n monitoring
kubectl auth can-i create namespaces --as=system:serviceaccount:monitoring:azure-devops-sa

# Check monitoring stack status
kubectl get pods -n monitoring
kubectl get services -n monitoring
```

## Security Considerations

1. **Principle of Least Privilege**: The RBAC configuration provides only the necessary permissions for monitoring deployment
2. **Namespace Isolation**: Monitoring components are isolated in their own namespace
3. **Service Account**: Dedicated service account for monitoring operations
4. **Audit Trail**: All RBAC operations are logged for audit purposes

## Maintenance

### Adding New Permissions
If new permissions are needed:
1. Update the cluster role in `azure-devops-rbac.yaml`
2. Redeploy the RBAC configuration
3. Test the new permissions

### Removing Permissions
1. Remove unnecessary permissions from the cluster role
2. Redeploy the RBAC configuration
3. Verify that monitoring still works correctly

## Files

- `azure-devops-rbac.yaml`: Main RBAC configuration
- `deploy-monitoring-with-rbac.ps1`: Deployment script with RBAC
- `cleanup-monitoring-with-rbac.ps1`: Cleanup script with RBAC
- `azure-pipelines.yml`: Updated pipeline configuration 