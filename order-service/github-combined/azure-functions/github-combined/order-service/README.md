# Order Service

ASP.NET Core 8 REST API managing the full order lifecycle. Runs in **Azure Kubernetes Service** (2 replicas) with **Azure SQL** as the data store and publishes events to **Azure Service Bus**.

---

## Architecture

Client → APIM (https://apim-furniture-dev.azure-api.net/orders)
↓
AKS (2 pods, LoadBalancer IP)
↓
Azure SQL (OrdersDB) + Service Bus (order-processing queue)
↑
Key Vault (SQL connection string via K8s Secret)

## Tech Stack

- **Framework**: ASP.NET Core 8
- **ORM**: Entity Framework Core 8 (SQL Server)
- **Container**: Docker (multi-stage, ~220MB image)
- **Orchestration**: Azure Kubernetes Service (AKS)
- **Messaging**: Azure Service Bus (order-processing queue)
- **Registry**: Azure Container Registry (dockerregistrydevenv.azurecr.io)

---

## API Endpoints

| Method | Path                           | Description              |
| ------ | ------------------------------ | ------------------------ |
| GET    | `/api/orders`                  | List orders (paginated)  |
| GET    | `/api/orders/{id}`             | Get order by ID          |
| GET    | `/api/orders/customer/{email}` | Orders by customer email |
| POST   | `/api/orders`                  | Create order             |
| PUT    | `/api/orders/{id}/status`      | Update order status      |

**Swagger UI**: `http://[AKS-LB-IP]/swagger` (when AKS running)
**Via APIM**: `https://apim-furniture-dev.azure-api.net/orders/api/orders`

### Order Statuses

`Pending` → `Processing` → `Shipped` → `Delivered` | `Cancelled` | `Returned`

---

## Data Model (Azure SQL — OrdersDB)

**Tables**: Orders, OrderItems
**Order Number Format**: `ORD-{YYYYMMDD}-{RANDOM6}`

---

## Local Development

```powershell
cd order-service/OrderService.Api
az login
dotnet restore
dotnet run
# Swagger at http://localhost:5000/swagger
```

### Run Tests

```powershell
cd order-service
dotnet test --collect:"XPlat Code Coverage"
# Unit coverage target: 76%+
# Integration tests: POST 201, GET 200, pagination, 400/404 validation
```

---

## Kubernetes Deployment

```powershell
# Start AKS cluster
az aks start --name aks-furniture-dev --resource-group rg-furniture-dev-eastus-001

# Get credentials
az aks get-credentials --resource-group rg-furniture-dev-eastus-001 --name aks-furniture-dev

# Check pods
kubectl get pods -n furniture-dev -l app=order-service

# View logs
kubectl logs -l app=order-service -n furniture-dev --follow

# Stop AKS (save cost ~$75/month when stopped)
az aks stop --name aks-furniture-dev --resource-group rg-furniture-dev-eastus-001
```

**Pod spec**: 2 replicas, 256Mi/512Mi memory, 0.25/0.5 CPU, liveness + readiness probes.

---

## CI/CD Pipeline

- **CI**: Docker build → Push to ACR → Publish K8s manifests artifact
- **CD**: `kubectl apply` to AKS with manual approval for staging/prod
- Image tagged: `{Build.BuildId}` + `latest`
