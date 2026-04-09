"use client";

import { useEffect } from "react";
import { useMsal } from "@azure/msal-react";
import { useRouter } from "next/navigation";

export default function AuthCallbackPage() {
  const { instance } = useMsal();
  const router = useRouter();

  useEffect(() => {
    const handleCallback = async () => {
      try {
        console.log("Callback: Starting");
        const response = await instance.handleRedirectPromise();
        console.log("Callback: Response:", response);

        if (response !== null) {
          console.log("Login successful:", response.account);
        }

        // Always check accounts after handling redirect
        const accounts = instance.getAllAccounts();
        console.log("Accounts after redirect:", accounts);

        if (accounts.length > 0) {
          console.log("Account found, redirecting to home");
          setTimeout(() => router.push("/"), 1000); // Delay redirect
        }
      } catch (error) {
        console.error("Callback error:", error);
      }
    };

    handleCallback();
  }, [instance, router]);

  return (
    <div className="flex items-center justify-center min-h-screen">
      <p>Processing authentication...</p>
    </div>
  );
}
