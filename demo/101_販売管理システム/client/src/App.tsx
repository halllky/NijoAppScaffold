import React from "react"
import { Outlet, Link, useNavigation } from "react-router-dom"
import * as Icon from "@heroicons/react/24/solid"
import { NowLoading } from "./ui"
import { LoginUserProvider, useLoginLogout } from "./useLoginLogout"
import * as P000Module from "./pages/P000_トップページ"
import * as P002Module from "./pages/P002_ログアウト"
import { navigationItems } from "./navigationItems"
import DebugMenuButton from "./debug-rooms/デバッグメニュー"

/**
 * アプリケーションの一番外枠。
 * アプリケーション全体で用いる React コンテキストのプロバイダー配置を行う。
 */
export default function App() {
  return (
    <LoginUserProvider>
      <RootLayout />
    </LoginUserProvider>
  )
}

/**
 * アプリケーション全体の枠。
 * 画面全体の外枠のレイアウトを行う。
 *
 * 業務画面へのリンクとログアウトリンクはログイン後のみ表示する。
 * どの業務画面をナビゲーションに表示するかは呼び出し元が決める。
 */
function RootLayout() {
  const navigation = useNavigation()
  const { loginUser } = useLoginLogout()

  return (
    <div className="flex flex-col h-full">

      {/* ルートナビゲーション */}
      <nav className="bg-gray-800 text-white px-8 py-2">
        <ul className="flex flex-wrap gap-x-8 items-center">
          <li className="shrink-0">
            <RootNavigationLink to={P000Module.URL} className="text-lg font-bold mr-4">
              販売管理システム
            </RootNavigationLink>
          </li>
          {loginUser && navigationItems.map(item => (
            <li key={item.to} className="shrink-0">
              <RootNavigationLink to={item.to} icon={item.icon}>{item.label}</RootNavigationLink>
            </li>
          ))}

          <li className="flex-1"></li>

          {loginUser && (
            <li className="shrink-0">
              <RootNavigationLink to={P002Module.URL} icon={Icon.ArrowRightEndOnRectangleIcon}>
                ログアウト
              </RootNavigationLink>
            </li>
          )}

          {/* 開発環境でのみデバッグメニューを表示 */}
          {import.meta.env.DEV && (
            <DebugMenuButton />
          )}
        </ul>
      </nav>

      {/* 各画面のloader実行中でもルートナビゲーションが使えるようにするため relative の位置はここ */}
      <div className="flex-1 overflow-auto relative">

        {/* 各画面の loader 実行中に表示するローディングオーバーレイ */}
        {navigation.state === "loading" && (
          <NowLoading />
        )}

        {/* routes.tsx で指定した各画面はここに表示される */}
        <Outlet />
      </div>
    </div>
  )
}

/** ルートナビゲーションリンク */
function RootNavigationLink({ to, children, className, icon }: {
  to: string
  children: React.ReactNode
  className?: string
  icon?: React.ElementType
}) {
  const IconComponent = icon
  return (
    <Link to={to} className={`hover:text-gray-300 select-none flex items-center gap-1 ${className ?? ''}`}>
      {IconComponent && <IconComponent className="w-5 h-5" />}
      {children}
    </Link>
  )
}
