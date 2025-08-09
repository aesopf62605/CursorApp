# Verify Service Accounts for Monitoring Stack
# This script verifies that all required service accounts are created

Write-Host "=== Verifying Service Accounts for Monitoring Stack ===" -ForegroundColor Green

$NAMESPACE = "monitoring"

# Check if monitoring namespace exists
Write-Host "Checking monitoring namespace..." -ForegroundColor Yellow
$namespaceExists = kubectl get namespace $NAMESPACE --ignore-not-found
if (-not $namespaceExists) {
    Write-Host "❌ Monitoring namespace does not exist!" -ForegroundColor Red
    Write-Host "Creating monitoring namespace..." -ForegroundColor Yellow
    kubectl create namespace $NAMESPACE
} else {
    Write-Host "✅ Monitoring namespace exists" -ForegroundColor Green
}

# List of expected service accounts
$expectedServiceAccounts = @(
    "prometheus",
    "promtail", 
    "node-exporter"
)

Write-Host "`n=== Checking Service Accounts ===" -ForegroundColor Cyan

$allGood = $true
foreach ($sa in $expectedServiceAccounts) {
    $exists = kubectl get serviceaccount $sa -n $NAMESPACE --ignore-not-found
    if ($exists) {
        Write-Host "✅ Service Account '$sa' exists in namespace '$NAMESPACE'" -ForegroundColor Green
    } else {
        Write-Host "❌ Service Account '$sa' does NOT exist in namespace '$NAMESPACE'" -ForegroundColor Red
        $allGood = $false
    }
}

Write-Host "`n=== Checking Cluster Roles ===" -ForegroundColor Cyan

$expectedClusterRoles = @(
    "prometheus",
    "promtail",
    "node-exporter"
)

foreach ($role in $expectedClusterRoles) {
    $exists = kubectl get clusterrole $role --ignore-not-found
    if ($exists) {
        Write-Host "✅ Cluster Role '$role' exists" -ForegroundColor Green
    } else {
        Write-Host "❌ Cluster Role '$role' does NOT exist" -ForegroundColor Red
        $allGood = $false
    }
}

Write-Host "`n=== Checking Cluster Role Bindings ===" -ForegroundColor Cyan

$expectedClusterRoleBindings = @(
    "prometheus",
    "promtail",
    "node-exporter"
)

foreach ($binding in $expectedClusterRoleBindings) {
    $exists = kubectl get clusterrolebinding $binding --ignore-not-found
    if ($exists) {
        Write-Host "✅ Cluster Role Binding '$binding' exists" -ForegroundColor Green
    } else {
        Write-Host "❌ Cluster Role Binding '$binding' does NOT exist" -ForegroundColor Red
        $allGood = $false
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "✅ All service accounts and RBAC resources are properly configured!" -ForegroundColor Green
} else {
    Write-Host "❌ Some service accounts or RBAC resources are missing!" -ForegroundColor Red
    Write-Host "This may cause pod creation failures. Please check the deployment logs." -ForegroundColor Yellow
}

Write-Host "`n=== Current Service Accounts in Monitoring Namespace ===" -ForegroundColor Cyan
kubectl get serviceaccounts -n $NAMESPACE

Write-Host "`n=== Current Cluster Roles (monitoring related) ===" -ForegroundColor Cyan
kubectl get clusterroles | findstr -E "(prometheus|promtail|node-exporter)"

Write-Host "`n=== Current Cluster Role Bindings (monitoring related) ===" -ForegroundColor Cyan
kubectl get clusterrolebindings | findstr -E "(prometheus|promtail|node-exporter)"