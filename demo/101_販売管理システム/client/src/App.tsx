import React from "react"
import { Outlet, Link, Form, useNavigation } from "react-router-dom"
import * as Icon from "@heroicons/react/24/solid"
import { NowLoading } from "./ui"
import { useOptionalLoginUser } from "./login/loginUser"
import { useCanInvoke } from "./login/canInvoke"
import * as P000Module from "./pages/P000_トップページ"
import * as P002Module from "./pages/P002_ログアウト"
import { P001_ログイン } from "./pages/P001_ログイン"
import { navigationItems } from "./navigationItems"
import { APP_NAME } from "./appName"
import { APP_VERSION, APP_VERSION_SHORT } from "./appVersion"
import DebugMenuButton from "./debug-rooms/デバッグメニュー"

/**
 * アプリケーション全体の枠。
 * 画面全体の外枠のレイアウトを行う。
 *
 * 業務画面へのリンクとログアウトリンクはログイン後のみ表示する。
 * 業務画面へのリンクは、その画面を開くのに呼び出す処理をログインユーザーが呼び出せる場合だけ表示する。
 */
export default function App() {
  const navigation = useNavigation()
  const loginUser = useOptionalLoginUser()
  const canInvoke = useCanInvoke()

  return (
    <div className="flex flex-col h-full">

      {/* ルートナビゲーション */}
      <nav className="bg-gray-800 text-white px-8 py-2">
        <ul className="flex flex-wrap gap-x-8 items-center">
          <li className="shrink-0">
            <RootNavigationLink to={P000Module.URL} className="text-lg font-bold mr-4">
              {APP_NAME}
            </RootNavigationLink>
          </li>
          {loginUser && navigationItems.filter(item => canInvoke(item.operation)).map(item => (
            <li key={item.to} className="shrink-0">
              <RootNavigationLink to={item.to} icon={item.icon} badge={item.badge}>{item.label}</RootNavigationLink>
            </li>
          ))}

          <li className="flex-1"></li>

          {loginUser && (
            <li className="shrink-0">
              <Form method="post" action={P002Module.URL}>
                <button type="submit" className={`${ROOT_NAVIGATION_ITEM_CLASS} cursor-pointer`}>
                  <Icon.ArrowRightEndOnRectangleIcon className="w-5 h-5" />
                  ログアウト
                </button>
              </Form>
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

        {/* routes.tsx で指定した各画面はここに表示される。
            ログインが必要な画面をログインせずに開こうとした場合は、代わりにログイン画面が表示される */}
        <P001_ログイン>
          <Outlet />
        </P001_ログイン>
      </div>

      {/* 問い合わせや障害のときに、どのバージョンの画面で起きたかを確かめられるよう、画面の隅にバージョンを表示する */}
      <AppVersion />
    </div>
  )
}

/**
 * 画面の隅のバージョン。普段は短い形で表示し、クリックすると完全な形（コミットの識別子の全体）に切り替える。
 * 問い合わせのときに完全な形を伝えられるよう、文字は選択してコピーできる。
 */
function AppVersion() {
  const [showFull, setShowFull] = React.useState(false)
  return (
    <div
      className="fixed bottom-0 right-1 text-[10px] text-gray-400 select-text cursor-pointer"
      title={APP_VERSION}
      onClick={() => setShowFull(value => !value)}
    >
      {showFull ? APP_VERSION : APP_VERSION_SHORT}
    </div>
  )
}

/** ルートナビゲーションリンク */
function RootNavigationLink({ to, children, className, icon, badge }: {
  to: string
  children: React.ReactNode
  className?: string
  icon?: React.ElementType
  badge?: React.ComponentType
}) {
  const IconComponent = icon
  const BadgeComponent = badge
  return (
    <Link to={to} className={`${ROOT_NAVIGATION_ITEM_CLASS} ${className ?? ''}`}>
      {IconComponent && <IconComponent className="w-5 h-5" />}
      {children}
      {BadgeComponent && <BadgeComponent />}
    </Link>
  )
}

/** ルートナビゲーションの項目（リンクとボタン）の見た目 */
const ROOT_NAVIGATION_ITEM_CLASS = "hover:text-gray-300 select-none flex items-center gap-1"
