import React from "react"
import { GetPixelFunction } from "./useGetPixel"
import { CellPosition, CellSelectionRange } from "./useSelection"

export type SelectedRangeProps = {
  /** グリッド自体にフォーカスが当たっているかどうか */
  isGridActive: boolean
  /** 座標計算関数 */
  getPixel: GetPixelFunction
  /** アンカーセルの位置 */
  anchorCell: CellPosition | null
  /** 選択範囲 */
  selectedRange: CellSelectionRange | null
}

export const SelectedRange = React.memo((props: SelectedRangeProps) => {
  return (
    <>
      {/* 非固定列に対する選択範囲。固定ボディセルより後ろ、非固定ボディセルより手前に表示する必要がある。 */}
      <SelectedRangeImpl {...props} zIndex="5" />

      {/* 固定列に対する選択範囲。固定列ヘッダセルより後ろ、固定列ボディセルより手前に表示する必要がある。 */}
      {/* <SelectedRangeImpl {...props} zIndex="30" /> */}
    </>
  )
})

type SelectedRangeImplProps = SelectedRangeProps & {
  /** 固定列に対する選択の場合とそうでない場合で z-index が変わるので */
  zIndex: string
}

function SelectedRangeImpl({
  isGridActive,
  zIndex,
  getPixel,
  anchorCell,
  selectedRange,
}: SelectedRangeImplProps) {

  // DOMへのアクセス
  const containerRef = React.useRef<HTMLDivElement>(null)
  const leftRef = React.useRef<HTMLDivElement>(null)
  const rightRef = React.useRef<HTMLDivElement>(null)
  const aboveRef = React.useRef<HTMLDivElement>(null)
  const belowRef = React.useRef<HTMLDivElement>(null)

  React.useEffect(() => {
    if (!selectedRange) return;
    if (!isGridActive) return;
    if (!containerRef.current) return;

    const left = getPixel({ position: 'left', colIndex: selectedRange.startCol })
    const right = getPixel({ position: 'right', colIndex: selectedRange.endCol })
    const top = getPixel({ position: 'top', rowIndex: selectedRange.startRow })
    const bottom = getPixel({ position: 'bottom', rowIndex: selectedRange.endRow })
    containerRef.current.style.left = `${left}px`
    containerRef.current.style.top = `${top}px`
    containerRef.current.style.minWidth = `${right - left}px`
    containerRef.current.style.minHeight = `${bottom - top}px`
    containerRef.current.style.zIndex = zIndex

    if (leftRef.current && rightRef.current && aboveRef.current && belowRef.current) {
      if (anchorCell) {
        const activeTop = getPixel({ position: 'top', rowIndex: anchorCell.rowIndex })
        const activeBottom = getPixel({ position: 'bottom', rowIndex: anchorCell.rowIndex })
        const activeLeft = getPixel({ position: 'left', colIndex: anchorCell.colIndex })
        const activeRight = getPixel({ position: 'right', colIndex: anchorCell.colIndex })

        leftRef.current.style.left = `${left}px`
        leftRef.current.style.top = `${activeTop}px`
        leftRef.current.style.width = `${activeLeft - left}px`
        leftRef.current.style.height = `${activeBottom - activeTop}px`
        leftRef.current.style.zIndex = zIndex

        rightRef.current.style.left = `${activeRight}px`
        rightRef.current.style.top = `${activeTop}px`
        rightRef.current.style.width = `${right - activeRight}px`
        rightRef.current.style.height = `${activeBottom - activeTop}px`
        rightRef.current.style.zIndex = zIndex

        aboveRef.current.style.left = `${left}px`
        aboveRef.current.style.top = `${top}px`
        aboveRef.current.style.width = `${right - left}px`
        aboveRef.current.style.height = `${activeTop - top}px`
        aboveRef.current.style.zIndex = zIndex

        belowRef.current.style.left = `${left}px`
        belowRef.current.style.top = `${activeBottom}px`
        belowRef.current.style.width = `${right - left}px`
        belowRef.current.style.height = `${bottom - activeBottom}px`
        belowRef.current.style.zIndex = zIndex
      } else {
        leftRef.current.style.width = '0px'
        leftRef.current.style.height = '0px'
        rightRef.current.style.width = '0px'
        rightRef.current.style.height = '0px'
        aboveRef.current.style.width = '0px'
        aboveRef.current.style.height = '0px'
        belowRef.current.style.width = '0px'
        belowRef.current.style.height = '0px'
      }
    }
  }, [
    anchorCell,
    selectedRange,
    isGridActive,
    getPixel,
    zIndex,
    containerRef,
    leftRef,
    rightRef,
    aboveRef,
    belowRef,
  ])

  // フォーカスが当たっていない場合は何も表示しない
  if (!isGridActive) {
    return null;
  }

  return (
    <>
      {/* 選択範囲全体の外枠 */}
      <div ref={containerRef} className="absolute border-1 border-sky-500 pointer-events-none"></div>

      {/* アクティブセルの位置だけ背景色なし、それ以外の選択セルは背景色あり、とするため、4つのdivでアクティブセル以外の部分を覆う */}
      <div ref={leftRef} className="absolute bg-sky-200/25 mix-blend-multiply pointer-events-none" />
      <div ref={rightRef} className="absolute bg-sky-200/25 mix-blend-multiply pointer-events-none" />
      <div ref={aboveRef} className="absolute bg-sky-200/25 mix-blend-multiply pointer-events-none" />
      <div ref={belowRef} className="absolute bg-sky-200/25 mix-blend-multiply pointer-events-none" />
    </>
  )
}
