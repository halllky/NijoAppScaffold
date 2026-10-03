export type SearchDialogHostProps = {
  children?: React.ReactNode
}

/**
 * 検索ダイアログの描画先。
 * 検索ダイアログを開く画面よりも外側に1つだけ置く。通常はアプリケーションのルートに置く。
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
