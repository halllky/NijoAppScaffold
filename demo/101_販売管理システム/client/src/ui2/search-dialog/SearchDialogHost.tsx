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
  throw new Error('not implemented')
}

/**
 * ui2 フォルダ内部でのみ使用する。
 * ダイアログを SearchDialogHost の中に描画し、ダイアログが閉じたときに解決する Promise を返す。
 * renderDialog の引数 close を呼ぶとダイアログを取り除き、その引数で Promise を解決する。
 */
export function useOpenDialogInHost(): <TResult>(
  renderDialog: (close: (result: TResult) => void) => React.ReactNode,
) => Promise<TResult> {
  throw new Error('not implemented')
}

/**
 * ui2 フォルダ内部でのみ使用する。
 * 外部参照先（メタデータの refTo）に対応する検索ダイアログを、SearchDialogHost に渡された対応表から引く。
 * 対応表に無い場合は例外を投げる。
 */
export function useSearchDialogOf(refTo: string): SearchDialog<unknown, unknown> {
  throw new Error('not implemented')
}
