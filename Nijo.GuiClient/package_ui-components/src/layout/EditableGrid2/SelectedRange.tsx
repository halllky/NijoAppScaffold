import React from "react"
import { GetPixelFunction } from "./useGetPixel"
import { CellPosition, CellSelectionRange } from "./useSelection"

export type SelectedRangeProps = {
  /** アンカーセルの位置 */
  anchorCell: CellPosition | null
  /** 選択範囲 */
  selectedRange: CellSelectionRange | null
  /** 座標計算関数 */
  getPixel: GetPixelFunction
  /** 最も右にある固定列のインデックス。チェックボックス列がある場合はそれを0とする */
  lastFixedIndex: number | null
}

/**
 * 固定列用の選択範囲表示
 */
export const SelectedRangeForFixedColumn = React.memo(({
  anchorCell,
  getPixel,
  lastFixedIndex,
  selectedRange,
}: SelectedRangeProps) => {

  if (!anchorCell) return null
  if (!selectedRange) return null
  if (lastFixedIndex === null) return null
  if (selectedRange.startCol > lastFixedIndex) return null

  // 固定列の範囲（0 ～ lastFixedIndex）でクリップする
  const clippedRange: CellSelectionRange = {
    ...selectedRange,
    endCol: Math.min(selectedRange.endCol, lastFixedIndex),
  }

  return (
    // z-index は固定列ヘッダより後ろ、固定列ボディセルより手前
    <div className="sticky left-0 z-15">
      <SelectedRangeImpl
        getPixel={getPixel}
        selectedRange={clippedRange}
        hideBorderRight={selectedRange.endCol > lastFixedIndex}

        // アンカーセルが固定列の範囲外の場合、
        // 範囲全体をオーバーレイで覆わせるために null を渡す
        anchorCell={anchorCell.colIndex > lastFixedIndex ? null : anchorCell}
      />
    </div>
  )
})

/**
 * スクロール列用の選択範囲表示
 */
export const SelectedRangeForScrollableColumn = React.memo(({
  anchorCell,
  getPixel,
  selectedRange,
}: SelectedRangeProps) => {

  if (!anchorCell) return null
  if (!selectedRange) return null

  return (
    <SelectedRangeImpl
      getPixel={getPixel}
      anchorCell={anchorCell}
      selectedRange={selectedRange}
      zIndex="5" // 固定列ボディセルより後ろ、非固定列ボディセルより手前
    />
  )
})

/**
 * 選択範囲表示の実装部分
 */
function SelectedRangeImpl({
  zIndex,
  getPixel,
  anchorCell,
  selectedRange,
  hideBorderRight,
}: {
  zIndex?: string
  getPixel: GetPixelFunction
  anchorCell: CellPosition | null
  selectedRange: CellSelectionRange
  hideBorderRight?: boolean
}) {

  // 選択範囲全体の座標
  const left = getPixel({ position: 'left', colIndex: selectedRange.startCol })
  const right = getPixel({ position: 'right', colIndex: selectedRange.endCol })
  const top = getPixel({ position: 'top', rowIndex: selectedRange.startRow })
  const bottom = getPixel({ position: 'bottom', rowIndex: selectedRange.endRow })

  // アンカーセルの座標
  const anchorTop = anchorCell === null ? null : getPixel({ position: 'top', rowIndex: anchorCell.rowIndex })
  const anchorBottom = anchorCell === null ? null : getPixel({ position: 'bottom', rowIndex: anchorCell.rowIndex })
  const anchorLeft = anchorCell === null ? null : getPixel({ position: 'left', colIndex: anchorCell.colIndex })
  const anchorRight = anchorCell === null ? null : getPixel({ position: 'right', colIndex: anchorCell.colIndex })

  const borderClassNames = "absolute pointer-events-none border-1 border-sky-500"
  const overlayClassName = "absolute pointer-events-none bg-sky-200/25 mix-blend-multiply"

  return (
    <>
      {/* 選択範囲全体の外枠 */}
      <div className={borderClassNames} style={{
        left: `${left}px`,
        top: `${top}px`,
        minWidth: `${Math.max(0, right - left)}px`,
        minHeight: `${Math.max(0, bottom - top)}px`,
        zIndex,
        borderRightWidth: hideBorderRight ? 0 : undefined,
      }} />

      {/* アンカーセルの位置だけ背景色なし、それ以外の選択セルは背景色あり、とするため、4つのdivでアンカーセル以外の部分を覆う */}
      {anchorCell && (<>
        <div className={overlayClassName} style={{
          left: `${left}px`,
          top: `${anchorTop}px`,
          width: `${Math.max(0, anchorLeft! - left)}px`,
          height: `${Math.max(0, anchorBottom! - anchorTop!)}px`,
          zIndex,
        }} />
        <div className={overlayClassName} style={{
          left: `${anchorRight}px`,
          top: `${anchorTop}px`,
          width: `${Math.max(0, right - anchorRight!)}px`,
          height: `${Math.max(0, anchorBottom! - anchorTop!)}px`,
          zIndex,
        }} />
        <div className={overlayClassName} style={{
          left: `${left}px`,
          top: `${top}px`,
          width: `${Math.max(0, right - left)}px`,
          height: `${Math.max(0, anchorTop! - top)}px`,
          zIndex,
        }} />
        <div className={overlayClassName} style={{
          left: `${left}px`,
          top: `${anchorBottom}px`,
          width: `${Math.max(0, right - left)}px`,
          height: `${Math.max(0, bottom - anchorBottom!)}px`,
          zIndex,
        }} />
      </>)}

      {/* アンカーセルが表示範囲外にある場合は全面をオーバーレイ */}
      {anchorCell === null && (
        <div className={overlayClassName} style={{
          left: `${left}px`,
          top: `${top}px`,
          width: `${Math.max(0, right - left)}px`,
          height: `${Math.max(0, bottom - top)}px`,
          zIndex,
        }} />
      )}
    </>
  )
}
