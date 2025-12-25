# ==============================================================================
# CLEANUP SCRIPT - PowerShell
# ==============================================================================
# This script removes all deployed Kubernetes resources
# ==============================================================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Red
Write-Host "  Kubernetes Cleanup" -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""

$confirm = Read-Host "This will DELETE all resources in namespace 'microservices-poc'. Continue? (yes/no)"

if ($confirm -ne "yes") {
    Write-Host "Cleanup cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Deleting namespace and all resources..." -ForegroundColor Yellow
kubectl delete namespace microservices-poc

Write-Host ""
Write-Host "Waiting for cleanup to complete..." -ForegroundColor Gray
kubectl wait --for=delete namespace/microservices-poc --timeout=180s 2>$null

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Cleanup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "All resources have been removed." -ForegroundColor Green
Write-Host ""
