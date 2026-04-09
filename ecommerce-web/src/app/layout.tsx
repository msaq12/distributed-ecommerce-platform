"use client";

import { Inter } from "next/font/google";
import "./globals.css";
import Navbar from "@/components/Navbar";
import Footer from "@/components/Footer";
import { MsalProvider } from "@azure/msal-react";
import { msalInstance } from "@/lib/msalInstance";

const inter = Inter({ subsets: ["latin"] });

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body
        className={`${inter.className} flex flex-col min-h-screen bg-gray-50`}
      >
        <MsalProvider instance={msalInstance}>
          <Navbar />
          <main className="flex-1">{children}</main>
          <Footer />
        </MsalProvider>
      </body>
    </html>
  );
}
