# ==============================================================================
# DEPLOY ALL - PowerShell Script
# ==============================================================================
# This script deploys all Kubernetes resources in the correct order
# ==============================================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Kubernetes Deployment - Step by Step" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if kubectl is available
try {
    kubectl version --client --short | Out-Null
    Write-Host "[OK] kubectl is installed" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] kubectl is not installed" -ForegroundColor Red
    Write-Host "Please install kubectl: https://kubernetes.io/docs/tasks/tools/" -ForegroundColor Yellow
    exit 1
}

# Check if connected to cluster
try {
    kubectl cluster-info | Out-Null
    Write-Host "[OK] Connected to Kubernetes cluster" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] Not connected to Kubernetes cluster" -ForegroundColor Red
    Write-Host "Please start Docker Desktop Kubernetes or Minikube" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Starting deployment..." -ForegroundColor Yellow
Write-Host ""

# Step 1: Namespace
Write-Host "[Step 1/12] Creating namespace..." -ForegroundColor Cyan
kubectl apply -f step1-namespace.yaml
Start-Sleep -Seconds 2

# Step 2: ConfigMaps
Write-Host "[Step 2/12] Creating ConfigMaps..." -ForegroundColor Cyan
kubectl apply -f step2-configmaps.yaml
Start-Sleep -Seconds 2

# Step 3: Storage
Write-Host "[Step 3/12] Creating persistent storage..." -ForegroundColor Cyan
kubectl apply -f step3-storage.yaml
Start-Sleep -Seconds 2

# Step 4: MongoDB
Write-Host "[Step 4/12] Deploying MongoDB..." -ForegroundColor Cyan
kubectl apply -f step4-mongodb.yaml
Start-Sleep -Seconds 2

# Step 5: Redis
Write-Host "[Step 5/12] Deploying Redis..." -ForegroundColor Cyan
kubectl apply -f step5-redis.yaml
Start-Sleep -Seconds 2

# Step 6: Elasticsearch
Write-Host "[Step 6/12] Deploying Elasticsearch..." -ForegroundColor Cyan
kubectl apply -f step7-elasticsearch.yaml
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "Waiting for databases to be ready (this may take 2-3 minutes)..." -ForegroundColor Yellow
Write-Host "You can watch progress with: kubectl get pods -n microservices-poc -w" -ForegroundColor Gray
Write-Host ""

# Wait for databases
Write-Host "Waiting for MongoDB..." -ForegroundColor Gray
kubectl wait --for=condition=ready pod -l app=mongodb -n microservices-poc --timeout=180s 2>$null
Write-Host "[OK] MongoDB is ready" -ForegroundColor Green

Write-Host "Waiting for Redis..." -ForegroundColor Gray
kubectl wait --for=condition=ready pod -l app=redis -n microservices-poc --timeout=180s 2>$null
Write-Host "[OK] Redis is ready" -ForegroundColor Green

Write-Host "Waiting for Elasticsearch..." -ForegroundColor Gray
kubectl wait --for=condition=ready pod -l app=elasticsearch -n microservices-poc --timeout=300s 2>$null
Write-Host "[OK] Elasticsearch is ready" -ForegroundColor Green

Write-Host ""

# Step 7: Products App
Write-Host "[Step 7/12] Deploying Products microservice..." -ForegroundColor Cyan
kubectl apply -f step8-products-app.yaml
Start-Sleep -Seconds 2

# Step 8: Users App
Write-Host "[Step 8/12] Deploying Users microservice..." -ForegroundColor Cyan
kubectl apply -f step9-users-app.yaml
Start-Sleep -Seconds 2

# Step 9: Orders App
Write-Host "[Step 9/12] Deploying Orders microservice..." -ForegroundColor Cyan
kubectl apply -f step10-orders-app.yaml
Start-Sleep -Seconds 2

# Step 10: React App
Write-Host "[Step 10/12] Deploying React frontend..." -ForegroundColor Cyan
kubectl apply -f step11-react-app.yaml
Start-Sleep -Seconds 2

# Step 11: Nginx Proxy
Write-Host "[Step 11/12] Deploying Nginx reverse proxy..." -ForegroundColor Cyan
kubectl apply -f step12-nginx-proxy.yaml
Start-Sleep -Seconds 2

# Step 12: Expose Service
Write-Host "[Step 12/12] Exposing service externally..." -ForegroundColor Cyan
kubectl apply -f step13-expose-service.yaml
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Show status
Write-Host "Checking deployment status..." -ForegroundColor Yellow
Write-Host ""
kubectl get all -n microservices-poc

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Access Your Application" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your application is now accessible at:" -ForegroundColor Green
Write-Host "  http://localhost:30080" -ForegroundColor White
Write-Host ""
Write-Host "API Endpoints:" -ForegroundColor Yellow
Write-Host "  Products API: http://localhost:30080/api/products" -ForegroundColor White
Write-Host "  Users API:    http://localhost:30080/api/users" -ForegroundColor White
Write-Host "  Orders API:   http://localhost:30080/api/orders" -ForegroundColor White
Write-Host ""
Write-Host "Swagger Documentation:" -ForegroundColor Yellow
Write-Host "  Products: http://localhost:30080/products-swagger" -ForegroundColor White
Write-Host "  Users:    http://localhost:30080/users-swagger" -ForegroundColor White
Write-Host "  Orders:   http://localhost:30080/orders-swagger" -ForegroundColor White
Write-Host ""
Write-Host "Useful Commands:" -ForegroundColor Yellow
Write-Host "  View all resources:  kubectl get all -n microservices-poc" -ForegroundColor White
Write-Host "  View logs:          kubectl logs -f deployment/products-app -n microservices-poc" -ForegroundColor White
Write-Host "  Scale service:      kubectl scale deployment products-app --replicas=3 -n microservices-poc" -ForegroundColor White
Write-Host "  Delete all:         kubectl delete namespace microservices-poc" -ForegroundColor White
Write-Host ""
Write-Host "Waiting for all pods to be ready..." -ForegroundColor Gray
Start-Sleep -Seconds 10
kubectl get pods -n microservices-poc
Write-Host ""
Write-Host "Happy Learning!" -ForegroundColor Green
Write-Host ""