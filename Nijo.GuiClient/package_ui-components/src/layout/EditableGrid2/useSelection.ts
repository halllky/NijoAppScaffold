import React from "react"
import * as TanStack from "@tanstack/react-table"
import { ColumnMetadataInternal } from "./types-internal"
import { EditableGrid2Props } from "./types-public"
import { ScrollToCellFunction } from "./useScrollToCell"

/**
 * ボディセルの位置を表す構造体。
 * 不可視なセルを除いた、見えている範囲でのインデックス。
 * 列インデックスについてはチェックボックス列がある場合それを0とする。
 */
export interface CellPosition {
  rowIndex: number
  colIndex: number
}

/**
 * セル選択範囲を表す構造体。
 * 不可視なセルを除いた、見えている範囲でのインデックス。
 * 列インデックスについてはチェックボックス列がある場合それを0とする。
 */
export interface CellSelectionRange {
  startRow: number
  startCol: number
  endRow: number
  endCol: number
}

/**
 * グリッドの範囲選択機能を提供するフック。
 */
export function useSelection<TRow>(
  table: TanStack.Table<TRow>,
  props: EditableGrid2Props<TRow>,
  visibleLeafColumns: TanStack.ColumnDef<TRow, unknown>[],
  scrollToCell: ScrollToCellFunction,
) {

  //#region 状態

  // アンカーセル。範囲選択の起点。
  // Shiftキーを押しながら矢印キーやマウスクリックで選択範囲を拡張したとき、
  // このセルは固定されたまま、選択範囲の反対側のセルが移動する。
  const [anchorCell, setAnchorCell] = React.useState<CellPosition | null>(null)
  const setAnchorCellWithClamp = useClampSetter(setAnchorCell, props.rows.length, visibleLeafColumns.length, props.showCheckBox)

  // 選択範囲を構成する2点のセルのうちアンカーセルの反対側。
  // Shiftキーを押しながら矢印キーやマウスクリックで選択範囲を拡張したとき、
  // こちら側のセルが移動する。
  const [focusedCell, setFocusedCell] = React.useState<CellPosition | null>(null)
  const setFocusedCellWithClamp = useClampSetter(setFocusedCell, props.rows.length, visibleLeafColumns.length, props.showCheckBox)

  // マウスダウン中かどうか。
  // マウスダウンによってフォーカスが当たった場合、フォーカスイベントによる選択セルの上書きを防ぐために使用する。
  const isMouseDownRef = React.useRef(false)

  // 選択範囲。
  const selectedRange = React.useMemo<CellSelectionRange | null>(() => {
    if (!anchorCell || !focusedCell) return null
    return {
      startRow: Math.min(anchorCell.rowIndex, focusedCell.rowIndex),
      startCol: Math.min(anchorCell.colIndex, focusedCell.colIndex),
      endRow: Math.max(anchorCell.rowIndex, focusedCell.rowIndex),
      endCol: Math.max(anchorCell.colIndex, focusedCell.colIndex),
    }
  }, [anchorCell, focusedCell])

  // フォーカスを外す前に最後に選択していたセル
  const [lastFocused, setLastFocused] = React.useState<{ anchor: CellPosition | null, focused: CellPosition | null }>({ anchor: null, focused: null })

  // キー操作によるセル移動を検知して自動スクロールするために使う状態
  const [keyMoveState, setKeyMoveState] = React.useState<CellPosition | null>(null)

  //#endregion 状態

  // -------------------------------

  //#region イベント

  // 関数への参照を安定させて不要なレンダリングを抑制するためにメモ化する
  const handleKeyDown = React.useRef<React.KeyboardEventHandler>(() => { })
  const handleMouseDown = React.useRef<React.MouseEventHandler>(() => { })
  const handleMouseMove = React.useRef<React.MouseEventHandler>(() => { })
  const handleGridActiveChanged = React.useRef<(isGridActive: boolean) => void>(() => { })

  const tableRef = React.useRef(table)
  tableRef.current = table

  // 矢印キーによるセル移動
  handleKeyDown.current = e => {
    if (!focusedCell) return
    if (!['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.key)) return

    e.preventDefault()

    let { rowIndex, colIndex } = focusedCell
    const rowCount = props.rows.length
    const colCount = visibleLeafColumns.length
    const showCheckBox = props.showCheckBox === true || typeof props.showCheckBox === 'function'
    const minColIndex = showCheckBox ? 1 : 0

    if (e.ctrlKey) {
      // Ctrl キーが押されていれば端まで移動
      switch (e.key) {
        case 'ArrowUp': rowIndex = 0; break
        case 'ArrowDown': rowIndex = rowCount - 1; break
        case 'ArrowLeft': colIndex = minColIndex; break
        case 'ArrowRight': colIndex = colCount - 1; break
      }
    } else {
      // 押された矢印キーに応じて1セル移動
      switch (e.key) {
        case 'ArrowUp': rowIndex -= 1; break
        case 'ArrowDown': rowIndex += 1; break
        case 'ArrowLeft': colIndex -= 1; break
        case 'ArrowRight': colIndex += 1; break
      }
    }
    const nextPos = { rowIndex, colIndex }

    if (e.shiftKey) {
      setFocusedCellWithClamp(nextPos)
    } else {
      setAnchorCellWithClamp(nextPos)
      setFocusedCellWithClamp(nextPos)
    }

    // セル移動を検知してスクロールするために状態を更新
    setKeyMoveState(nextPos)
  }

  // マウスダウン。
  // Shiftキーが押されていれば範囲選択拡張、押されていなければ新規選択開始。
  handleMouseDown.current = e => {
    const helper = getHelper(tableRef)
    const cellPos = helper.getCellPositionFromMouseEvent(e)
    if (!cellPos) return

    // チェックボックス列が押された場合は無視
    if (cellPos.colIndex === 0
      && (props.showCheckBox === true
        || typeof props.showCheckBox === 'function')) {
      return
    }

    // セル選択
    if (e.shiftKey) {
      setFocusedCellWithClamp(cellPos)
    } else {
      setAnchorCellWithClamp(cellPos)
      setFocusedCellWithClamp(cellPos)
    }

    // マウスアップ時に解除するよう予約
    isMouseDownRef.current = true
    window.addEventListener('mouseup', () => {
      isMouseDownRef.current = false
    }, { once: true })
  }

  // マウスムーブ。
  // マウスダウン中であれば範囲選択拡張。
  handleMouseMove.current = e => {
    if (!isMouseDownRef.current) return

    const helper = getHelper(tableRef)
    const cellPos = helper.getCellPositionFromMouseEvent(e)
    if (!cellPos) return
    if (cellPos.rowIndex === focusedCell?.rowIndex
      && cellPos.colIndex === focusedCell?.colIndex) return

    setFocusedCellWithClamp(cellPos)
  }

  // グリッドのアクティブ状態が変化したとき。
  // * フォーカスがあたったときは、最後に選択していたセルか、それがなければ先頭セルを選択
  // * フォーカスが外れたときは選択解除（プロパティで指定されている場合のみ）
  handleGridActiveChanged.current = isGridActive => {
    const helper = getHelper(tableRef)

    if (isGridActive) {
      // マウスダウンによってフォーカスが当たった場合（セルクリック時など）は
      // handleMouseDown の方で適切なセルが選択されるため、
      // ここでの「前回選択していたセルの復元」は行わない。
      if (isMouseDownRef.current) return

      if (lastFocused.anchor && lastFocused.focused) {
        setAnchorCellWithClamp(lastFocused.anchor)
        setFocusedCellWithClamp(lastFocused.focused)
      } else {
        const firstCell = helper.getFirstDataCell()
        setAnchorCellWithClamp(firstCell)
        setFocusedCellWithClamp(firstCell)
      }

    } else if (props.clearSelectionOnBlur) {
      setLastFocused({
        anchor: anchorCell,
        focused: focusedCell,
      })
      setAnchorCellWithClamp(null)
      setFocusedCellWithClamp(null)
    }
  }

  const selectionEvents = React.useMemo(() => ({
    handleKeyDown: (e: React.KeyboardEvent) => handleKeyDown.current(e),
    handleMouseDown: (e: React.MouseEvent) => handleMouseDown.current(e),
    handleMouseMove: (e: React.MouseEvent) => handleMouseMove.current(e),
    handleGridActiveChanged: (isGridActive: boolean) => handleGridActiveChanged.current(isGridActive),
  }), [])

  //#endregion イベント

  // -------------------------------

  //#region useEffect

  // 行数、列数が変わったら選択セルをクランプする
  React.useEffect(() => {
    setAnchorCellWithClamp(anchorCell)
    setFocusedCellWithClamp(focusedCell)
  }, [
    props.rows.length,
    visibleLeafColumns.length,
  ])

  // キー操作によるセル移動を検知して移動後のセルが見えるように自動スクロール
  React.useEffect(() => {
    if (keyMoveState) {
      scrollToCell(keyMoveState)
      setKeyMoveState(null)
    }
  }, [keyMoveState, scrollToCell])

  //#endregion useEffect

  return {
    selectedRange,
    anchorCell,
    focusedCell,
    selectionEvents,
  }
}

/**
 * セル位置設定関数のラッパー。
 * 指定された最大行・列インデックスを超えないようにクランプしてから設定する。
 */
function useClampSetter(
  setter: React.Dispatch<React.SetStateAction<CellPosition | null>>,
  rowCount: number,
  colCount: number,
  hasCheckBoxColumn?: EditableGrid2Props<any>["showCheckBox"],
) {
  return React.useCallback((cell: CellPosition | null) => {
    if (!cell || rowCount === 0 || colCount === 0) {
      setter(null)

    } else {
      const showCheckBox = hasCheckBoxColumn === true || typeof hasCheckBoxColumn === 'function'
      setter({
        rowIndex: Math.min(Math.max(cell.rowIndex, 0), rowCount - 1),
        colIndex: Math.min(Math.max(cell.colIndex, showCheckBox ? 1 : 0), colCount - 1),
      })
    }
  }, [setter, rowCount, colCount, hasCheckBoxColumn])
}

/**
 * Tanstack Table の標準のAPIに加えて
 * EditableGrid2 固有の情報を考慮したヘルパーを作成する
 */
function getHelper<TRow>(
  tableRef: React.RefObject<TanStack.Table<TRow>>,
) {

  return {
    /** データが1行以上あるか */
    hasDataRow: () => tableRef.current.getRowModel().rows.length > 0,

    /** 列が1列以上あるか。チェックボックス列は除外。 */
    hasVisibleDataColumn: () => tableRef.current.getVisibleFlatColumns().some(col => {
      const meta = col.columnDef.meta as ColumnMetadataInternal<TRow>
      return !meta.isRowCheckBox
    }),

    /** 選択可能な最初のセルを取得する。データ行やデータ列が存在しない場合は null を返す。 */
    getFirstDataCell: (): CellPosition | null => {
      const rowModel = tableRef.current.getRowModel()
      if (rowModel.rows.length === 0) return null

      const dataColumns = tableRef.current.getVisibleFlatColumns().filter(col => {
        const meta = col.columnDef.meta as ColumnMetadataInternal<TRow>
        return !meta.isRowCheckBox
      })
      if (dataColumns.length === 0) return null

      return {
        rowIndex: 0,
        colIndex: dataColumns[0].getIndex(),
      }
    },

    /** マウスイベントの座標と対応するセル位置を取得する */
    getCellPositionFromMouseEvent: (e: React.MouseEvent): CellPosition | null => {
      const target = e.target as HTMLElement
      const td = target.closest('td')
      if (!td) return null

      // 属性名は EditableGrid2.tsx で設定しているものと一致させる必要がある
      const rowIndex = Number(td.getAttribute('data-row-index'))
      const colIndex = Number(td.getAttribute('data-col-index'))

      if (isNaN(rowIndex) || isNaN(colIndex)) return null

      return { rowIndex, colIndex }
    },
  }
}
