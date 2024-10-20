import {defineConfig} from "vite";
import react from "@vitejs/plugin-react";
import dns from "dns";
import fs from "fs";
import basicSsl from "@vitejs/plugin-basic-ssl"

// Instead of hosting on 120.0.0.1, use localhost, https://vitejs.dev/config/server-options.html#server-host
dns.setDefaultResultOrder("verbatim");

// https://vitejs.dev/config/
export default defineConfig({
  base: "./",
  optimizeDeps: {
    esbuildOptions: {
      target: "es2020",
    },
  },
  define: {
    'process.env': process.env
  },
  esbuild: {
    // https://github.com/vitejs/vite/issues/8644#issuecomment-1159308803
    logOverride: {"this-is-undefined-in-esm": "silent"},
  },
  plugins: [
    react({
      babel: {
        //plugins: ["babel-plugin-macros", "babel-plugin-styled-components"],
      },
    }),
    basicSsl(),
  ],
  server: {
    host: "localhost",
    port: 3000,
    https: true
  },
});
