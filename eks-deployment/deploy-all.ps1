# ==============================================================================
# DEPLOY ALL MICROSERVICES TO EKS
# ==============================================================================
# This script deploys the entire microservices application to AWS EKS
# ==============================================================================

param(
    [string]$Namespace = "microservices-poc",
    [switch]$SkipInfrastructure,
    [switch]$WaitForReady
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deploying Microservices to AWS EKS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
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

# Display cluster information
Write-Host ""
Write-Host "Cluster Information:" -ForegroundColor Yellow
kubectl config current-context
Write-Host ""

# Confirm deployment
$confirmation = Read-Host "Do you want to proceed with deployment? (y/n)"
if ($confirmation -ne 'y') {
    Write-Host "Deployment cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Starting deployment..." -ForegroundColor Green
Write-Host ""

# Step 1: Create Namespace
Write-Host "[1/8] Creating namespace..." -ForegroundColor Cyan
kubectl apply -f base/namespace.yaml
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creating namespace" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Namespace created" -ForegroundColor Green
Write-Host ""

# Step 2: Create ConfigMaps
Write-Host "[2/8] Creating ConfigMaps..." -ForegroundColor Cyan
kubectl apply -f base/configmap.yaml
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creating ConfigMaps" -ForegroundColor Red
    exit 1
}
Write-Host "✓ ConfigMaps created" -ForegroundColor Green
Write-Host ""

# Step 3: Create Storage
Write-Host "[3/8] Creating Persistent Storage..." -ForegroundColor Cyan
kubectl apply -f base/storage.yaml
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creating storage" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Storage created" -ForegroundColor Green
Write-Host ""

if (-not $SkipInfrastructure) {
    # Step 4: Deploy Infrastructure (MongoDB, Redis, Elasticsearch)
    Write-Host "[4/8] Deploying infrastructure services..." -ForegroundColor Cyan
    
    Write-Host "  → Deploying MongoDB..." -ForegroundColor White
    kubectl apply -f infrastructure/mongodb.yaml
    
    Write-Host "  → Deploying Redis..." -ForegroundColor White
    kubectl apply -f infrastructure/redis.yaml
    
    Write-Host "  → Deploying Elasticsearch..." -ForegroundColor White
    kubectl apply -f infrastructure/elasticsearch.yaml
    
    Write-Host "✓ Infrastructure services deployed" -ForegroundColor Green
    Write-Host ""
    
    # Wait for infrastructure to be ready
    Write-Host "Waiting for infrastructure services to be ready..." -ForegroundColor Yellow
    Write-Host "  → Waiting for MongoDB..." -ForegroundColor White
    kubectl wait --for=condition=ready pod -l app=mongodb -n $Namespace --timeout=300s
    
    Write-Host "  → Waiting for Redis..." -ForegroundColor White
    kubectl wait --for=condition=ready pod -l app=redis -n $Namespace --timeout=180s
    
    Write-Host "  → Waiting for Elasticsearch..." -ForegroundColor White
    kubectl wait --for=condition=ready pod -l app=elasticsearch -n $Namespace --timeout=300s
    
    Write-Host "✓ Infrastructure services are ready" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "[4/8] Skipping infrastructure deployment" -ForegroundColor Yellow
    Write-Host ""
}

# Step 5: Deploy Applications
Write-Host "[5/8] Deploying application services..." -ForegroundColor Cyan

Write-Host "  → Deploying Products App..." -ForegroundColor White
kubectl apply -f apps/products-app.yaml

Write-Host "  → Deploying Users App..." -ForegroundColor White
kubectl apply -f apps/users-app.yaml

Write-Host "  → Deploying Orders App..." -ForegroundColor White
kubectl apply -f apps/orders-app.yaml

Write-Host "  → Deploying React App..." -ForegroundColor White
kubectl apply -f apps/react-app.yaml

Write-Host "✓ Application services deployed" -ForegroundColor Green
Write-Host ""

# Step 6: Deploy Nginx Proxy
Write-Host "[6/8] Deploying Nginx reverse proxy..." -ForegroundColor Cyan
kubectl apply -f apps/nginx-proxy.yaml
Write-Host "✓ Nginx proxy deployed" -ForegroundColor Green
Write-Host ""

# Step 7: Deploy Ingress
Write-Host "[7/8] Deploying AWS Load Balancer Ingress..." -ForegroundColor Cyan
kubectl apply -f apps/ingress.yaml
Write-Host "✓ Ingress deployed" -ForegroundColor Green
Write-Host ""

# Step 8: Verify Deployment
Write-Host "[8/8] Verifying deployment..." -ForegroundColor Cyan
Write-Host ""

if ($WaitForReady) {
    Write-Host "Waiting for application pods to be ready..." -ForegroundColor Yellow
    
    Write-Host "  → Waiting for Products App..." -ForegroundColor White
    kubectl wait --for=condition=ready pod -l app=products-app -n $Namespace --timeout=180s
    
    Write-Host "  → Waiting for Users App..." -ForegroundColor White
    kubectl wait --for=condition=ready pod -l app=users-app -n $Namespace --timeout=180s
    
    Write-Host "  → Waiting for Orders App..." -ForegroundColor White
    kubectl wait --for=condition=ready pod -l app=orders-app -n $Namespace --timeout=180s
    
    Write-Host "  → Waiting for React App..." -ForegroundColor White
    kubectl wait --for=condition=ready pod -l app=react-app -n $Namespace --timeout=180s
    
    Write-Host "  → Waiting for Nginx Proxy..." -ForegroundColor White
    kubectl wait --for=condition=ready pod -l app=nginx-proxy -n $Namespace --timeout=180s
    
    Write-Host "✓ All pods are ready" -ForegroundColor Green
    Write-Host ""
}

Write-Host "Deployment Status:" -ForegroundColor Yellow
Write-Host ""
kubectl get all -n $Namespace
Write-Host ""

Write-Host "Ingress Status:" -ForegroundColor Yellow
kubectl get ingress -n $Namespace
Write-Host ""

# Get Load Balancer URL
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Getting Load Balancer URL (this may take a few minutes)..." -ForegroundColor Yellow
$maxAttempts = 12
$attempt = 0
$lbUrl = ""

while ($attempt -lt $maxAttempts -and [string]::IsNullOrEmpty($lbUrl)) {
    $attempt++
    Write-Host "Attempt $attempt/$maxAttempts..." -ForegroundColor White
    
    $ingressInfo = kubectl get ingress microservices-ingress -n $Namespace -o json | ConvertFrom-Json
    if ($ingressInfo.status.loadBalancer.ingress.Count -gt 0) {
        $lbUrl = $ingressInfo.status.loadBalancer.ingress[0].hostname
    }
    
    if ([string]::IsNullOrEmpty($lbUrl)) {
        Start-Sleep -Seconds 10
    }
}

if (-not [string]::IsNullOrEmpty($lbUrl)) {
    Write-Host ""
    Write-Host "✓ Application is accessible at:" -ForegroundColor Green
    Write-Host "  http://$lbUrl" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "API Endpoints:" -ForegroundColor Yellow
    Write-Host "  Products: http://$lbUrl/api/products" -ForegroundColor White
    Write-Host "  Users:    http://$lbUrl/api/users" -ForegroundColor White
    Write-Host "  Orders:   http://$lbUrl/api/orders" -ForegroundColor White
    Write-Host ""
}
else {
    Write-Host ""
    Write-Host "Load Balancer is still provisioning. Check status with:" -ForegroundColor Yellow
    Write-Host "  kubectl get ingress -n $Namespace" -ForegroundColor White
    Write-Host ""
}

Write-Host "Useful Commands:" -ForegroundColor Yellow
Write-Host "  View pods:     kubectl get pods -n $Namespace" -ForegroundColor White
Write-Host "  View services: kubectl get svc -n $Namespace" -ForegroundColor White
Write-Host "  View logs:     kubectl logs -f deployment/<app-name> -n $Namespace" -ForegroundColor White
Write-Host "  Port forward:  kubectl port-forward svc/nginx-proxy 8080:80 -n $Namespace" -ForegroundColor White
Write-Host ""
