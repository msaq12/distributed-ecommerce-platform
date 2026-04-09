"use client";

import { useMsal } from "@azure/msal-react";
import ProtectedRoute from "@/components/protectedroute";
import Link from "next/link";

export default function ProfilePage() {
  const { accounts } = useMsal();
  const user = accounts[0];

  return (
    <ProtectedRoute>
      <div className="max-w-4xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-8">My Profile</h1>

        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <h2 className="text-xl font-bold mb-4">Account Information</h2>

          <div className="space-y-4">
            <div>
              <p className="text-sm text-gray-600">Display Name</p>
              <p className="font-medium text-lg">
                {user?.name || (user?.idTokenClaims as any)?.name || "N/A"}
              </p>
            </div>

            <div>
              <p className="text-sm text-gray-600">Email</p>
              <p className="font-medium text-lg">
                {user?.username ||
                  (user?.idTokenClaims as any)?.emails?.[0] ||
                  "N/A"}
              </p>
            </div>

            <div>
              <p className="text-sm text-gray-600">Given Name</p>
              <p className="font-medium text-lg">
                {(user?.idTokenClaims as any)?.given_name || "N/A"}
              </p>
            </div>

            <div>
              <p className="text-sm text-gray-600">Family Name</p>
              <p className="font-medium text-lg">
                {(user?.idTokenClaims as any)?.family_name || "N/A"}
              </p>
            </div>
          </div>
        </div>

        <div className="grid md:grid-cols-2 gap-6">
          <Link
            href="/profile/orders"
            className="bg-blue-600 text-white text-center py-4 rounded-lg font-semibold hover:bg-blue-700"
          >
            View Order History
          </Link>

          <Link
            href="/profile/edit"
            className="bg-gray-200 text-gray-800 text-center py-4 rounded-lg font-semibold hover:bg-gray-300"
          >
            Edit Profile
          </Link>
        </div>
      </div>
    </ProtectedRoute>
  );
}
