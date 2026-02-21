import React from "react"
import * as TanStack from "@tanstack/react-table"
import { EditableGridCellEditor, EditableGridCellEditorRef } from "./types-public"
import { checkIfCellReadOnly, ColumnMetadataInternal } from "./types-internal"
import { CellPosition } from "./useSelection"
import { GetPixelFunction } from "./useGetPixel"
import { RowAccessor } from "./useRowAccessor"

export type CellEditorProps<TRow> = {
  /** グリッド全体がアクティブ状態かどうか */
  isGridActive: boolean
  /** useSelection で管理されているフォーカスセル */
  focusedCell: CellPosition | null
  rowModel: TanStack.RowModel<TRow>
  visibleLeafColumns: TanStack.Column<TRow, unknown>[]
  /** 編集状態が変わったときに呼ばれるコールバック */
  onEditingStateChanged: (isEditing: boolean) => void
  /** グリッド全体のpropsで指定される標準コンポーネント */
  gridEditorComponent?: EditableGridCellEditor
  /** グリッド全体のpropsで指定される読み取り専用条件 */
  gridIsReadOnly: boolean | ((row: TRow, rowIndex: number) => boolean) | undefined
  /** 座標計算関数 */
  getPixel: GetPixelFunction
  /** 最新の行データを取得する関数 */
  getRowObject: RowAccessor<TRow>
}

export type CellEditorRef = {
  /**
   * EditableGrid2 側でトリガーしてセルエディタに編集開始を要求するために使用
   *
   * @param inputChar クイック編集で最初に入力された文字。nullの場合は通常の編集開始。
   */
  requestEditStart: (inputChar: string | null) => void
}

/**
 * セルの編集を行うコンポーネント。
 * 通常時は透明で表示される。編集モードになると可視化される。
 *
 * キーボードでIME変換が必要な文字が入力された場合、
 * 最初の1文字目がIME変換候補状態で表示されるという動きを実現するため、
 * EditableGrid にフォーカスが当たっているうちは、見えないだけで、必ずこのコンポーネントにフォーカスが当たる。
 */
