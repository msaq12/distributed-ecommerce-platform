# Product Service

ASP.NET Core 8 REST API serving the furniture product catalog. Stores products in **Azure Cosmos DB** with **Redis caching** and is secured via **Azure Key Vault + Managed Identity**.

---

## Architecture

Client → APIM (https://apim-furniture-dev.azure-api.net/products)
↓
App Service (app-product-service-dev)
↓
Cosmos DB (products container) ← Redis Cache
↑
Key Vault (connection string)

## Tech Stack

- **Framework**: ASP.NET Core 8
- **Database**: Azure Cosmos DB (NoSQL)
- **Cache**: Azure Cache for Redis (cache-aside pattern)
- **Secrets**: Azure Key Vault (Managed Identity, no passwords in code)
- **Hosting**: Azure App Service (B1 plan)
- **Documentation**: Swagger / OpenAPI (available at `/swagger`)

---

## API Endpoints

| Method | Path                            | Description                      |
| ------ | ------------------------------- | -------------------------------- |
| GET    | `/api/products`                 | List all products (cached 5 min) |
| GET    | `/api/products/{id}/{category}` | Get product by ID + category     |
| POST   | `/api/products`                 | Create product                   |
| PUT    | `/api/products/{id}`            | Update product                   |
| DELETE | `/api/products/{id}/{category}` | Delete product                   |

**Swagger UI**: https://app-product-service-dev.azurewebsites.net/swagger
**Via APIM**: https://apim-furniture-dev.azure-api.net/products/api/products
(requires `Ocp-Apim-Subscription-Key` header)

---

## Product Model

```json
{
  "id": "prod-001",
  "name": "Oslo Sofa",
  "category": "sofas",
  "price": 1299.99,
  "description": "...",
  "stockQuantity": 50,
  "hasVariants": false,
  "imageUrl": "https://stfurndev...blob.core.windows.net/product-images/oslo-sofa.jpg"
}
```

**Key design decision**: Single model with `hasVariants` flag supports both simple and variant products in the same Cosmos container.

---

## Local Development

### Prerequisites

- .NET 8 SDK
- Azure CLI (for Key Vault auth via `az login`)
- Access to the Azure dev environment

### Run Locally

```powershell
cd product-service/ProductService.Api
az login
dotnet restore
dotnet run
# Swagger at http://localhost:5000/swagger
```

### Run Tests

```powershell
cd product-service
dotnet test --collect:"XPlat Code Coverage"
# Unit coverage target: 80%+
```

---

## Deployment

Automated via Azure DevOps CI/CD:

- **CI**: Builds on every push to `main`/`develop` → produces `ProductService.Api.zip`
- **CD**: Multi-stage (Dev → Staging → Prod with manual approval gates)

Manual deploy:

```powershell
dotnet publish -c Release -o ./publish
az webapp deploy --resource-group rg-furniture-dev-eastus-001 --name app-product-service-dev --src-path ./publish.zip
```

---

## Key Configuration

| Setting                  | Source                  |
| ------------------------ | ----------------------- |
| `CosmosConnectionString` | Key Vault secret        |
| `RedisConnectionString`  | Key Vault secret        |
| `KeyVault:Uri`           | App Service app setting |

All secrets accessed via Managed Identity — no passwords stored in code or config files.

---

## CI/CD Pipeline

File: `azure-pipelines/ci-pipeline.yaml`

Stages: Build → Unit Tests → Publish Artifact → Security Scan
