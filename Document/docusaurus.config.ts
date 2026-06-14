import { themes as prismThemes } from 'prism-react-renderer';
import type { Config } from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

const config: Config = {
  title: 'Nijo App Scaffold',
  favicon: 'img/icon.png',

  // Future flags, see https://docusaurus.io/docs/api/docusaurus-config#future
  future: {
    v4: true, // Improve compatibility with the upcoming Docusaurus v4
  },

  // Set the production url of your site here
  url: 'https://halllky.github.io',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/NijoAppScaffold',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'halllky', // Usually your GitHub org/user name.
  projectName: 'Nijo App Scaffold', // Usually your repo name.

  onBrokenLinks: 'throw',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'ja',
    locales: ['ja'],
  },

  // フォントに Noto Sans JP を使用
  headTags: [
    {
      tagName: 'link',
      attributes: {
        rel: 'preconnect',
        href: 'https://fonts.googleapis.com',
      },
    },
    {
      tagName: 'link',
      attributes: {
        rel: 'preconnect',
        href: 'https://fonts.gstatic.com',
        crossorigin: 'true',
      },
    },
    {
      tagName: 'link',
      attributes: {
        rel: 'stylesheet',
        href: 'https://fonts.googleapis.com/css2?family=Noto+Sans+JP:wght@100..900&display=swap',
      },
    },
  ],

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          sidebarCollapsed: false,
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          // editUrl: 'https://github.com/facebook/docusaurus/tree/main/packages/create-docusaurus/templates/shared/',
        },
        blog: {
          showReadingTime: true,
          feedOptions: {
            type: ['rss', 'atom'],
            xslt: true,
          },
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          // editUrl: 'https://github.com/facebook/docusaurus/tree/main/packages/create-docusaurus/templates/shared/',

          // Useful options to enforce blogging best practices
          onInlineTags: 'warn',
          onInlineAuthors: 'warn',
          onUntruncatedBlogPosts: 'warn',
        },
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    // Replace with your project's social card
    image: 'img/icon.png',
    colorMode: {
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'Nijo App Scaffold',
      logo: {
        alt: 'Nijo App Scaffold Logo',
        src: 'img/icon.png',
      },
      items: [
        {
          to: 'docs/intro',
          position: 'left',
          label: 'ドキュメント',
        },
        {
          to: 'docs/demo',
          position: 'left',
          label: 'デモ',
        },
        {
          to: '/reference/CLI',
          label: 'コマンドラインAPI',
          position: 'left',
        },
        {
          href: 'https://github.com/halllky/NijoAppScaffold',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'ドキュメント',
          items: [
            { label: 'はじめに', to: '/docs/intro' },
            { label: 'デモ', to: '/docs/demo' },
            { label: '設計・開発', to: '/docs/workflow/' },
            { label: 'アーキテクチャ概要', to: '/docs/architecture' },
          ],
        },
        {
          title: 'リファレンス',
          items: [
            { label: 'コマンドライン API', to: '/reference/CLI' },
            { label: 'nijo.xml 属性種類定義', to: '/reference/ValueMemberTypes' },
          ],
        },
        {
          title: 'リンク',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/halllky/NijoAppScaffold',
            },
          ],
        },
      ],
      copyright: `Copyright © 2023 halky. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
