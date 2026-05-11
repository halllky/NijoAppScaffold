import React from "react"
import { Outlet, Link, useNavigation } from "react-router-dom"
import * as Icon from "@heroicons/react/24/solid"
import * as P000 from "../pages/P000_トップページ"
import * as P002 from "../pages/P002_ログアウト"
import * as P100 from "../pages/P100_売上"
import * as P200 from "../pages/P200_入荷"
import * as P300 from "../pages/P300_商品"
import * as P400 from "../pages/P400_従業員"
import { NowLoading } from "./NowLoading"

/**
 * ログイン後のアプリケーション全体の枠
 */
export function RootLayout() {
  const navigation = useNavigation()

  return (
    <div className="flex flex-col h-full">

      {/* ルートナビゲーション */}
      <nav className="bg-gray-800 text-white px-8 py-2">
        <ul className="flex flex-wrap gap-x-8 items-center">
          <li className="shrink-0">
            <RootNavigationLink to={P000.URL} className="text-lg font-bold mr-4">販売管理システム</RootNavigationLink>
          </li>
          <li className="shrink-0">
            <RootNavigationLink to={P100.URL} icon={Icon.CurrencyYenIcon}>売上</RootNavigationLink>
          </li>
          <li className="shrink-0">
            <RootNavigationLink to={P200.URL} icon={Icon.TruckIcon}>入荷</RootNavigationLink>
          </li>
          <li className="shrink-0">
            <RootNavigationLink to={P300.URL} icon={Icon.CubeIcon}>商品</RootNavigationLink>
          </li>
          <li className="shrink-0">
            <RootNavigationLink to={P400.URL} icon={Icon.UserGroupIcon}>従業員</RootNavigationLink>
          </li>

          <li className="flex-1"></li>

          <li className="shrink-0">
            <RootNavigationLink to={P002.URL} icon={Icon.ArrowRightEndOnRectangleIcon}>ログアウト</RootNavigationLink>
          </li>
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
