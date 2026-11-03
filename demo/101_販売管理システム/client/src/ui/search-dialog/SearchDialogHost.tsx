import React from "react"
import type * as RHF from "react-hook-form"
import type { FindByCode, SearchDialog } from "./defineSearchDialog"
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

  // 対応表の各検索ダイアログの、コードを照合する関数。
  // 照合はグリッドの列のようにフックを呼べない場所からも行うので、ここで検索ダイアログごとにフックを呼んで解決しておく
  const findByCodesRef = React.useRef(new Map<string, FindByCode<RHF.FieldValues, unknown>>())
  const registerFindByCode = React.useCallback((refTo: string, findByCode: FindByCode<RHF.FieldValues, unknown>) => {
    findByCodesRef.current.set(refTo, findByCode)
  }, [])
  const findByCode = React.useCallback((refTo: string): FindByCode<RHF.FieldValues, unknown> => (code, params, signal) => {
    const resolved = findByCodesRef.current.get(refTo)
    if (!resolved) throw new Error(`${refTo} の検索ダイアログのコードの照合が準備できていません。`)
    return resolved(code, params, signal)
  }, [])

  const contextValue = React.useMemo(() => ({ dialogs, openDialog, findByCode }), [dialogs, openDialog, findByCode])

  return (
    <SearchDialogHostContext.Provider value={contextValue}>
      {Object.entries(dialogs).map(([refTo, dialog]) => dialog && (
        <FindByCodeResolver
          key={refTo}
          refTo={refTo}
          dialog={dialog as SearchDialog<RHF.FieldValues, unknown>}
          register={registerFindByCode}
        />
      ))}

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
 * ui フォルダ内部でのみ使用する。
 * ダイアログを SearchDialogHost の中に描画し、ダイアログが閉じたときに解決する Promise を返す関数。
 * renderDialog の引数 close を呼ぶとダイアログを取り除き、その引数で Promise を解決する。
 */
export type OpenDialogInHost = <TResult>(
  renderDialog: (close: (result: TResult) => void) => React.ReactNode,
) => Promise<TResult>

/**
 * ui フォルダ内部でのみ使用する。
 * ダイアログを SearchDialogHost の中に描画する関数を返す。
 * SearchDialogHost の外で使った場合、返された関数を呼んだときに例外を投げる。
 */
export function useOpenDialogInHost(): OpenDialogInHost {
  const host = React.useContext(SearchDialogHostContext)
  return host?.openDialog ?? throwHostNotFound
}

/**
 * ui フォルダ内部でのみ使用する。
 * SearchDialogHost の対応表から引いた検索ダイアログ。コードの照合は、フックを呼ばずに使えるよう解決済みのものを持つ。
 */
export type SearchDialogInHost = SearchDialog<RHF.FieldValues, unknown> & {
  /** 手入力されたコードを照合する */
  findByCode: FindByCode<RHF.FieldValues, unknown>
}

/**
 * ui フォルダ内部でのみ使用する。
 * 外部参照先（メタデータの refTo）に対応する検索ダイアログを、SearchDialogHost に渡された対応表から引く関数を返す。
 * 対応表に無い場合は、返された関数が例外を投げる。
 */
export function useFindSearchDialog(): (refTo: string) => SearchDialogInHost {
  const host = React.useContext(SearchDialogHostContext)
  return React.useCallback((refTo: string) => {
    if (!host) throw new Error('外部参照の入力欄は SearchDialogHost の中に置いてください。')
    const dialog = (host.dialogs as Record<string, SearchDialog<RHF.FieldValues, unknown> | undefined>)[refTo]
    if (!dialog) throw new Error(`${refTo} の検索ダイアログが SearchDialogHost の対応表にありません。`)
    return { ...dialog, findByCode: host.findByCode(refTo) }
  }, [host])
}

/** 検索ダイアログのコードを照合する関数をフックで作り、SearchDialogHost に登録する。何も描画しない */
function FindByCodeResolver({ refTo, dialog, register }: {
  refTo: string
  dialog: SearchDialog<RHF.FieldValues, unknown>
  register: (refTo: string, findByCode: FindByCode<RHF.FieldValues, unknown>) => void
}): React.ReactNode {
  const findByCode = dialog.refField.useFindByCode()
  React.useLayoutEffect(() => register(refTo, findByCode), [register, refTo, findByCode])
  return null
}

const SearchDialogHostContext = React.createContext<{
  dialogs: Partial<SearchDialogRegistry>
  openDialog: OpenDialogInHost
  findByCode: (refTo: string) => FindByCode<RHF.FieldValues, unknown>
} | undefined>(undefined)

const throwHostNotFound: OpenDialogInHost = () => {
  throw new Error('検索ダイアログを開くには SearchDialogHost の中に置いてください。')
}
