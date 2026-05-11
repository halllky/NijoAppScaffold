import type { Config } from 'tailwindcss'

export default {
  content: [
    // 依存先パッケージ（ui-components）のファイルも監視対象に含める
    "../package_ui-components/src/**/*.{js,ts,jsx,tsx}",
  ],
} satisfies Config