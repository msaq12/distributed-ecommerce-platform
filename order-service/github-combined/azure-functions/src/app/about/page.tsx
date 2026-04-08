"use client";
import { useEffect } from "react";

export default function AboutPage() {
  useEffect(() => {
    console.log("About page loaded!");
    document.title = "About Us - FurnitureHub";
  }, []);

  return <h1>About Us</h1>;
}
