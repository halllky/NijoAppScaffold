import react from '@vitejs/plugin-react-swc'
import { defineConfig } from 'vite'
import { viteSingleFile } from 'vite-plugin-singlefile'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
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
    // クライアント側のデバッグサーバーがどのポートで起動するか
    port: 5176,
    strictPort: true,

    // クライアント側からの /api に対するHTTPリクエストがこの target に送信されるようにする。
    // デバッグ実行の際だけ有効。
    // 予めバックエンド（C#, ASP.NET Core）の Web サーバーをこのポートで起動させておく必要がある。
    proxy: {
      '/api': { target: 'http://localhost:5290' },
    },
  },
})
