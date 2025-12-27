# ==============================================================================
# AUTOMATED DEPLOYMENT SCRIPT - COST-OPTIMIZED FOR LEARNING
# ==============================================================================
# 
# This script deploys the entire microservices application to AWS EKS.
# It's designed for learning, so it includes detailed output and explanations.
# 
# What this script does:
# 1. Verifies kubectl is configured
# 2. Creates namespace
# 3. Creates ConfigMaps and storage
# 4. Deploys infrastructure (MongoDB, Redis)
# 5. Waits for infrastructure to be ready
# 6. Deploys applications (Products, Users, Orders, React)
# 7. Deploys Nginx proxy
# 8. Creates AWS Load Balancer Ingress
# 9. Displays status and URLs
# 
# Usage:
#   .\deploy-all.ps1                    # Deploy everything
#   .\deploy-all.ps1 -WaitForReady      # Wait for all pods to be ready
#   .\deploy-all.ps1 -SkipInfrastructure # Skip MongoDB/Redis (if already deployed)
# 
# ==============================================================================

param(
    [string]$Namespace = "microservices-dev",
    [switch]$SkipInfrastructure,
    [switch]$WaitForReady
)

# ==============================================================================
# HELPER FUNCTIONS
# ==============================================================================

function Write-Step {
    param([string]$Message, [int]$Step, [int]$Total)
    Write-Host ""
    Write-Host "[$Step/$Total] $Message" -ForegroundColor Cyan
    Write-Host ("=" * 80) -ForegroundColor DarkGray
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "  → $Message" -ForegroundColor White
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

# ==============================================================================
# BANNER
# ==============================================================================

Write-Host ""
Write-Host ("=" * 80) -ForegroundColor Cyan
Write-Host "  MICROSERVICES DEPLOYMENT TO AWS EKS" -ForegroundColor Cyan
Write-Host "  Cost-Optimized Configuration for Learning" -ForegroundColor Cyan
Write-Host ("=" * 80) -ForegroundColor Cyan
Write-Host ""

# ==============================================================================
# STEP 1: VERIFY PREREQUISITES
# ==============================================================================

Write-Step "Verifying prerequisites" 1 8

Write-Info "Checking kubectl installation..."
try {
    $kubectlVersion = kubectl version --client
    if ($LASTEXITCODE -eq 0) {
        Write-Success "kubectl is installed"
    }
    else {
        Write-Error-Custom "kubectl is not installed or not in PATH"
        Write-Info "Install from: https://kubernetes.io/docs/tasks/tools/"
        exit 1
    }
}
catch {
    Write-Error-Custom "kubectl is not installed"
    exit 1
}

Write-Info "Checking cluster connection..."
try {
    kubectl cluster-info --request-timeout=5s | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Connected to Kubernetes cluster"
    }
    else {
        Write-Error-Custom "Cannot connect to Kubernetes cluster"
        Write-Info "Run: aws eks update-kubeconfig --region <region> --name <cluster-name>"
        exit 1
    }
}
catch {
    Write-Error-Custom "Cannot connect to cluster"
    exit 1
}

Write-Host ""
Write-Info "Cluster information:"
$context = kubectl config current-context
Write-Host "  Context: $context" -ForegroundColor White
$clusterInfo = kubectl cluster-info | Select-Object -First 1
Write-Host "  $clusterInfo" -ForegroundColor White

# Confirm deployment
Write-Host ""
Write-Warning "This will deploy resources to the cluster shown above."
Write-Host "Estimated AWS costs: ~`$0.10-0.20 for a few hours of learning" -ForegroundColor Yellow
$confirmation = Read-Host "Continue with deployment? (y/n)"
if ($confirmation -ne 'y') {
    Write-Info "Deployment cancelled"
    exit 0
}

# ==============================================================================
# STEP 2: CREATE NAMESPACE
# ==============================================================================

Write-Step "Creating namespace" 2 8

Write-Info "Applying namespace.yaml..."
kubectl apply -f base/namespace.yaml

