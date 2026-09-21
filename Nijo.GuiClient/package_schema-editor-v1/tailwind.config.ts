import type { Config } from 'tailwindcss'

export default {
  content: [
    // 依存先パッケージ（ui-components）のファイルも監視対象に含める
    "../package_ui-components/src/**/*.{js,ts,jsx,tsx}",
    // 依存先パッケージ（editable-grid）のビルド済みファイルも監視対象に含める
    "../../node_modules/@halllky/editable-grid/dist/**/*.js",
  ],
} satisfies Config
