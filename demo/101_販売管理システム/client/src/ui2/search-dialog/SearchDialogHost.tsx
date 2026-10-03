import React from "react"
import type * as RHF from "react-hook-form"
import type { SearchDialog } from "./defineSearchDialog"
import type { SearchDialogRegistry } from "./SearchDialogRegistry"

export type SearchDialogHostProps = {
  /**
   * 外部参照先と検索ダイアログの対応表。SearchDialogRegistry に型を登録したものと同じ対応表を渡す。
   * SearchDialogHost を入れ子にした場合、内側の対応表は外側の対応表に足される。同じキーは内側が優先される。
   */
  dialogs: Partial<SearchDialogRegistry>
  children?: React.ReactNode
}

/**
 * 検索ダイアログの描画先と、外部参照先と検索ダイアログの対応表の提供元。
 * 検索ダイアログを開く画面よりも外側に置く。通常はアプリケーションのルートに置く。
 * 検索ダイアログの中から別の検索ダイアログを開いた場合は、後から開いたものが手前に重なる。
 */
export function SearchDialogHost(props: SearchDialogHostProps): React.ReactNode {
  const outer = React.useContext(SearchDialogHostContext)

  // 開いているダイアログ。後ろのものほど手前に重なる
  const [openDialogs, setOpenDialogs] = React.useState<{ id: number, node: React.ReactNode }[]>([])
  const nextIdRef = React.useRef(0)

  const openDialog: OpenDialogInHost = React.useCallback(<TResult,>(
    renderDialog: (close: (result: TResult) => void) => React.ReactNode,
  ) => new Promise<TResult>(resolve => {
    const id = nextIdRef.current++
    let isClosed = false
    const close = (result: TResult) => {
      // 閉じる操作が重なっても、Promise を解決するのは最初の1回だけ
      if (isClosed) return
      isClosed = true
      setOpenDialogs(prev => prev.filter(dialog => dialog.id !== id))
      resolve(result)
    }
    setOpenDialogs(prev => [...prev, { id, node: renderDialog(close) }])
  }), [])

  // 入れ子の場合は外側の対応表に内側の対応表を足す
  const outerDialogs = outer?.dialogs
  const dialogs = React.useMemo(() => ({ ...outerDialogs, ...props.dialogs }), [outerDialogs, props.dialogs])
  const contextValue = React.useMemo(() => ({ dialogs, openDialog }), [dialogs, openDialog])

  return (
    <SearchDialogHostContext.Provider value={contextValue}>
      {props.children}

      {/* 開いているダイアログ。ダイアログの中から別のダイアログを開けるよう、コンテキストの内側に描画する */}
      {openDialogs.map(dialog => (
        <React.Fragment key={dialog.id}>
          {dialog.node}
        </React.Fragment>
      ))}
    </SearchDialogHostContext.Provider>
  )
}

/**
 * ui2 フォルダ内部でのみ使用する。
 * ダイアログを SearchDialogHost の中に描画し、ダイアログが閉じたときに解決する Promise を返す関数。
 * renderDialog の引数 close を呼ぶとダイアログを取り除き、その引数で Promise を解決する。
 */
export type OpenDialogInHost = <TResult>(
  renderDialog: (close: (result: TResult) => void) => React.ReactNode,
) => Promise<TResult>

/**
 * ui2 フォルダ内部でのみ使用する。
 * ダイアログを SearchDialogHost の中に描画する関数を返す。
 * SearchDialogHost の外で使った場合、返された関数を呼んだときに例外を投げる。
 */
export function useOpenDialogInHost(): OpenDialogInHost {
  const host = React.useContext(SearchDialogHostContext)
  return host?.openDialog ?? throwHostNotFound
}

/**
 * ui2 フォルダ内部でのみ使用する。
 * 外部参照先（メタデータの refTo）に対応する検索ダイアログを、SearchDialogHost に渡された対応表から引く関数を返す。
 * 対応表に無い場合は、返された関数が例外を投げる。
 */
export function useFindSearchDialog(): (refTo: string) => SearchDialog<RHF.FieldValues, unknown> {
  const host = React.useContext(SearchDialogHostContext)
  const dialogs = host?.dialogs
  return React.useCallback((refTo: string) => {
    if (!dialogs) throw new Error('外部参照の入力欄は SearchDialogHost の中に置いてください。')
    const dialog = (dialogs as Record<string, SearchDialog<RHF.FieldValues, unknown> | undefined>)[refTo]
    if (!dialog) throw new Error(`${refTo} の検索ダイアログが SearchDialogHost の対応表にありません。`)
    return dialog
  }, [dialogs])
}

const SearchDialogHostContext = React.createContext<{
  dialogs: Partial<SearchDialogRegistry>
  openDialog: OpenDialogInHost
} | undefined>(undefined)

const throwHostNotFound: OpenDialogInHost = () => {
  throw new Error('検索ダイアログを開くには SearchDialogHost の中に置いてください。')
}
