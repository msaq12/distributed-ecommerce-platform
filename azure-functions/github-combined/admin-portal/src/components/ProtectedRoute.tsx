import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../lib/authConfig";
import LoadingSpinner from "./LoadingSpinner";

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { instance, accounts, inProgress } = useMsal();
  const isAuthenticated = accounts.length > 0;

  if (!isAuthenticated && inProgress === "none") {
    instance.loginRedirect(loginRequest);
    return <LoadingSpinner />;
  }

  if (inProgress !== "none" || !isAuthenticated) {
    return <LoadingSpinner />;
  }

  return <>{children}</>;
}
