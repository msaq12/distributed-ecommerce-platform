# E-commerce Website

Customer-facing furniture shopping website built with **Next.js**. Customers can browse products, add to cart, authenticate via **Microsoft Entra External ID** (Azure AD B2C), and place orders.

---

## Live URL

https://app-furniture-ecommerce-dev-dudqdpakerefarb8.canadacentral-01.azurewebsites.net

---

## Features

- 🛍️ Product catalog — browsing, filtering by category
- 🛒 Shopping cart (client-side state)
- 🔐 Customer authentication (Microsoft Entra External ID)
- 📦 Checkout → Order creation via APIM → Order Service
- 👤 Order history (authenticated users)

---

## Tech Stack

| Item      | Choice                           |
| --------- | -------------------------------- |
| Framework | Next.js (React)                  |
| Auth      | MSAL Browser + Entra External ID |
| API calls | Fetch via APIM gateway           |
| Styling   | CSS / Tailwind                   |

---

## Authentication

Uses **Microsoft Entra External ID** (consumer-facing B2C replacement):

- OAuth 2.0 Authorization Code Flow + PKCE
- Access token passed to APIM for protected endpoints
- APIM policy validates JWT signature and `aud` claim

---

## Local Development

```bash
cd ecommerce-web
npm install
cp .env.local.example .env.local   # fill in auth values
npm run dev
# http://localhost:3000
```

---

## E2E Tests

Playwright test suite in `../e2e-tests/`:

```bash
cd e2e-tests
npx playwright test
npx playwright show-report
```

Covers: homepage load, product catalog, cart interactions, checkout redirect, API health checks.

---

## CI/CD

- **CI**: Node 20, `npm ci`, `npm run build` → artifact `out/`
- **CD**: Deploy to App Service via zip deploy