if ($LASTEXITCODE -eq 0) {
    Write-Success "Namespace '$Namespace' created"
}
else {
    Write-Error-Custom "Failed to create namespace"
    exit 1
}

# ==============================================================================
# STEP 3: CREATE CONFIGMAPS
# ==============================================================================

Write-Step "Creating ConfigMaps" 3 8

Write-Info "Applying configmap.yaml..."
kubectl apply -f base/configmap.yaml

if ($LASTEXITCODE -eq 0) {
    Write-Success "ConfigMap 'app-config' created"
    Write-Info "Contains database connection strings and app configuration"
}
else {
    Write-Error-Custom "Failed to create ConfigMaps"
    exit 1
}

# ==============================================================================
# STEP 4: CREATE PERSISTENT STORAGE
# ==============================================================================

Write-Step "Creating persistent storage" 4 8

Write-Info "Applying storage.yaml..."
kubectl apply -f base/storage.yaml

if ($LASTEXITCODE -eq 0) {
    Write-Success "Storage resources created"
    Write-Info "MongoDB: 2 GB EBS volume"
    Write-Info "Redis: 1 GB EBS volume"
    Write-Warning "PVCs will bind when pods are scheduled (WaitForFirstConsumer mode)"
}
else {
    Write-Error-Custom "Failed to create storage"
    exit 1
}

# ==============================================================================
# STEP 5: DEPLOY INFRASTRUCTURE
# ==============================================================================

Write-Step "Deploying infrastructure services" 5 8

if (-not $SkipInfrastructure) {
    Write-Info "Deploying MongoDB..."
    kubectl apply -f infrastructure/mongodb.yaml
    if ($LASTEXITCODE -eq 0) {
        Write-Success "MongoDB deployment created"
    }
    
    Write-Info "Deploying Redis..."
    kubectl apply -f infrastructure/redis.yaml
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Redis deployment created"
    }
    
    Write-Host ""
    Write-Info "Waiting for infrastructure to be ready (this may take 1-2 minutes)..."
    Write-Info "MongoDB is starting up and binding to storage..."
    
    kubectl wait --for=condition=ready pod -l app=mongodb -n $Namespace --timeout=180s 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "MongoDB is ready"
    }
    else {
        Write-Warning "MongoDB is taking longer than expected (continuing anyway)"
    }
    
    Write-Info "Redis is starting up..."
    kubectl wait --for=condition=ready pod -l app=redis -n $Namespace --timeout=120s 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Redis is ready"
    }
    else {
        Write-Warning "Redis is taking longer than expected (continuing anyway)"
    }
}
else {
    Write-Info "Skipping infrastructure deployment (as requested)"
}

# ==============================================================================
# STEP 6: DEPLOY APPLICATIONS
# ==============================================================================

Write-Step "Deploying application services" 6 8

Write-Info "Deploying Products API..."
kubectl apply -f apps/products-app.yaml
Write-Success "Products app deployed"

Write-Info "Deploying Users API..."
kubectl apply -f apps/users-app.yaml
Write-Success "Users app deployed"

Write-Info "Deploying Orders API..."
kubectl apply -f apps/orders-app.yaml
Write-Success "Orders app deployed"

Write-Info "Deploying React frontend..."
kubectl apply -f apps/react-app.yaml
Write-Success "React app deployed"

# ==============================================================================
# STEP 7: DEPLOY NGINX PROXY
# ==============================================================================

Write-Step "Deploying Nginx reverse proxy" 7 8

Write-Info "Applying nginx-proxy.yaml..."
kubectl apply -f apps/nginx-proxy.yaml

if ($LASTEXITCODE -eq 0) {
    Write-Success "Nginx proxy deployed"
    Write-Info "Acts as API gateway routing traffic to microservices"
}
else {
    Write-Error-Custom "Failed to deploy Nginx proxy"
    exit 1
}

