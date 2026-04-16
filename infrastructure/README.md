![Azure](https://img.shields.io/badge/Azure-0089D0?style=flat&logo=microsoft-azure&logoColor=white)
![.NET](https://img.shields.io/badge/.NET%208-512BD4?style=flat&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-20232A?style=flat&logo=react&logoColor=61DAFB)
![Kubernetes](https://img.shields.io/badge/Kubernetes-326CE5?style=flat&logo=kubernetes&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)

# Azure E-commerce Microservices Platform

A **production-grade, cloud-native e-commerce platform** built on Microsoft Azure, demonstrating microservices architecture, containerization, CI/CD automation, and enterprise security.

Built as a hands-on learning project targeting **AZ-204** and **AZ-400** certifications.

---

## 🏗️ Architecture

> See [architecture diagram](./docs/architecture.png) for full visual overview.

| Layer             | Technology                                          | Purpose                                             |
| ----------------- | --------------------------------------------------- | --------------------------------------------------- |
| Customer Frontend | Next.js → Azure Static Web App                      | Product catalog, cart, checkout                     |
| Admin Portal      | React + Vite → Azure App Service                    | Product & order management                          |
| API Gateway       | Azure API Management (Consumption)                  | Single entry point, rate limiting, JWT validation   |
| Product Service   | ASP.NET Core → Azure App Service                    | Product CRUD with Cosmos DB                         |
| Order Service     | ASP.NET Core → AKS (2 replicas)                     | Order lifecycle with SQL                            |
| Identity          | Azure AD B2C (External ID)                          | Customer auth; Azure AD for staff                   |
| Messaging         | Azure Service Bus + Event Grid                      | Async order processing, product events              |
| Serverless        | Azure Functions (3 triggers)                        | Invoice generation, blob processing, scheduled jobs |
| Storage           | Cosmos DB, Azure SQL, Redis, Blob                   | Multi-model persistence                             |
| Observability     | Application Insights + Log Analytics                | Distributed tracing, dashboards, alerts             |
| Security          | Key Vault, Managed Identity, Private Endpoints, NSG | Zero hard-coded secrets                             |
| CI/CD             | Azure DevOps (4 CI + 4 CD pipelines)                | Build, test, deploy automation                      |
| IaC               | Bicep + ARM templates                               | 100% infrastructure as code                         |
| Testing           | xUnit, Jest, Playwright, k6/Azure Load Testing      | Unit, integration, E2E, performance                 |

---

## 📦 Repositories

| Repo                                 | Description                                |
| ------------------------------------ | ------------------------------------------ |
| [product-service](./product-service) | .NET 8 REST API, Cosmos DB, Redis caching  |
| [order-service](./order-service)     | .NET 8 REST API, Azure SQL, Kubernetes     |
| [ecommerce-web](./ecommerce-web)     | Next.js customer-facing website            |
| [admin-portal](./admin-portal)       | React + Vite internal management dashboard |
| [infrastructure](./infrastructure)   | Bicep modules + ARM templates              |
| [e2e-tests](./e2e-tests)             | Playwright end-to-end test suite           |

---

## 🚀 Live Endpoints

| Service                  | URL                                                                                     |
| ------------------------ | --------------------------------------------------------------------------------------- |
| E-commerce Site          | https://app-furniture-ecommerce-dev-dudqdpakerefarb8.canadacentral-01.azurewebsites.net |
| Admin Portal             | https://app-furniture-admin-dev-188.azurewebsites.net                                   |
| API Gateway              | https://apim-furniture-dev.azure-api.net                                                |
| Product Service (direct) | https://app-product-service-dev.azurewebsites.net                                       |

---

## 🛠️ Azure Services Used (15+)

Azure App Service · Azure Kubernetes Service · Azure Container Registry · Azure Cosmos DB · Azure SQL Database · Azure Cache for Redis · Azure Blob Storage · Azure Key Vault · Azure API Management · Azure Service Bus · Azure Event Grid · Azure Functions · Application Insights · Log Analytics · Azure AD / External ID · Azure Load Testing · Azure DevOps

---

## 📊 Performance Baseline (Day 26)

- **100 concurrent users**, 7-minute load test
- **p95 response time**: < 2,000ms threshold
- **Error rate**: < 5% threshold
- Tested via Azure Load Testing (k6 engine)

---

## 🔒 Security Highlights

- Zero hard-coded secrets — all via Key Vault references
- Managed Identity for all service-to-service auth
- Private Endpoints for SQL and Cosmos DB
- NSG on private endpoint subnet
- Microsoft Defender enabled (VMs, SQL, Storage, Containers, Key Vault)
- HTTPS-only + TLS 1.2 minimum on all App Services
- JWT validation at APIM layer

---

## 🧪 Test Coverage

| Service         | Unit | Integration | E2E           |
| --------------- | ---- | ----------- | ------------- |
| Product Service | ~82% | ✅          | ✅ Playwright |
| Order Service   | ~76% | ✅          | ✅ Playwright |
| Admin Portal    | ~70% | N/A         | ✅ Playwright |

---

## Getting Started

See individual service READMEs for local development setup.
See [DEPLOYMENT-RUNBOOK.md](./docs/DEPLOYMENT-RUNBOOK.md) for full deployment guide.

---

## Author

Built as a structured 28-day Azure learning project.
**Skills demonstrated**: AZ-204, AZ-400, microservices, Kubernetes, DevSecOps, cloud-native architecture.
