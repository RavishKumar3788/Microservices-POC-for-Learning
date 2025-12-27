# ==============================================================================
# CLEANUP EKS DEPLOYMENT
# ==============================================================================
# This script removes all deployed resources from EKS cluster
# ==============================================================================

param(
    [string]$Namespace = "microservices-poc",
    [switch]$KeepNamespace,
    [switch]$Force
)

Write-Host "========================================" -ForegroundColor Red
Write-Host "  EKS Cleanup Script" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""

# Check if kubectl is configured
Write-Host "Checking kubectl configuration..." -ForegroundColor Yellow
try {
    kubectl cluster-info | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: kubectl is not configured or cannot connect to cluster" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ kubectl is configured and connected" -ForegroundColor Green
}
catch {
    Write-Host "Error: kubectl is not installed or configured" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Cluster Information:" -ForegroundColor Yellow
kubectl config current-context
Write-Host ""

# Confirm deletion
if (-not $Force) {
    Write-Host "WARNING: This will delete all resources in namespace: $Namespace" -ForegroundColor Red
    $confirmation = Read-Host "Are you sure you want to continue? (yes/no)"
    if ($confirmation -ne 'yes') {
        Write-Host "Cleanup cancelled." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Starting cleanup..." -ForegroundColor Yellow
Write-Host ""

# Step 1: Delete Ingress (to remove AWS Load Balancer)
Write-Host "[1/7] Deleting Ingress (AWS Load Balancer)..." -ForegroundColor Cyan
kubectl delete -f apps/ingress.yaml --ignore-not-found=true
Write-Host "✓ Ingress deleted" -ForegroundColor Green
Write-Host ""

# Wait for Load Balancer to be deleted
Write-Host "Waiting for AWS Load Balancer to be deleted (30 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30
Write-Host ""

# Step 2: Delete Nginx Proxy
Write-Host "[2/7] Deleting Nginx reverse proxy..." -ForegroundColor Cyan
kubectl delete -f apps/nginx-proxy.yaml --ignore-not-found=true
Write-Host "✓ Nginx proxy deleted" -ForegroundColor Green
Write-Host ""

# Step 3: Delete Application Services
Write-Host "[3/7] Deleting application services..." -ForegroundColor Cyan

Write-Host "  → Deleting React App..." -ForegroundColor White
kubectl delete -f apps/react-app.yaml --ignore-not-found=true

Write-Host "  → Deleting Orders App..." -ForegroundColor White
kubectl delete -f apps/orders-app.yaml --ignore-not-found=true

Write-Host "  → Deleting Users App..." -ForegroundColor White
kubectl delete -f apps/users-app.yaml --ignore-not-found=true

Write-Host "  → Deleting Products App..." -ForegroundColor White
kubectl delete -f apps/products-app.yaml --ignore-not-found=true

Write-Host "✓ Application services deleted" -ForegroundColor Green
Write-Host ""

# Step 4: Delete Infrastructure Services
Write-Host "[4/7] Deleting infrastructure services..." -ForegroundColor Cyan

Write-Host "  → Deleting Elasticsearch..." -ForegroundColor White
kubectl delete -f infrastructure/elasticsearch.yaml --ignore-not-found=true

Write-Host "  → Deleting Redis..." -ForegroundColor White
kubectl delete -f infrastructure/redis.yaml --ignore-not-found=true

Write-Host "  → Deleting MongoDB..." -ForegroundColor White
kubectl delete -f infrastructure/mongodb.yaml --ignore-not-found=true

Write-Host "✓ Infrastructure services deleted" -ForegroundColor Green
Write-Host ""

# Step 5: Delete ConfigMaps
Write-Host "[5/7] Deleting ConfigMaps..." -ForegroundColor Cyan
kubectl delete -f base/configmap.yaml --ignore-not-found=true
Write-Host "✓ ConfigMaps deleted" -ForegroundColor Green
Write-Host ""

# Step 6: Delete Storage
Write-Host "[6/7] Deleting Persistent Storage..." -ForegroundColor Cyan
Write-Host "WARNING: This will delete all data in persistent volumes!" -ForegroundColor Yellow

if (-not $Force) {
    $storageConfirmation = Read-Host "Delete storage and all data? (yes/no)"
    if ($storageConfirmation -eq 'yes') {
        kubectl delete -f base/storage.yaml --ignore-not-found=true
        Write-Host "✓ Storage deleted" -ForegroundColor Green
    }
    else {
        Write-Host "Storage deletion skipped" -ForegroundColor Yellow
    }
}
else {
    kubectl delete -f base/storage.yaml --ignore-not-found=true
    Write-Host "✓ Storage deleted" -ForegroundColor Green
}
Write-Host ""

# Step 7: Delete Namespace
if (-not $KeepNamespace) {
    Write-Host "[7/7] Deleting namespace..." -ForegroundColor Cyan
    kubectl delete -f base/namespace.yaml --ignore-not-found=true
    Write-Host "✓ Namespace deleted" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "[7/7] Keeping namespace as requested" -ForegroundColor Yellow
    Write-Host ""
}

# Verify cleanup
Write-Host "Verifying cleanup..." -ForegroundColor Yellow
Write-Host ""

$remainingResources = kubectl get all -n $Namespace 2>&1
if ($LASTEXITCODE -eq 0 -and $remainingResources -notlike "*No resources found*") {
    Write-Host "Remaining resources in namespace:" -ForegroundColor Yellow
    kubectl get all -n $Namespace
    Write-Host ""
    Write-Host "Note: Some resources may take time to fully terminate" -ForegroundColor Yellow
}
else {
    Write-Host "✓ All resources cleaned up" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Cleanup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

Write-Host "Check AWS Console to verify:" -ForegroundColor Yellow
Write-Host "  - Load Balancers have been deleted" -ForegroundColor White
Write-Host "  - EBS volumes have been deleted (if PVs were deleted)" -ForegroundColor White
Write-Host ""