# ==============================================================================
# STEP 8: CREATE AWS LOAD BALANCER INGRESS
# ==============================================================================

Write-Step "Creating AWS Load Balancer Ingress" 8 8

Write-Info "Applying ingress.yaml..."
kubectl apply -f apps/ingress.yaml

if ($LASTEXITCODE -eq 0) {
    Write-Success "Ingress created"
    Write-Info "AWS Load Balancer Controller will provision an ALB (takes 2-3 minutes)"
}
else {
    Write-Error-Custom "Failed to create Ingress"
    Write-Warning "Make sure AWS Load Balancer Controller is installed"
    Write-Info "See README.md for installation instructions"
}

# ==============================================================================
# WAIT FOR PODS (OPTIONAL)
# ==============================================================================

if ($WaitForReady) {
    Write-Host ""
    Write-Host ("=" * 80) -ForegroundColor DarkGray
    Write-Info "Waiting for all pods to be ready..."
    Write-Host ("=" * 80) -ForegroundColor DarkGray
    
    $apps = @("products-app", "users-app", "orders-app", "react-app", "nginx-proxy")
    foreach ($app in $apps) {
        Write-Info "Waiting for $app..."
        kubectl wait --for=condition=ready pod -l app=$app -n $Namespace --timeout=180s 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "$app is ready"
        }
        else {
            Write-Warning "$app is taking longer than expected"
        }
    }
}

# ==============================================================================
# DISPLAY STATUS
# ==============================================================================

Write-Host ""
Write-Host ("=" * 80) -ForegroundColor Green
Write-Host "  DEPLOYMENT COMPLETE!" -ForegroundColor Green
Write-Host ("=" * 80) -ForegroundColor Green
Write-Host ""

Write-Host "Current Status:" -ForegroundColor Cyan
Write-Host ("=" * 80) -ForegroundColor DarkGray
kubectl get all -n $Namespace
Write-Host ""

Write-Host "Ingress Status:" -ForegroundColor Cyan
Write-Host ("=" * 80) -ForegroundColor DarkGray
kubectl get ingress -n $Namespace
Write-Host ""

# ==============================================================================
# GET LOAD BALANCER URL
# ==============================================================================

Write-Host "Retrieving Load Balancer URL..." -ForegroundColor Cyan
Write-Host ("=" * 80) -ForegroundColor DarkGray

$maxAttempts = 12
$attempt = 0
$lbUrl = ""

while ($attempt -lt $maxAttempts -and [string]::IsNullOrEmpty($lbUrl)) {
    $attempt++
    Write-Info "Attempt $attempt/$maxAttempts (waiting for ALB to provision)..."
    
    try {
        $ingressJson = kubectl get ingress microservices-ingress -n $Namespace -o json 2>&1 | ConvertFrom-Json
        if ($ingressJson.status.loadBalancer.ingress.Count -gt 0) {
            $lbUrl = $ingressJson.status.loadBalancer.ingress[0].hostname
        }
    }
    catch {
        # Ingress not ready yet
    }
    
    if ([string]::IsNullOrEmpty($lbUrl)) {
        Start-Sleep -Seconds 10
    }
}

Write-Host ""
if (-not [string]::IsNullOrEmpty($lbUrl)) {
    Write-Host ("=" * 80) -ForegroundColor Green
    Write-Host "  APPLICATION IS ACCESSIBLE!" -ForegroundColor Green
    Write-Host ("=" * 80) -ForegroundColor Green
    Write-Host ""
    Write-Host "Load Balancer URL:" -ForegroundColor Cyan
    Write-Host "  http://$lbUrl" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Test Endpoints:" -ForegroundColor Cyan
    Write-Host "  Health Check:  http://$lbUrl/nginx-health" -ForegroundColor White
    Write-Host "  Frontend:      http://$lbUrl/" -ForegroundColor White
    Write-Host "  Products API:  http://$lbUrl/api/products" -ForegroundColor White
    Write-Host "  Users API:     http://$lbUrl/api/users" -ForegroundColor White
    Write-Host "  Orders API:    http://$lbUrl/api/orders" -ForegroundColor White
    Write-Host ""
}
else {
    Write-Warning "Load Balancer is still provisioning..."
    Write-Info "This is normal! ALB takes 2-3 minutes to fully provision."
    Write-Host ""
    Write-Host "To check status, run:" -ForegroundColor Cyan
    Write-Host "  kubectl get ingress -n $Namespace" -ForegroundColor White
    Write-Host ""
    Write-Host "To get URL when ready, run:" -ForegroundColor Cyan
    Write-Host "  kubectl get ingress microservices-ingress -n $Namespace -o jsonpath=`'{.status.loadBalancer.ingress[0].hostname}`'" -ForegroundColor White
    Write-Host ""
}

