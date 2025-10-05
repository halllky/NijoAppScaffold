import { defineConfig } from 'vite'
import { viteSingleFile } from 'vite-plugin-singlefile'
import react from '@vitejs/plugin-react-swc'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    viteSingleFile(),
  ],
  resolve: {
    alias: {
      '@nijo/ui-components': path.resolve(__dirname, '../package_ui-components/src'),
    },
  },
  build: {
    minify: false,
  },
  server: {
    port: 5176,
    strictPort: true,
  }
})
