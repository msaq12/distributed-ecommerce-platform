import ReactDOM from "react-dom/client";
import { MsalProvider } from "@azure/msal-react";
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from "./lib/authConfig";
import App from "./App";
import "./index.css";

const msalInstance = new PublicClientApplication(msalConfig);

ReactDOM.createRoot(document.getElementById("root")!).render(
  <MsalProvider instance={msalInstance}>
    <App />
  </MsalProvider>,
);
