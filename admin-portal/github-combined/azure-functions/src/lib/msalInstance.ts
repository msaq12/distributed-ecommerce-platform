import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from "./authConfig";

const instance = new PublicClientApplication(msalConfig);

// Initialize synchronously at module load
instance.initialize().catch(err => {
  console.error("MSAL initialization failed:", err);
});

export const msalInstance = instance;
