import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@nijo/ui-components': path.resolve(__dirname, '../../ui-components/src'),
    },
  },
  build: {
    minify: false,
  },
  server: {
    port: 5173,
    strictPort: true,
  }
})
