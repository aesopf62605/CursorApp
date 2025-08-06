# Monitoring Stack for Invoice API

This directory contains Kubernetes manifests for a comprehensive monitoring stack including Prometheus, Grafana, Loki, and related components.

## Components

### 1. Prometheus
- **Purpose**: Metrics collection and storage
- **Port**: 9090
- **Features**:
  - Scrapes metrics from Invoice API and Kubernetes components
  - Custom alerting rules for Invoice API
  - Persistent storage for metrics retention
  - RBAC configured for Kubernetes discovery

### 2. Grafana
- **Purpose**: Metrics visualization and dashboards
- **Port**: 3000
- **Credentials**: admin/admin123
- **Features**:
  - Pre-configured Prometheus data source
  - Custom Invoice API dashboard
  - Persistent storage for dashboards and settings

### 3. Loki
- **Purpose**: Log aggregation and storage
- **Port**: 3100
- **Features**:
  - Collects logs from all pods
  - Structured log parsing for Invoice API
  - Persistent storage for log retention

### 4. Promtail
- **Purpose**: Log collection agent
- **Features**:
  - DaemonSet running on all nodes
  - Collects logs from all pods
  - Sends logs to Loki
  - Structured parsing for JSON logs

### 5. Node Exporter
- **Purpose**: System metrics collection
- **Port**: 9100
- **Features**:
  - DaemonSet running on all nodes
  - Collects CPU, memory, disk, and network metrics
  - Exposes metrics for Prometheus scraping

## Quick Start

### Deploy Monitoring Stack

```powershell
# Run the deployment script
.\deploy-monitoring.ps1
```

### Manual Deployment

```bash
# Create namespace
kubectl apply -f monitoring-namespace.yaml

# Deploy components
kubectl apply -f prometheus-configmap.yaml
kubectl apply -f prometheus-deployment.yaml
kubectl apply -f grafana-deployment.yaml
kubectl apply -f loki-deployment.yaml
kubectl apply -f promtail-deployment.yaml
kubectl apply -f node-exporter-deployment.yaml
kubectl apply -f monitoring-ingress.yaml

# Update Invoice API with monitoring annotations
kubectl apply -f invoiceapi-deployment.yaml
```

## Access URLs

After deployment, you can access the monitoring tools at:

- **Prometheus**: http://prometheus.local
- **Grafana**: http://grafana.local (admin/admin123)
- **Invoice API**: http://invoiceapi.local

## Monitoring Features

### Invoice API Metrics
- Request rate and latency
- Error rates and status codes
- Active connections
- Custom business metrics

### Kubernetes Metrics
- Node resource usage (CPU, memory, disk)
- Pod status and health
- Service endpoints
- Cluster-wide metrics

### Logging
- Structured log collection from Invoice API
- JSON log parsing with trace correlation
- Log retention and search capabilities
- Integration with Grafana for log visualization

## Alerting Rules

Prometheus includes pre-configured alerting rules:

1. **InvoiceAPIDown**: Triggers when Invoice API is down for more than 1 minute
2. **InvoiceAPIHighLatency**: Triggers when 95th percentile latency exceeds 1 second
3. **InvoiceAPIHighErrorRate**: Triggers when error rate exceeds 5%

## Customization

### Adding Custom Metrics
To add custom metrics to your Invoice API:

1. Add Prometheus.NET NuGet package to your project
2. Configure metrics collection in your application
3. Expose metrics endpoint at `/metrics`

### Adding Custom Dashboards
To add custom Grafana dashboards:

1. Create dashboard JSON in `grafana-dashboards` ConfigMap
2. Restart Grafana deployment
3. Dashboard will be automatically provisioned

### Modifying Alert Rules
Edit the `alert_rules.yml` section in `prometheus-configmap.yaml` to add or modify alerting rules.

## Troubleshooting

### Check Pod Status
```bash
kubectl get pods -n monitoring
```

### View Logs
```bash
# Prometheus logs
kubectl logs -f deployment/prometheus -n monitoring

# Grafana logs
kubectl logs -f deployment/grafana -n monitoring

# Loki logs
kubectl logs -f deployment/loki -n monitoring
```

### Check Services
```bash
kubectl get svc -n monitoring
```

### Access Pod Shell
```bash
kubectl exec -it deployment/prometheus -n monitoring -- /bin/sh
```

## Storage

All components use persistent storage:
- **Prometheus**: 10Gi for metrics storage
- **Grafana**: 5Gi for dashboards and settings
- **Loki**: 10Gi for log storage

## Security Notes

- Default Grafana credentials: admin/admin123
- Consider changing passwords in production
- RBAC is configured for minimal required permissions
- Services are exposed via Ingress for controlled access

## Scaling

### Horizontal Pod Autoscaling
To enable HPA for the Invoice API:

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: invoiceapi-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: invoiceapi
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

### Prometheus Scaling
For high-volume environments, consider:
- Prometheus federation
- Remote storage (Thanos, Cortex)
- Horizontal scaling with sharding

## Maintenance

### Backup
- Grafana dashboards are stored in ConfigMaps
- Prometheus data is in persistent volumes
- Consider regular backups of persistent volumes

### Updates
- Update container images in deployment files
- Test in staging environment first
- Monitor for breaking changes in new versions

## Integration with CI/CD

Add monitoring deployment to your CI/CD pipeline:

```yaml
# Example GitHub Actions step
- name: Deploy Monitoring
  run: |
    kubectl apply -f k8s/dev/monitoring-namespace.yaml
    kubectl apply -f k8s/dev/
```

## Performance Considerations

- Monitor resource usage of monitoring components
- Adjust resource limits based on cluster size
- Consider using resource quotas for monitoring namespace
- Use node selectors for monitoring pod placement if needed 