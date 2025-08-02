import { defineConfig } from 'vitepress'
import { withMermaid } from 'vitepress-plugin-mermaid'

// https://vitepress.dev/reference/site-config
export default withMermaid({
  title: "Nijo Application Builder",
  description: "スキーマ駆動型アプリケーション生成フレームワーク",

  // GitHub Pages用の設定
  base: '/NijoApplicationBuilder/',
  outDir: '../../docs',

  // 静的ファイルの設定
  cleanUrls: true,

  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'ホーム', link: '/' },
      { text: 'チュートリアル', link: '/tutorials/' },
      { text: 'ハウツーガイド', link: '/how-to-guides/' },
      { text: 'リファレンス', link: '/reference/' },
      { text: '設計思想', link: '/explanation/' }
    ],

    sidebar: [
      {
        text: '📚 Tutorials',
        collapsed: false,
        items: [
          { text: 'チュートリアル概要', link: '/tutorials/' },
          { text: '5分で作る住所録アプリ', link: '/tutorials/getting-started' }
        ]
      },
      {
        text: '🛠️ How-to Guides',
        collapsed: false,
        items: [
          { text: 'ハウツーガイド概要', link: '/how-to-guides/' },
          { text: 'プロジェクト適用判断', link: '/how-to-guides/project-evaluation' }
        ]
      },
      {
        text: '📖 Reference',
        collapsed: false,
        items: [
          { text: 'リファレンス概要', link: '/reference/' },
          { text: 'モデル技術仕様', link: '/reference/models-specification' }
        ]
      },
      {
        text: '💡 Explanation',
        collapsed: false,
        items: [
          { text: '設計思想概要', link: '/explanation/' },
          { text: 'モデル間の協調による アプリケーション構成', link: '/explanation/models-overview' },
          { text: 'モデル設計思想と開発手法比較', link: '/explanation/model-design-philosophy' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/example/nijo' }
    ],

    footer: {
      copyright: 'Copyright © 2025 Nijo Application Builder'
    },

    search: {
      provider: 'local'
    }
  }
})
