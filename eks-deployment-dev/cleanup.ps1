# ==============================================================================
# CLEANUP SCRIPT - REMOVE ALL EKS RESOURCES
# ==============================================================================
# 
# This script safely removes all deployed resources from your EKS cluster.
# It's designed for learning, so it asks for confirmation before deleting.
# 
# What this script does:
# 1. Confirms you really want to delete everything
# 2. Deletes Ingress (removes AWS Load Balancer)
# 3. Deletes applications (Products, Users, Orders, React, Nginx)
# 4. Deletes infrastructure (MongoDB, Redis)
# 5. Optionally deletes persistent storage (data will be lost!)
# 6. Optionally deletes namespace (removes everything)
# 7. Verifies cleanup is complete
# 
# Usage:
#   .\cleanup.ps1                    # Interactive cleanup with confirmations
#   .\cleanup.ps1 -Force             # Skip confirmations (careful!)
#   .\cleanup.ps1 -KeepStorage       # Keep PVCs and data
#   .\cleanup.ps1 -KeepNamespace     # Keep namespace
# 
# ==============================================================================

param(
    [string]$Namespace = "microservices-dev",
    [switch]$Force,
    [switch]$KeepStorage,
    [switch]$KeepNamespace
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

function Write-Warning-Custom {
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
Write-Host ("=" * 80) -ForegroundColor Red
Write-Host "  CLEANUP SCRIPT - REMOVE ALL RESOURCES" -ForegroundColor Red
Write-Host ("=" * 80) -ForegroundColor Red
Write-Host ""

# ==============================================================================
# VERIFY KUBECTL
# ==============================================================================

Write-Info "Checking kubectl connection..."
try {
    kubectl cluster-info --request-timeout=5s | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Cannot connect to Kubernetes cluster"
        exit 1
    }
    Write-Success "Connected to cluster"
}
catch {
    Write-Error-Custom "kubectl is not installed or not configured"
    exit 1
}

Write-Host ""
Write-Info "Cluster information:"
$context = kubectl config current-context
Write-Host "  Context: $context" -ForegroundColor White
Write-Host "  Namespace: $Namespace" -ForegroundColor White

# ==============================================================================
# CONFIRMATION
# ==============================================================================

if (-not $Force) {
    Write-Host ""
    Write-Host ("=" * 80) -ForegroundColor Yellow
    Write-Host "WARNING: This will delete the following:" -ForegroundColor Red
    Write-Host ("=" * 80) -ForegroundColor Yellow
    Write-Host "  ✗ AWS Load Balancer (Internet access will be removed)" -ForegroundColor White
    Write-Host "  ✗ All application pods (Products, Users, Orders, React, Nginx)" -ForegroundColor White
    Write-Host "  ✗ Infrastructure pods (MongoDB, Redis)" -ForegroundColor White
    
    if (-not $KeepStorage) {
        Write-Host "  ✗ Persistent storage (ALL DATA WILL BE LOST!)" -ForegroundColor Red
    }
    else {
        Write-Host "  ✓ Persistent storage will be kept" -ForegroundColor Green
    }
    
    if (-not $KeepNamespace) {
        Write-Host "  ✗ Namespace '$Namespace'" -ForegroundColor White
    }
    else {
        Write-Host "  ✓ Namespace '$Namespace' will be kept" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Warning-Custom "AWS resources will stop incurring charges after deletion"
    Write-Host ""
    
    $confirmation = Read-Host "Are you sure you want to continue? Type 'yes' to confirm"
    if ($confirmation -ne 'yes') {
        Write-Info "Cleanup cancelled"
        exit 0
    }
}

Write-Host ""
Write-Success "Starting cleanup..."

# ==============================================================================
# STEP 1: DELETE INGRESS (AWS LOAD BALANCER)
# ==============================================================================

Write-Step "Deleting AWS Load Balancer Ingress" 1 7

Write-Info "Deleting ingress..."
kubectl delete -f apps/ingress.yaml --ignore-not-found=true 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Success "Ingress deleted"
    Write-Info "AWS Load Balancer will be removed (takes ~30 seconds)"
}
else {
    Write-Warning-Custom "Ingress may not exist or already deleted"
}

Write-Info "Waiting for AWS Load Balancer to be deleted..."
Start-Sleep -Seconds 30

# ==============================================================================
# STEP 2: DELETE NGINX PROXY
# ==============================================================================

Write-Step "Deleting Nginx reverse proxy" 2 7

Write-Info "Deleting nginx-proxy..."
kubectl delete -f apps/nginx-proxy.yaml --ignore-not-found=true 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Success "Nginx proxy deleted"
}
else {
    Write-Warning-Custom "Nginx proxy may not exist or already deleted"
}

