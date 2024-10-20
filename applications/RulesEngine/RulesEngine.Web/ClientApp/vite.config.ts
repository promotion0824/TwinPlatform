import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import pluginRewriteAll from 'vite-plugin-rewrite-all';

// https://vitejs.dev/config/
// rewriteall plugin to handle "." in url's (https://github.com/vitejs/vite/issues/2415)
export default defineConfig({
  plugins: [react(), pluginRewriteAll()],
  base: './',
  server: {
    port: 3000,
    strictPort: true,
    hmr: {
      // go directly to vite server for hmr. Going through webserver reloads the page during server stop/start
      port: 3000
    }
  }
})
