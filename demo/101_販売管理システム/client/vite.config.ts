import { execSync } from "node:child_process"
import { readFileSync } from "node:fs"
import { defineConfig } from "vite"
import { viteSingleFile } from "vite-plugin-singlefile"
import react from "@vitejs/plugin-react-swc"
import tailwindcss from "@tailwindcss/vite"

export default defineConfig({
  define: {
    // 画面のバージョン。サーバーと同じ「バージョン番号+コミットの識別子」の形にする
    __APP_VERSION__: JSON.stringify(readAppVersion()),
  },
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

/**
 * プロジェクトのフォルダの直下の VERSION ファイルに書いたバージョン番号に、git のコミットの識別子を付けて返す。
 * git の管理の外でビルドしたときは、バージョン番号だけを返す。
 */
function readAppVersion(): string {
  const version = readFileSync(new URL("../VERSION", import.meta.url), "utf-8").trim()
  try {
    const commit = execSync("git rev-parse HEAD", { stdio: ["ignore", "pipe", "ignore"] }).toString().trim()
    return `${version}+${commit}`
  } catch {
    return version
  }
}
