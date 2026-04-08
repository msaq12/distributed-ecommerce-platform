# Admin Portal

React + Vite internal dashboard for managing products and orders. Protected by **Azure Active Directory** (employee authentication). Deployed to Azure App Service.

---

## Features

- 🔐 Azure AD authentication (Authorization Code + PKCE)
- 📦 Product management — list, create, edit, delete
- 📋 Order management — view all orders, update status
- 📊 Dashboard — summary metrics

**Live URL**: https://app-furniture-admin-dev-188.azurewebsites.net
**Auth**: Login with your Azure AD (tenant: 3d596dca-678d-4cfd-a4b7-495b9a445195) credentials

---

## Tech Stack

| Item       | Choice                  |
| ---------- | ----------------------- |
| Framework  | React 19 + TypeScript   |
| Build Tool | Vite 6                  |
| Auth       | MSAL React 5 (Azure AD) |
| Styling    | Tailwind CSS 3          |
| Icons      | Lucide React            |
| Routing    | React Router 7          |

---

## Local Development

```bash
cd admin-portal
npm install
cp .env.example .env   # fill in your Azure AD values
npm run dev
# http://localhost:5173
```

### Environment Variables

```bash
VITE_CLIENT_ID=08e49961-c861-4a32-a247-62f292b20bd5
VITE_TENANT_ID=3d596dca-678d-4cfd-a4b7-495b9a445195
VITE_REDIRECT_URI=http://localhost:5173/
VITE_APIM_BASE_URL=https://apim-furniture-dev.azure-api.net
VITE_APIM_KEY=your-subscription-key
```

---

## Auth Flow

1. User visits portal → redirected to Azure AD login
2. Azure AD issues JWT access token
3. Token passed in `Authorization: Bearer` header on all API calls
4. APIM validates JWT signature and claims

**Important hook rule**: `useMsal()` only in React components — never in `api.ts`. Pass token as parameter to API functions.

---

## Build & Deploy

```bash
npm run build        # outputs to dist/
```

Automated via Azure DevOps CI/CD pipeline (`azure-pipelines/ci-pipeline.yaml`).

---

## Test Coverage

```bash
npm test
npm run test:coverage   # target: 70%+
```
