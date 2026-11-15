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
    // 開発中も本番と同じ振る舞いにするため、サーバーがすべての応答に付けるセキュリティのヘッダーと同じものを付ける。
    // 変えるときは、サーバーの側も同じに変える
    headers: {
      'X-Frame-Options': 'SAMEORIGIN',
      'Content-Security-Policy': "frame-ancestors 'self'",
      'X-Content-Type-Options': 'nosniff',
      'Referrer-Policy': 'same-origin',
    },
    proxy: {
      // ここのポートは ASP.NET Core デバッグ設定と合わせる必要がある
      '/api': { target: 'http://localhost:5290' },
      '/swagger': { target: 'http://localhost:5290' },
    },
  }
})
