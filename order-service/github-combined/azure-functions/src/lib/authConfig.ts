import { Configuration, PopupRequest } from "@azure/msal-browser";

export const msalConfig: Configuration = {
  auth: {
    clientId:
      process.env.NEXT_PUBLIC_CLIENT_ID ||
      "8cd8010b-53d2-4053-b680-b6a3dd283ec1",
    authority:
      "https://furnituredropship.ciamlogin.com/e135a78d-ca29-4518-9e9f-75bde749d771",
    knownAuthorities: ["furnituredropship.ciamlogin.com"],
    redirectUri:
      process.env.NEXT_PUBLIC_REDIRECT_URI ||
      "http://localhost:3000/auth/callback",
    postLogoutRedirectUri:
      process.env.NEXT_PUBLIC_REDIRECT_URI?.replace("/auth/callback", "") ||
      "http://localhost:3000",
  },
  cache: {
    cacheLocation: "localStorage",
  },
};

export const loginRequest: PopupRequest = {
  scopes: ["openid", "profile", "email"],
};
