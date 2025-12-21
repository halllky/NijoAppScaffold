import { ReactNode, useCallback, useState } from "react"
import * as Icon from "@heroicons/react/24/solid"
import { Allotment, LayoutPriority } from "allotment"
import { PageBase } from "./PageBase"
import { PageTitle } from "./PageTitle"
import { Button } from "../input/Button"

export type SearchPageBaseProps<TItem, TCondition> = {
  /** ページタイトル。ブラウザのタイトルバーに表示されます。 */
  pageTitle?: string
  /** 検索条件欄のデフォルトサイズ（ピクセル） */
  searchConditionDefaultSize?: number
  /** 検索処理。検索条件とページング情報を受け取り、結果を返す */
  onSearch: (condition: TCondition, pageIndex: number, pageSize: number) => Promise<{ items: TItem[], totalCount: number }>
  /** ヘッダー部分のページタイトルの横の部分のレイアウト */
  header?: ReactNode
  /**
   * 検索条件エリア
   * ※ 注意: 渡されるコンポーネント内で `<form id="search-form">` を定義し、その onSubmit で `props.search` を呼び出してください。
   */
  searchCondition: (props: { search: (condition: TCondition) => void }) => ReactNode
  /** 検索結果エリア */
  searchResult: (props: { items: TItem[] }) => ReactNode
}

/**
 * 一覧検索画面のベースコンポーネント。
 *
 * ### このコンポーネントの責務
 * - ページの枠のレイアウト（検索条件、結果、ページャ）
 * - 検索処理の呼び出しと結果の保持
 * - ページング管理
 *
 * ### 利用側の責務
 * - 検索条件フォームのレイアウトと状態管理
 * - 検索結果一覧のレイアウト
 * - 実際のAPI呼び出し
 * - **重要**: 検索条件フォームは `<form id="search-form">` でラップすること（ヘッダーのボタンと連携するため）
 */
export function SearchPageBase<TItem, TCondition>(props: SearchPageBaseProps<TItem, TCondition>) {
  const [items, setItems] = useState<TItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [pageIndex, setPageIndex] = useState(0)
  const [pageSize, setPageSize] = useState(20)
  const [lastCondition, setLastCondition] = useState<TCondition | undefined>(undefined)
  const [loading, setLoading] = useState(false)
  const [searchConditionVisible, setSearchConditionVisible] = useState(true)

  const executeSearch = useCallback(async (condition: TCondition, page: number, size: number) => {
    try {
      setLoading(true)
      const result = await props.onSearch(condition, page, size)
      setItems(result.items)
      setTotalCount(result.totalCount)
      setPageIndex(page)
      setPageSize(size)
      setLastCondition(condition)
    } finally {
      setLoading(false)
    }
  }, [props.onSearch])

  const handleSearch = useCallback((condition: TCondition) => {
    // 新しい条件で検索するときは1ページ目に戻る
    executeSearch(condition, 0, pageSize)
  }, [executeSearch, pageSize])

  const handlePageChange = useCallback((newPage: number) => {
    if (lastCondition) {
      executeSearch(lastCondition, newPage, pageSize)
    }
  }, [executeSearch, lastCondition, pageSize])

  // ページャーの計算
  const totalPages = Math.ceil(totalCount / pageSize)
  const canPrev = pageIndex > 0
  const canNext = pageIndex < totalPages - 1

  return (
    <PageBase
      browserTitle={props.pageTitle}
      className="overflow-hidden"
      header={(
        <>
          {/* ページタイトル */}
          <button
            type="button"
            className="flex-1 flex items-center gap-1 cursor-pointer select-none"
            onClick={() => setSearchConditionVisible(prev => !prev)}
          >
            {searchConditionVisible ? (
              <Icon.ChevronDownIcon className="w-4 h-4" />
            ) : (
              <Icon.ChevronRightIcon className="w-4 h-4" />
            )}
            <PageTitle>
              {props.pageTitle}
            </PageTitle>
          </button>

          {/* クリアボタン */}
          <Button outline
            onClick={() => {
              // form="search-form" を持つ reset ボタンとして振る舞わせる、
              // または利用側が reset イベントをハンドリングすることを期待する
              // ここでは単純に form の reset をトリガーするために type="reset" のボタンを配置する
              // ただし onClick で何かする必要がある場合はここに追加
              const form = document.getElementById('search-form') as HTMLFormElement
              form?.reset()
            }}
          >
            クリア
          </Button>

          {/* 検索ボタン */}
          <Button submit fill form="search-form" disabled={loading}>
            検索
          </Button>
        </>
      )}
      contents={(
        <Allotment vertical
          defaultSizes={[props.searchConditionDefaultSize ?? 200, 999]}
          proportionalLayout={false} // ページサイズ変更時に検索結果欄だけ伸縮するようにするため
          className="pb-1"
        >

          {/* 検索条件欄 */}
          <Allotment.Pane minSize={50} visible={searchConditionVisible}>
            <div className="h-full overflow-auto bg-white">
              {props.searchCondition({ search: handleSearch })}
            </div>
          </Allotment.Pane>

          {/* 検索結果欄 */}
          <Allotment.Pane
            priority={LayoutPriority.High} // ページサイズ変更時に検索結果欄だけ伸縮する
          >
            <div className="h-full overflow-auto p-1 border bg-white">
              {props.searchResult({ items })}
            </div>
          </Allotment.Pane>

        </Allotment>
      )}
      footer={(
        <>
          {/* 件数表示 */}
          <div className="text-sm text-gray-600">
            {totalCount > 0 ? (
              <>
                全 {totalCount} 件中 {pageIndex * pageSize + 1} - {Math.min((pageIndex + 1) * pageSize, totalCount)} 件を表示
              </>
            ) : (
              <>データなし</>
            )}
          </div>

          {/* ページャー */}
          <div className="flex gap-2 items-center">
            <button
              className="px-3 py-1 border rounded hover:bg-gray-100 disabled:opacity-50"
              onClick={() => handlePageChange(pageIndex - 1)}
              disabled={!canPrev || loading}
            >
              前へ
            </button>
            <span className="px-2 py-1">
              {pageIndex + 1} / {Math.max(1, totalPages)}
            </span>
            <button
              className="px-3 py-1 border rounded hover:bg-gray-100 disabled:opacity-50"
              onClick={() => handlePageChange(pageIndex + 1)}
              disabled={!canNext || loading}
            >
              次へ
            </button>
          </div>
        </>
      )}
    />
  )
}
