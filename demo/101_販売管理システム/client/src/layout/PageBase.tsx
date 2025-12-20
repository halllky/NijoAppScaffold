import { ReactNode, useEffect } from "react"

export type PageBaseProps = {
  /** ページタイトル。ブラウザのタイトルバーに表示されます。 */
  pageTitle?: string
  /** ヘッダーのタイトルの横に表示するコンテンツ */
  header?: ReactNode
  /** ヘッダーの右側に表示するコンテンツ */
  headerRight?: ReactNode
  /** コンテンツ */
  children?: ReactNode
  /** フッター */
  footer?: ReactNode
  /** ルート要素の追加クラス */
  className?: string
  /** コンテンツエリアの追加クラス */
  contentClassName?: string
}

/**
 * ページのベースコンポーネント。
 * ヘッダー、コンテンツ、フッターのレイアウトと、ページタイトルの設定を行います。
 */
export function PageBase(props: PageBaseProps) {
  useEffect(() => {
    if (props.pageTitle) {
      document.title = props.pageTitle
    }
  }, [props.pageTitle])

  return (
    <div className={`flex flex-col gap-2 ${props.className ?? ''}`}>

      {/* ヘッダ */}
      <header className="flex flex-wrap items-center px-8 py-1 gap-4">
        <h1 className="text-xl font-bold">
          {props.pageTitle}
        </h1>
        {props.header}

        {props.headerRight && (
          <>
            <div className="flex-grow" />
            <div className="flex gap-2">
              {props.headerRight}
            </div>
          </>
        )}
      </header>

      {/* コンテンツ */}
      <div className={`flex-grow flex flex-col ${props.contentClassName ?? 'px-8 py-2'}`}>
        {props.children}
      </div>

      {/* フッター */}
      {props.footer && (
        <footer className="px-8 py-2 border-t flex items-center justify-between bg-gray-50">
          {props.footer}
        </footer>
      )}
    </div>
  )
}
