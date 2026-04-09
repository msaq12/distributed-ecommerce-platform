# ============================================
# Deploy Product Invalidation Function
# ============================================
# Usage: .\deploy-cache-invalidate-function.ps1
# ============================================

param(
    [string]$FunctionAppName = "func-cache-invalidation-dev-kxbfgtdal2mic",
    [string]$ResourceGroup = "rg-furniture-dev-canadacentral-001"
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Step { param($message) Write-Host "`n>> $message" -ForegroundColor Cyan }
function Write-Success { param($message) Write-Host "[OK] $message" -ForegroundColor Green }
function Write-Fail { param($message) Write-Host "[FAIL] $message" -ForegroundColor Red }

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $scriptDir) { $scriptDir = Get-Location }

Write-Host "============================================" -ForegroundColor Yellow
Write-Host "  Cache Invalidation Function Deployment" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow
Write-Host "Function App: $FunctionAppName"
Write-Host "Resource Group: $ResourceGroup"
Write-Host "Source Directory: $scriptDir"

# Step 1: Verify Azure CLI is logged in
Write-Step "Checking Azure CLI login..."
try {
    $account = az account show 2>$null | ConvertFrom-Json
    Write-Success "Logged in as: $($account.user.name)"
} catch {
    Write-Fail "Not logged in to Azure CLI. Run 'az login' first."
    exit 1
}

# Step 2: Verify func CLI is installed
Write-Step "Checking Azure Functions Core Tools..."
try {
    $funcVersion = func --version 2>$null
    Write-Success "Azure Functions Core Tools version: $funcVersion"
} catch {
    Write-Fail "Azure Functions Core Tools not found. Install with: winget install Microsoft.Azure.FunctionsCoreTools"
    exit 1
}

# Step 3: Restore and Build
Write-Step "Building project..."
dotnet clean --verbosity quiet
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Fail "Build failed!"
    exit 1
}
Write-Success "Build completed"

# Step 4: Deploy
Write-Step "Deploying to Azure..."
func azure functionapp publish $FunctionAppName

if ($LASTEXITCODE -ne 0) {
    Write-Fail "Deployment failed!"
    exit 1
}
Write-Success "Deployment completed"

# Step 5: Verify deployment
Write-Step "Verifying deployment..."
Start-Sleep -Seconds 5

$functions = az functionapp function list --name $FunctionAppName --resource-group $ResourceGroup 2>$null | ConvertFrom-Json

if ($functions -and $functions.Count -gt 0) {
    Write-Success "Functions deployed:"
    foreach ($func in $functions) {
        Write-Host "  - $($func.name)" -ForegroundColor White
    }
} else {
    Write-Host "  Waiting for functions to register..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10

    $functions = az functionapp function list --name $FunctionAppName --resource-group $ResourceGroup 2>$null | ConvertFrom-Json
    if ($functions -and $functions.Count -gt 0) {
        Write-Success "Functions deployed:"
        foreach ($func in $functions) {
            Write-Host "  - $($func.name)" -ForegroundColor White
        }
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
$appUrl = "https://" + $FunctionAppName + ".azurewebsites.net"
Write-Host "Function App URL: $appUrl" -ForegroundColor White
Write-Host ""