# ==============================================================================
# STEP 3: DELETE APPLICATIONS
# ==============================================================================

Write-Step "Deleting application services" 3 7

$apps = @(
    @{Name = "React App"; File = "apps/react-app.yaml" },
    @{Name = "Orders App"; File = "apps/orders-app.yaml" },
    @{Name = "Users App"; File = "apps/users-app.yaml" },
    @{Name = "Products App"; File = "apps/products-app.yaml" }
)

foreach ($app in $apps) {
    Write-Info "Deleting $($app.Name)..."
    kubectl delete -f $app.File --ignore-not-found=true 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "$($app.Name) deleted"
    }
}

# ==============================================================================
# STEP 4: DELETE INFRASTRUCTURE
# ==============================================================================

Write-Step "Deleting infrastructure services" 4 7

Write-Info "Deleting Redis..."
kubectl delete -f infrastructure/redis.yaml --ignore-not-found=true 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Success "Redis deleted"
}

Write-Info "Deleting MongoDB..."
kubectl delete -f infrastructure/mongodb.yaml --ignore-not-found=true 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Success "MongoDB deleted"
}

# ==============================================================================
# STEP 5: DELETE CONFIGMAPS
# ==============================================================================

Write-Step "Deleting ConfigMaps" 5 7

Write-Info "Deleting ConfigMaps..."
kubectl delete -f base/configmap.yaml --ignore-not-found=true 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Success "ConfigMaps deleted"
}

# ==============================================================================
# STEP 6: DELETE STORAGE (OPTIONAL)
# ==============================================================================

Write-Step "Managing persistent storage" 6 7

if (-not $KeepStorage) {
    Write-Host ""
    Write-Host ("=" * 80) -ForegroundColor Yellow
    Write-Warning-Custom "ATTENTION: You are about to delete persistent storage!"
    Write-Host "This will permanently delete:" -ForegroundColor Red
    Write-Host "  • All MongoDB data (products, users, orders)" -ForegroundColor White
    Write-Host "  • All Redis cache data" -ForegroundColor White
    Write-Host "  • EBS volumes in AWS" -ForegroundColor White
    Write-Host ""
    Write-Host "This action CANNOT be undone!" -ForegroundColor Red
    Write-Host ("=" * 80) -ForegroundColor Yellow
    Write-Host ""
    
    if (-not $Force) {
        $storageConfirmation = Read-Host "Delete storage and ALL DATA? Type 'yes' to confirm"
        if ($storageConfirmation -eq 'yes') {
            Write-Info "Deleting persistent volumes and claims..."
            kubectl delete -f base/storage.yaml --ignore-not-found=true 2>&1 | Out-Null
            Write-Success "Storage deleted"
            Write-Info "EBS volumes will be removed from AWS"
        }
        else {
            Write-Info "Storage deletion skipped"
            $KeepStorage = $true
        }
    }
    else {
        kubectl delete -f base/storage.yaml --ignore-not-found=true 2>&1 | Out-Null
        Write-Success "Storage deleted"
    }
}
else {
    Write-Info "Keeping persistent storage (as requested)"
    Write-Success "PVCs and data are preserved"
}

# ==============================================================================
# STEP 7: DELETE NAMESPACE (OPTIONAL)
# ==============================================================================

Write-Step "Managing namespace" 7 7

if (-not $KeepNamespace) {
    Write-Info "Deleting namespace '$Namespace'..."
    kubectl delete -f base/namespace.yaml --ignore-not-found=true 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Namespace deleted"
        Write-Info "This removes any remaining resources in the namespace"
    }
}
else {
    Write-Info "Keeping namespace '$Namespace' (as requested)"
    Write-Success "Namespace preserved"
}

# ==============================================================================
# VERIFY CLEANUP
# ==============================================================================

Write-Host ""
Write-Host ("=" * 80) -ForegroundColor Cyan
Write-Host "Verifying cleanup..." -ForegroundColor Cyan
Write-Host ("=" * 80) -ForegroundColor DarkGray
Write-Host ""

