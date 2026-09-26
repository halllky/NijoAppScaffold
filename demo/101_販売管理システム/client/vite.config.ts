import { defineConfig } from "vite"
import { viteSingleFile } from "vite-plugin-singlefile"
import react from "@vitejs/plugin-react-swc"
import tailwindcss from "@tailwindcss/vite"

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    viteSingleFile(),
  ],
  build: {
    minify: false,
  },
  server: {
    port: 5173,
    strictPort: true,
    host: true,
    proxy: {
      // ここのポートは ASP.NET Core デバッグ設定と合わせる必要がある
      '/api': { target: 'http://localhost:5290' },
      '/swagger': { target: 'http://localhost:5290' },
    },
  }
})
