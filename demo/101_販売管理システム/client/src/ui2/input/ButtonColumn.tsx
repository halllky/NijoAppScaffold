import * as EG2 from "@halllky/editable-grid"

/**
 * ボタンの列のオプション。
 * セルの描画はヘルパーが決めるので、ここでは指定できない。
 */
export type ButtonColumnOptions<TRow> = Omit<
  Partial<EG2.EditableGridLeafColumn<TRow>>,
  'columnId' | 'renderHeader' | 'renderBody' | 'getValuesForRender' | 'editor' | 'cellToText' | 'textToCell' | 'wrap'
> & {
  /** 列見出し。未指定の場合は見出しを表示しない */
  header?: string
  /** 列ID。未指定の場合はボタンの文言から決まる。同じ文言のボタンの列を2つ以上並べる場合のみ指定する */
  columnId?: string
  /**
   * セルが読み取り専用のときもボタンを表示するなら true。
   * 未指定の場合、読み取り専用のセル（グリッド全体・行・セルのいずれの指定によるものも含む）ではボタンを表示しない。
   * 行の削除のように値を変える操作は未指定のまま、行の選択のように値を変えない操作は true にする。
   */
  showWhenReadOnly?: boolean
}

/**
 * 各行にボタンを1つ置く列。
 * onClick には、クリックした時点の行の最新の値と、行キーが渡される。
 * onClick は列定義を作ったときのものが呼ばれる。
 * Enter か Space でもボタン起動。
 */
export function buttonColumn<TRow>(
  text: string,
  onClick: (row: TRow, rowKey: string) => void,
  options?: ButtonColumnOptions<TRow>,
): EG2.EditableGridLeafColumn<TRow> {
  const { header, columnId, showWhenReadOnly, onCellKeyDown, ...rest } = options ?? {}

  // 描画中のボタン。
  // キー操作の時点ではグリッド全体の読み取り専用を知る手段が無いので、ボタンが表示されているかどうかは描画結果で判断する。
  // onCellKeyDown が isReadOnly を渡してくれるようになればこれをしなくてもよい
  const renderedButtons = new Map<string, HTMLButtonElement>()

  return {
    defaultWidth: 64,
    disableResizing: true,
    ...rest,
    columnId: columnId ?? `button:${text}`,
    renderHeader: () => header === undefined ? null : (
      <div className="px-1 text-sm truncate select-none" title={header}>
        {header}
      </div>
    ),
    // 他の列のセルとそろえ、読み取り専用でないセルは背景を白にする
    renderBody: ({ getRow, rowKey, isReadOnly }) => (
      <div className={`h-full w-full flex items-stretch p-px ${isReadOnly ? '' : 'bg-white'}`}>
        {(showWhenReadOnly || !isReadOnly) && (
          <button
            type="button"
            // ボタンのクリックでセルが選ばれないよう、mousedown の伝播を止める
            onMouseDown={e => e.stopPropagation()}
            onClick={() => onClick(getRow(), rowKey)}
            ref={button => {
              if (!button) return
              renderedButtons.set(rowKey, button)
              return () => { renderedButtons.delete(rowKey) }
            }}
            className="flex-1 px-1 text-xs text-teal-700 border border-teal-700 bg-white hover:bg-gray-50 select-none cursor-pointer"
          >
            {text}
          </button>
        )}
      </div>
    ),
    onCellKeyDown: args => {
      const { event, rowKey } = args
      if ((event.key === 'Enter' || event.key === ' ')
        && !event.ctrlKey && !event.altKey && !event.metaKey && !event.shiftKey
        && !event.nativeEvent.isComposing) {
        const button = renderedButtons.get(rowKey)
        if (button) {
          event.preventDefault()
          button.click()
        }
      }
      if (!event.defaultPrevented) onCellKeyDown?.(args)
    },
  }
}
