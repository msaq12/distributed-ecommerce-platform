# Azure Functions — Utilities

Serverless background processing for the Furniture Dropshipping Platform. Three functions handling image processing, scheduled cleanup, and invoice generation. Built on **.NET 8 Isolated** and deployed to Azure Functions Consumption plan.

---

## Deployed Resource

```
Name:     func-utilities-dev-[unique]
Location: Canada Central
Plan:     Consumption (Y1 Dynamic) — $0/month
Runtime:  .NET Isolated 8
```

---

## Functions

| Function          | Trigger   | Schedule / Route                         |
| ----------------- | --------- | ---------------------------------------- |
| ImageOptimization | Blob      | `uploads/{name}` container               |
| DailyCleanup      | Timer     | Every day at 2:00 AM UTC (`0 0 2 * * *`) |
| GenerateInvoice   | HTTP POST | `/api/invoices/generate`                 |

---

## ImageOptimization — Blob Trigger

Fires whenever a new file is uploaded to the `uploads` container.

- Validates file extension: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`
- Copies processed blob to `product-images` container
- Production hook: plug in `SixLabors.ImageSharp` for resize/compression before copy

**Container pipeline:**

```
uploads/          ← trigger input  (raw uploads)
product-images/   ← output         (processed, public blob access)
exports/          ← cleaned by timer
```

> Never trigger on the output container — causes infinite loop.

---

## DailyCleanup — Timer Trigger

Runs nightly at 2:00 AM UTC.

- Deletes blobs in `exports` container older than 7 days
- Calls Order Service `/api/orders/statistics` and logs result to Application Insights
- Order stats failure is non-fatal — logged as Warning, cleanup continues

---

## GenerateInvoice — HTTP Trigger

Internal API for generating invoice JSON from an order.

- **Auth**: `AuthorizationLevel.Function` — requires `?code=` key
- **Input**: `POST` with body `{ "orderId": 123 }`
- **Flow**: fetch order from Order Service → build invoice → return JSON

**Response codes**: `400` bad input · `404` order not found · `503` Order Service down · `200` invoice JSON

**Invoice response includes**: invoice number, bill-to/ship-to, line items, subtotal, tax, shipping, total.

---

## Project Structure

```
azure-functions/utilities-function/
├── UtilitiesFunction.csproj
├── Program.cs
├── host.json
├── local.settings.json
├── Functions/
│   ├── ImageOptimizationFunction.cs
│   ├── DailyCleanupFunction.cs
│   └── InvoiceGenerationFunction.cs
├── Models/
│   ├── InvoiceRequest.cs
│   └── InvoiceResponse.cs
└── Services/
    └── OrderApiClient.cs
```

---

## Local Development

### Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4: `npm install -g azure-functions-core-tools@4`
- Azure CLI (`az login` for Key Vault + storage auth)
- Azurite or real storage connection string in `local.settings.json`

### Run Locally

```powershell
cd azure-functions/utilities-function
az login
func start
# Functions listed at http://localhost:7071
```

### Test Invoice Function

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/api/invoices/generate" `
  -Method POST -Body '{"orderId": 21}' -ContentType "application/json"
```

### Trigger Timer Manually

```powershell
Invoke-RestMethod -Uri "http://localhost:7071/admin/functions/DailyCleanup" `
  -Method POST -Body '{}' -ContentType "application/json"
```

### Test Blob Trigger

Upload a file to the `uploads` container — the function fires automatically.

```powershell
az storage blob upload --account-name stfurndev[unique] `
  --container-name uploads --name test-sofa.jpg --file "C:\path\to\image.jpg" --auth-mode login
```

---

## App Settings

| Setting                                 | Value                                        |
| --------------------------------------- | -------------------------------------------- |
| `FUNCTIONS_WORKER_RUNTIME`              | `dotnet-isolated`                            |
| `FUNCTIONS_EXTENSION_VERSION`           | `~4`                                         |
| `OrderServiceUrl`                       | `http://135.237.18.70` (AKS LoadBalancer IP) |
| `BlobStorageConnectionString`           | Storage account connection string            |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | From `appi-furniture-dev`                    |

All sensitive values stored in **Key Vault** — accessed via **Managed Identity** (no passwords in code).

---

## Deploy to Azure

```powershell
cd azure-functions/utilities-function
dotnet publish -c Release -o ./publish
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

az functionapp deployment source config-zip `
  --name func-utilities-dev-[unique] `
  --resource-group rg-furniture-dev-canadacentral-001 `
  --src ./deploy.zip
```

---

## Monitoring

**Application Insights** — `appi-furniture-dev`:

```kusto
// All function executions
requests
| where cloud_RoleName contains "utilities"
| project timestamp, name, success, duration, resultCode
| order by timestamp desc

// Function log messages
traces
| where message contains "invoice" or message contains "cleanup" or message contains "image"
| project timestamp, message, severityLevel
| order by timestamp desc
```

---

## Architecture Decision

**Separate Function App per domain** — `func-utilities-dev` handles background utility work independently from `func-order-processor` (Service Bus trigger, Day 11) and `func-cache-invalidation` (Event Grid trigger, Day 12). Gives independent deployment, scaling, and failure isolation per domain.