export const CellEditor = React.forwardRef(function CellEditor<TRow>({
  isGridActive,
  focusedCell,
  rowModel,
  visibleLeafColumns,
  onEditingStateChanged,
  gridEditorComponent,
  gridIsReadOnly,
  getPixel,
  getRowObject,
}: CellEditorProps<TRow>, ref: React.ForwardedRef<CellEditorRef>) {

  const editorTextareaRef = React.useRef<EditableGridCellEditorRef>(null)

  const [editorComponent, setEditorComponent] = React.useState<EditableGridCellEditor>(gridEditorComponent ?? NoopEditor)
  const [edittingCell, setEdittingCell] = React.useState<TanStack.Cell<TRow, unknown> | null>(null)

  const isGridActiveRef = React.useRef(isGridActive)
  isGridActiveRef.current = isGridActive

  // -----------------------------------

  // 編集確定
  const commitEditing = (v?: string) => {
    if (edittingCell === null) return;

    const columnMeta = edittingCell.column.columnDef.meta as ColumnMetadataInternal<TRow>
    const value = v ?? editorTextareaRef.current?.getCurrentValue() ?? ''
    if (columnMeta.original?.setValueFromEditor) {
      columnMeta.original.setValueFromEditor({
        rowIndex: edittingCell.row.index,
        row: getRowObject(edittingCell.row.index),
        value,
      })
    }

    setEdittingCell(null)
    onEditingStateChanged(false)

    // エディタが select 要素のとき編集確定後にキー操作ができなくなるので setTimeiout を挟む。
    // グリッドの中にフォーカスがある状態でグリッド外のボタンをクリックするなどした場合、
    // setTimeout後の時間ではグリッドからフォーカスが外れてしまっている可能性があるので考慮する。
    window.setTimeout(() => {
      if (!isGridActiveRef.current) return
      editorTextareaRef.current?.setValueAndSelectAll(value, 'edit-end')
    }, 0)
  }
  const commitEditingRef = React.useRef(commitEditing)
  commitEditingRef.current = commitEditing

  // 画面外クリックで編集確定
  React.useEffect(() => {
    if (!edittingCell) return

    const handleMouseDown = (e: MouseEvent) => {
      // エディタ内のクリックなら無視
      const domElement = editorTextareaRef.current?.getDomElement?.()
      if (domElement && e.target instanceof Node && domElement.contains(e.target)) return
      // エディタ外のクリックなら確定
      commitEditingRef.current()
    }

    document.addEventListener('mousedown', handleMouseDown)
    return () => document.removeEventListener('mousedown', handleMouseDown)
  }, [edittingCell, commitEditingRef])

  // 編集キャンセル
  const cancelEditing = () => {
    // エディタの値を編集前の値に戻す
    if (edittingCell) {
      const columnMeta = edittingCell.column.columnDef.meta as ColumnMetadataInternal<TRow>
      const rowOriginal = getRowObject(edittingCell.row.index)
      const value = columnMeta.original?.getValueForEditor
        ? columnMeta.original.getValueForEditor({ row: rowOriginal, rowIndex: edittingCell.row.index })
        : ''
      // エディタが select 要素のとき編集確定後にキー操作ができなくなるので setTimeiout を挟む。
      // グリッドの中にフォーカスがある状態でグリッド外のボタンをクリックするなどした場合、
      // setTimeout後の時間ではグリッドからフォーカスが外れてしまっている可能性があるので考慮する。
      window.setTimeout(() => {
        if (!isGridActiveRef.current) return
        editorTextareaRef.current?.setValueAndSelectAll(value, 'edit-end')
      }, 0)
    }

    setEdittingCell(null)
    onEditingStateChanged(false)
  }

  // エディタの外観
  const editorStyle = React.useMemo((): React.CSSProperties => {
    const style: React.CSSProperties = {
      position: 'absolute',
      zIndex: 30, // 固定列ヘッダよりも手前
      // クイック編集のためCellEditor自体は常に存在し続けるが、セル編集モードでないときは見えないようにする
      opacity: edittingCell ? undefined : 0,
      pointerEvents: edittingCell ? undefined : 'none',
    }

    if (!focusedCell) return style

    // エディタを編集対象セルの位置に移動させる
    const left = getPixel({ position: 'left', colIndex: focusedCell.colIndex })
    const right = getPixel({ position: 'right', colIndex: focusedCell.colIndex })
    const top = getPixel({ position: 'top', rowIndex: focusedCell.rowIndex })
    const bottom = getPixel({ position: 'bottom', rowIndex: focusedCell.rowIndex })
    style.left = `${left}px`
    style.top = `${top}px`

    // wrapするセルの編集中はテキストボックスが伸縮する必要がある。
    // 編集中でないときはエディタがグリッドの下限を超えて余計なスクロールが出るのを防ぐためheight固定
    const columnMeta = visibleLeafColumns[focusedCell.colIndex]?.columnDef.meta as ColumnMetadataInternal<TRow> | undefined
    if (edittingCell && columnMeta?.original?.wrap) {
      style.minHeight = `${bottom - top}px`
    } else {
      style.height = `${bottom - top}px`
    }

    // min-width で設定した場合、エディタの中の文字がオーバーフローしたときに横方向に延伸する。
    // width で指定した場合は縦方向。
    if (columnMeta?.original?.wrap) {
      style.width = `${right - left}px`
      style.minWidth = ''
    } else {
      style.width = ''
      style.minWidth = `${right - left}px`
    }

    return style
  }, [focusedCell, getPixel, edittingCell, visibleLeafColumns])

  // 移動後のセルの値をエディタにセットする
  React.useEffect(() => {
    if (edittingCell) return
    if (!focusedCell) return

    // 移動先の列のエディタコンポーネントに切り替え
    const columnMeta = visibleLeafColumns[focusedCell.colIndex]?.columnDef.meta as ColumnMetadataInternal<TRow> | undefined
    let value = ''
    if (columnMeta?.original?.getValueForEditor) {
      const cell = rowModel
        .flatRows[focusedCell.rowIndex]
        ?.getVisibleCells()
        ?.[focusedCell.colIndex]
      if (cell) {
        value = columnMeta.original.getValueForEditor({ row: getRowObject(cell.row.index), rowIndex: cell.row.index })
      }
      setEditorComponent(columnMeta.original?.editor ?? gridEditorComponent ?? NoopEditor)
    }

    // グリッドにフォーカスが当たった瞬間に確実にフォーカスさせるためsetTimeoutを挟む。
    // グリッドの中にフォーカスがある状態でグリッド外のボタンをクリックするなどした場合、
    // setTimeout後の時間ではグリッドからフォーカスが外れてしまっている可能性があるので考慮する。
    window.setTimeout(() => {
      if (!isGridActiveRef.current) return
      editorTextareaRef.current?.setValueAndSelectAll(value, 'move-focus')
    }, 0)
  }, [focusedCell, rowModel, visibleLeafColumns])

  // ref
  React.useImperativeHandle(ref, () => ({
    requestEditStart: inputChar => {
      if (!focusedCell) return;

      // エディタコンポーネントが指定されていない場合は編集開始しない
      if (editorComponent === NoopEditor) return;

      const cell = rowModel
        .flatRows[focusedCell.rowIndex]
        ?.getVisibleCells()
        ?.[focusedCell.colIndex]
      if (!cell) return;

      const columnMeta = cell.column.columnDef.meta as ColumnMetadataInternal<TRow>
      if (!columnMeta.original?.getValueForEditor) return;

      const rowOriginal = getRowObject(cell.row.index)
      if (checkIfCellReadOnly(cell, gridIsReadOnly, rowOriginal)) return;

      // 英数字などIME変換不要な文字が入力されたことによる編集開始の場合、
      // その文字を初期値としてエディタにセットする
      const value = inputChar ?? columnMeta.original.getValueForEditor({ row: rowOriginal, rowIndex: cell.row.index })
      editorTextareaRef.current?.setValueAndSelectAll(value, 'edit-start')

      setEdittingCell(cell)
      onEditingStateChanged(true)
    },
  }))

  return React.createElement(editorComponent, {
    isEditing: edittingCell !== null,
    requestCommit: commitEditing,
    requestCancel: cancelEditing,
    style: editorStyle,
    ref: editorTextareaRef,
  })
}) as (<TRow>(props: CellEditorProps<TRow> & { ref?: React.ForwardedRef<CellEditorRef> }) => React.ReactNode)


/**
 * エディタコンポーネントが指定されていない場合のデフォルトのエディタ。
 * フォーカスの保持だけを行い、エディタとしての機能は持たない。
 */
const NoopEditor: EditableGridCellEditor = React.forwardRef(function NoopEditor({ style }, ref) {

  const textareaRef = React.useRef<HTMLTextAreaElement>(null)

  React.useImperativeHandle(ref, () => ({
    blur: () => textareaRef.current?.blur(),
    getCurrentValue: () => textareaRef.current?.value ?? '',
    setValueAndSelectAll: () => textareaRef.current?.select(),
    getDomElement: () => textareaRef.current,
  }), [textareaRef])

  // このコンポーネントは常に非表示
  return (
    <textarea ref={textareaRef} style={style} />
  )
})