# ==============================================================================
# USEFUL COMMANDS
# ==============================================================================

Write-Host ("=" * 80) -ForegroundColor Cyan
Write-Host "Useful Commands for Learning:" -ForegroundColor Cyan
Write-Host ("=" * 80) -ForegroundColor DarkGray
Write-Host ""
Write-Host "View all resources:" -ForegroundColor Yellow
Write-Host "  kubectl get all -n $Namespace" -ForegroundColor White
Write-Host ""
Write-Host "View pods:" -ForegroundColor Yellow
Write-Host "  kubectl get pods -n $Namespace" -ForegroundColor White
Write-Host "  kubectl describe pod [pod-name] -n $Namespace" -ForegroundColor White
Write-Host ""
Write-Host "View logs:" -ForegroundColor Yellow
Write-Host "  kubectl logs -f deployment/products-app -n $Namespace" -ForegroundColor White
Write-Host "  kubectl logs -f deployment/mongodb -n $Namespace" -ForegroundColor White
Write-Host ""
Write-Host "Connect to pods:" -ForegroundColor Yellow
Write-Host "  kubectl exec -it deployment/mongodb -n $Namespace -- mongosh" -ForegroundColor White
Write-Host "  kubectl exec -it deployment/redis -n $Namespace -- redis-cli" -ForegroundColor White
Write-Host ""
Write-Host "Port forwarding (test locally):" -ForegroundColor Yellow
Write-Host "  kubectl port-forward svc/nginx-proxy 8080:80 -n $Namespace" -ForegroundColor White
Write-Host "  # Then access: http://localhost:8080" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Monitor resources:" -ForegroundColor Yellow
Write-Host "  kubectl top nodes" -ForegroundColor White
Write-Host "  kubectl top pods -n $Namespace" -ForegroundColor White
Write-Host ""

# ==============================================================================
# COST REMINDER
# ==============================================================================

Write-Host ("=" * 80) -ForegroundColor Yellow
Write-Host "COST REMINDER" -ForegroundColor Yellow
Write-Host ("=" * 80) -ForegroundColor DarkGray
Write-Host ""
Write-Host "You are now running resources on AWS that incur costs:" -ForegroundColor Yellow
Write-Host "  • EKS Cluster: ~`$0.10/hour" -ForegroundColor White
Write-Host "  • EC2 Nodes: ~`$0.03-0.05/hour per node" -ForegroundColor White
Write-Host "  • Load Balancer: ~`$0.02/hour" -ForegroundColor White
Write-Host "  • EBS Volumes: ~`$0.0001/hour (3 GB total)" -ForegroundColor White
Write-Host ""
Write-Host "Estimated total: ~`$0.15-0.20 per hour" -ForegroundColor Yellow
Write-Host ""
Write-Host "IMPORTANT: Delete resources after learning to avoid charges!" -ForegroundColor Red
Write-Host "  Run: .\cleanup.ps1" -ForegroundColor White
Write-Host ""

Write-Host ("=" * 80) -ForegroundColor Green
Write-Host "Happy Learning! 🚀" -ForegroundColor Green
Write-Host ("=" * 80) -ForegroundColor Green
Write-Host ""
