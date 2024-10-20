import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import pluginRewriteAll from 'vite-plugin-rewrite-all';

// https://vitejs.dev/config/
// rewriteall plugin to handle "." in url's (https://github.com/vitejs/vite/issues/2415)
export default defineConfig({
  plugins: [react(), pluginRewriteAll()],
  base: './',
  server: {
    port: 3100,
    strictPort: true,
    hmr: {
      // go directly to vite server for hmr. Going through webserver reloads the page during server stop/start
      port: 3100
    },
    proxy: {
      '/api': {
        target: 'http://localhost:7070',
        changeOrigin: true,
        secure: false,
        ws: true,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('proxy error', err);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log('Sending Request to the Target:', req.method, req.url);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log('Received Response from the Target:', proxyRes.statusCode, req.url);
          });
        },
      }
    }
  }
})
