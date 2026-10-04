import React from "react"
import { Link } from "react-router-dom"

/** パンくずリストの、現在のページより上位のページ */
export type BreadcrumbItem = {
  /** 表示名 */
  label: string
  /** 遷移先の URL */
  to: string
}

/**
 * パンくずリストのうち、現在のページより上位のページへのリンクの並び。
 * 各リンクの後ろに区切りを付けるので、この直後に現在のページのタイトルを置くこと。
 * 現在のページのタイトルはこのコンポーネントでは表示しない。
 */
export function Breadcrumb({ items }: {
  /** 上位のページ。最上位から順に並べる */
  items: BreadcrumbItem[]
}): React.ReactNode {
  return (
    <nav className="flex items-center gap-2 text-lg select-none">
      {items.map(item => (
        <React.Fragment key={item.to}>
          {/* 上位のページへのリンク */}
          <Link to={item.to} className="text-sky-600 font-bold underline hover:underline">
            {item.label}
          </Link>
          {/* 区切り */}
          <span className="text-gray-400">&gt;</span>
        </React.Fragment>
      ))}
    </nav>
  )
}
