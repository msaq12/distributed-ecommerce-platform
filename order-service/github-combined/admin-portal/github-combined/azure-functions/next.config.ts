import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  images: {
    unoptimized: true,
  },
  output: "standalone", // <-- add this line
};

export default nextConfig;
