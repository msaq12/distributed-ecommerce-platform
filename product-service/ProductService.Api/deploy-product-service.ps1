# Deploy Product Service to Azure App Service
# Usage: .\deploy-product-service.ps1

Write-Host "Starting Product Service Deployment..." -ForegroundColor Cyan

$projectPath = "C:\dev\furniture-dropship\product-service\ProductService.Api"
$appName = "app-product-service-dev-9191"
$resourceGroup = "rg-furniture-dev-eastus-001"

# Navigate to project
Set-Location $projectPath

# Clean previous build
Write-Host "Cleaning previous build..." -ForegroundColor Yellow
Remove-Item -Recurse -Force ./publish, ./deploy.zip -ErrorAction SilentlyContinue

# Publish for Linux runtime
Write-Host "Publishing for Linux runtime..." -ForegroundColor Yellow
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Create ZIP
Write-Host "Creating deployment package..." -ForegroundColor Yellow
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

# Deploy to Azure
Write-Host "Deploying to Azure..." -ForegroundColor Yellow
az webapp deploy `
  --name $appName `
  --resource-group $resourceGroup `
  --src-path ./deploy.zip `
  --type zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Deployment successful!" -ForegroundColor Green
    Write-Host "App URL: https://$appName.azurewebsites.net" -ForegroundColor Cyan
} else {
    Write-Host "❌ Deployment failed!" -ForegroundColor Red
    exit 1
}
