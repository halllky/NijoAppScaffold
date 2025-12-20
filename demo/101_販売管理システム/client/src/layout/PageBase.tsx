import { ReactNode, useEffect } from "react"

export type PageBaseProps = {
  /** ページタイトル。ブラウザのタイトルバーに表示されます。 */
  browserTitle?: string
  /** ヘッダー */
  header?: ReactNode
  /** コンテンツ */
  contents?: ReactNode
  /** フッター */
  footer?: ReactNode
  /** コンテンツエリアの追加クラス */
  className?: string
}

/**
 * ページのベースコンポーネント。
 * ヘッダー、コンテンツ、フッターのレイアウトと、ページタイトルの設定を行います。
 */
export function PageBase(props: PageBaseProps) {
  useEffect(() => {
    if (props.browserTitle) {
      document.title = props.browserTitle
    }
  }, [props.browserTitle])

  return (
    <div className="flex flex-col h-full">

      {/* ヘッダ */}
      {props.header && (
        <header className="flex flex-wrap items-center px-8 py-1 gap-4">
          {props.header}
        </header>
      )}

      {/* コンテンツ */}
      <div className={`flex-grow flex flex-col px-8 ${props.className ?? ''}`}>
        {props.contents}
      </div>

      {/* フッター */}
      {props.footer && (
        <footer className="px-8 py-1 border-t border-gray-300 flex items-center justify-between bg-gray-50">
          {props.footer}
        </footer>
      )}
    </div>
  )
}
