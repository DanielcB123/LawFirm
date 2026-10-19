import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    host: '0.0.0.0',
    port: 5173,
    strictPort: true,
    // Docker bind mounts on Windows can miss native file events; polling keeps CSS/TS updates hot.
    watch: {
      usePolling: true,
      interval: 100,
    },
    hmr: {
      host: 'localhost',
      port: 5173,
    },
  },
})