$remainingPods = kubectl get pods -n $Namespace 2>&1
if ($LASTEXITCODE -ne 0 -or $remainingPods -like "*No resources found*") {
    Write-Success "All pods removed"
}
else {
    Write-Host "Remaining pods (may be terminating):" -ForegroundColor Yellow
    kubectl get pods -n $Namespace
    Write-Host ""
    Write-Info "Some pods may take 30-60 seconds to fully terminate"
}

$remainingPVC = kubectl get pvc -n $Namespace 2>&1
if ($KeepStorage) {
    if ($LASTEXITCODE -eq 0 -and $remainingPVC -notlike "*No resources found*") {
        Write-Host ""
        Write-Host "Preserved storage:" -ForegroundColor Yellow
        kubectl get pvc -n $Namespace
    }
}
else {
    if ($LASTEXITCODE -ne 0 -or $remainingPVC -like "*No resources found*") {
        Write-Success "All storage removed"
    }
}

# ==============================================================================
# AWS CONSOLE VERIFICATION
# ==============================================================================

Write-Host ""
Write-Host ("=" * 80) -ForegroundColor Yellow
Write-Host "VERIFY IN AWS CONSOLE" -ForegroundColor Yellow
Write-Host ("=" * 80) -ForegroundColor DarkGray
Write-Host ""
Write-Host "Please verify the following in AWS Console:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Load Balancers (EC2 > Load Balancers):" -ForegroundColor Cyan
Write-Host "   • `'microservices-dev-alb`' should be deleted or deleting" -ForegroundColor White
Write-Host ""
Write-Host "2. EBS Volumes (EC2 > Volumes):" -ForegroundColor Cyan
if ($KeepStorage) {
    Write-Host "   • Volumes are KEPT (will continue to incur small charges)" -ForegroundColor Yellow
    Write-Host "   • Delete manually if not needed: EC2 > Volumes > Delete" -ForegroundColor White
}
else {
    Write-Host "   • Volumes should be deleting (may take 1-2 minutes)" -ForegroundColor White
}
Write-Host ""
Write-Host "3. Target Groups (EC2 > Target Groups):" -ForegroundColor Cyan
Write-Host "   • Should be automatically cleaned up" -ForegroundColor White
Write-Host ""

# ==============================================================================
# COST IMPACT
# ==============================================================================

Write-Host ("=" * 80) -ForegroundColor Green
Write-Host "COST IMPACT" -ForegroundColor Green
Write-Host ("=" * 80) -ForegroundColor DarkGray
Write-Host ""
Write-Host "Resources no longer incurring charges:" -ForegroundColor Green
Write-Host "  ✓ Application Load Balancer (~$0.02/hour saved)" -ForegroundColor White
Write-Host "  ✓ Application pods (CPU/memory usage reduced)" -ForegroundColor White

if (-not $KeepStorage) {
    Write-Host "  ✓ EBS volumes (~$0.0001/hour saved)" -ForegroundColor White
}
else {
    Write-Host "  ⚠ EBS volumes still active (~$0.0001/hour)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "IMPORTANT: EKS cluster and EC2 nodes are still running!" -ForegroundColor Red
Write-Host ""
Write-Host "To completely stop AWS charges:" -ForegroundColor Yellow
Write-Host "  1. Delete node group:" -ForegroundColor White
Write-Host "     eksctl delete nodegroup --cluster=<cluster-name> --name=<nodegroup-name>" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  2. Delete cluster:" -ForegroundColor White
Write-Host "     eksctl delete cluster --name=<cluster-name> --region=<region>" -ForegroundColor DarkGray
Write-Host ""

# ==============================================================================
# COMPLETION
# ==============================================================================

Write-Host ("=" * 80) -ForegroundColor Green
Write-Host "  CLEANUP COMPLETE!" -ForegroundColor Green
Write-Host ("=" * 80) -ForegroundColor Green
Write-Host ""

if ($KeepStorage -or $KeepNamespace) {
    Write-Host "Summary:" -ForegroundColor Cyan
    if ($KeepStorage) {
        Write-Host "  • Storage preserved (data intact)" -ForegroundColor Yellow
    }
    if ($KeepNamespace) {
        Write-Host "  • Namespace preserved" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "To redeploy, run: .\deploy-all.ps1" -ForegroundColor Cyan
    Write-Host ""
}
else {
    Write-Success "All application resources removed from cluster"
    Write-Host ""
    Write-Host "Thank you for learning with this project! 🎓" -ForegroundColor Green
    Write-Host ""
}

Write-Host ("=" * 80) -ForegroundColor DarkGray
