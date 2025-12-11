import React, { useMemo } from "react"
import {
  useReactTable,
  getCoreRowModel,
  createColumnHelper,
  flexRender,
} from "@tanstack/react-table"

/** YYYY/MM 形式の文字列 */
export type YearMonth = `${number}/${number}`

export type GridProps<TRow extends Row<TItem>, TItem extends BodyItem> = {
  /** 表示範囲の始点 */
  since: YearMonth
  /** 表示範囲の終点 */
  until: YearMonth
  /** 年月1個分の横幅(px指定) */
  calendarBodyWidthPx: number
  /** グリッドの各行 */
  rows: TRow[]
  /** グリッドのボディのアイテムを描画するための関数 */
  renderBodyItem: (props: BodyItemRendererProps<TRow, TItem>) => React.ReactNode
  /** グリッドの左側の固定列を描画するための関数の配列 */
  renderLeftColumns?: FixedColumnRenderer<TRow, TItem>[]
  /** グリッドの右側の固定列を描画するための関数の配列 */
  renderRightColumns?: FixedColumnRenderer<TRow, TItem>[]
  /** グリッドのボディのアイテムがクリックされたときのイベントハンドラ */
  onBodyItemClick?: (ev: BodyItemEventArgs<TRow, TItem>) => void
  /** グリッドのボディのアイテムがダブルクリックされたときのイベントハンドラ */
  onBodyItemDoubleClick?: (ev: BodyItemEventArgs<TRow, TItem>) => void
  /** グリッドのボディの何もない部分がクリックされたときのイベントハンドラ */
  onBodyClick?: (ev: BodyEventArgs<TRow, TItem>) => void
  /** グリッドのボディの何もない部分がダブルクリックされたときのイベントハンドラ */
  onBodyDoubleClick?: (ev: BodyEventArgs<TRow, TItem>) => void
  /** コンポーネント全体に適用 */
  className?: string
}

/** カレンダーの横1行分のオブジェクトが備えるべき属性 */
export type Row<TItem extends BodyItem> = {
  /** カレンダーのボディ部分に表示されるアイテム */
  bodyItems: TItem[]
}

/** カレンダーのボディ部分に表示されるアイテムが備えるべき属性 */
export type BodyItem = (
  | { type: 'point', yearMonth: YearMonth }
  | { type: 'range', since: YearMonth, until: YearMonth }
) & {
  /** アイテムの高さ(px) */
  heightPx?: number
}

// ----------------------------------------
// イベント引数型

/** カレンダー左右の固定列のレンダリング処理の引数 */
export type FixedColumnRenderer<TRow extends Row<TItem>, TItem extends BodyItem> = {
  header?: React.ReactNode
  body: (props: { row: TRow, rowIndex: number }) => React.ReactNode
  /** 列の幅(px指定) */
  widthPx?: number
}

/** ボディ要素レンダリング処理の引数 */
export type BodyItemRendererProps<TRow extends Row<TItem>, TItem extends BodyItem> = {
  row: TRow
  rowIndex: number
  item: TItem
}

/** ボディの何もない部分に対するイベントの引数 */
export type BodyEventArgs<TRow extends Row<TItem>, TItem extends BodyItem> = {
  row: TRow
  rowIndex: number
  yearMonth: YearMonth
}

/** ボディのアイテムに対するイベントの引数 */
export type BodyItemEventArgs<TRow extends Row<TItem>, TItem extends BodyItem> = {
  row: TRow
  rowIndex: number
  yearMonth: YearMonth
  item: TItem
}

// ----------------------------------------

/**
 * 横軸に年月をとり、縦軸にアイテムを並べるグリッドコンポーネント。
 */
export function Grid<TRow extends Row<TItem>, TItem extends BodyItem>(props: GridProps<TRow, TItem>) {
  const {
    since,
    until,
    calendarBodyWidthPx,
    rows,
    renderBodyItem,
    renderLeftColumns,
    renderRightColumns,
    onBodyItemClick,
    onBodyItemDoubleClick,
    onBodyClick,
    onBodyDoubleClick,
    className,
  } = props

  const months = useMemo(() => getMonthsList(since, until), [since, until])

  const columns = useMemo(() => {
    const columnHelper = createColumnHelper<TRow>()
    const leftCols = renderLeftColumns?.map((col, i) =>
      columnHelper.display({
        id: `left-${i}`,
        header: () => col.header,
        cell: info => col.body({ row: info.row.original, rowIndex: info.row.index }),
        size: col.widthPx ?? 64,
        meta: { sticky: 'left' } satisfies ColumnMeta,
      })
    ) ?? []

    const bodyCol = columnHelper.display({
      id: 'body',
      size: calendarBodyWidthPx * months.length,
      // 年月ヘッダー（すべての年月を1個の列として扱っている）
      header: () => (
        <div className="flex h-full">
          {months.map(ym => (
            <div
              key={ym}
              className="flex-1 border-l border-gray-200 border-b border-gray-300 text-center text-sm box-border min-w-0 flex items-center justify-center"
            >
              {ym}
            </div>
          ))}
        </div>
      ),
      // 年月ボディ部分（すべての年月を1個の列として扱っている）
      cell: info => {
        const { totalHeight, itemsWithLayout } = calculateRowLayout(info.row.original.bodyItems, months)
        return (
          <div className="relative w-full" style={{ height: totalHeight }}>
            {/* 各年月の背景 */}
            <div className="absolute inset-0 flex w-full h-full">
              {months.map(ym => (
                <div
                  key={ym}
                  className="flex-1 border-l border-gray-200 h-full"
                  onClick={() => onBodyClick?.({ row: info.row.original, rowIndex: info.row.index, yearMonth: ym })}
                  onDoubleClick={() => onBodyDoubleClick?.({ row: info.row.original, rowIndex: info.row.index, yearMonth: ym })}
                >
                </div>
              ))}
            </div>

            {/* Items */}
            {itemsWithLayout.map(({ item, style, startYM }, itemIndex) => (
              <div
                key={itemIndex}
                className="absolute z-10 overflow-hidden whitespace-nowrap"
                style={style}
                onClick={(e) => {
                  e.stopPropagation()
                  onBodyItemClick?.({ row: info.row.original, rowIndex: info.row.index, item: item as TItem, yearMonth: startYM })
                }}
                onDoubleClick={(e) => {
                  e.stopPropagation()
                  onBodyItemDoubleClick?.({ row: info.row.original, rowIndex: info.row.index, item: item as TItem, yearMonth: startYM })
                }}
              >
                {renderBodyItem({ row: info.row.original, rowIndex: info.row.index, item: item as TItem })}
              </div>
            ))}
          </div>
        )
      },
    })

    const rightCols = renderRightColumns?.map((col, i) =>
      columnHelper.display({
        id: `right-${i}`,
        header: () => col.header,
        cell: info => col.body({ row: info.row.original, rowIndex: info.row.index }),
        size: col.widthPx ?? 64,
        meta: { sticky: 'right' } satisfies ColumnMeta,
      })
    ) ?? []

    return [...leftCols, bodyCol, ...rightCols]
  }, [renderLeftColumns, renderRightColumns, calendarBodyWidthPx, months, onBodyClick, onBodyDoubleClick, onBodyItemClick, onBodyItemDoubleClick, renderBodyItem])

  const table = useReactTable({
    data: rows,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  return (
    <div className={`overflow-auto ${className ?? ''}`}>
      <table
        className="border-separate border-spacing-0 table-fixed"
        style={{ width: table.getTotalSize() }}
      >
        <thead>
          {table.getHeaderGroups().map(headerGroup => (
            <tr key={headerGroup.id}>
              {headerGroup.headers.map(header => {
                const isStickyLeft = (header.column.columnDef.meta as ColumnMeta)?.sticky === 'left'
                const isStickyRight = (header.column.columnDef.meta as ColumnMeta)?.sticky === 'right'

                let style: React.CSSProperties = {
                  width: header.getSize(),
                }

                if (isStickyLeft) {
                  style.position = 'sticky'
                  style.left = header.getStart()
                  style.zIndex = 30
                } else if (isStickyRight) {
                  style.position = 'sticky'
                  style.right = table.getTotalSize() - header.getStart() - header.getSize()
                  style.zIndex = 30
                }

                return (
                  <th
                    key={header.id}
                    colSpan={header.colSpan}
                    style={style}
                    className="p-0 bg-gray-100"
                  >
                    {header.isPlaceholder
                      ? null
                      : flexRender(header.column.columnDef.header, header.getContext())}
                  </th>
                )
              })}
            </tr>
          ))}
        </thead>
        <tbody>
          {table.getRowModel().rows.map(row => (
            <tr key={row.id}>
              {row.getVisibleCells().map(cell => {
                const isStickyLeft = (cell.column.columnDef.meta as ColumnMeta)?.sticky === 'left'
                const isStickyRight = (cell.column.columnDef.meta as ColumnMeta)?.sticky === 'right'

                let style: React.CSSProperties = {
                  width: cell.column.getSize(),
                }

                if (isStickyLeft) {
                  style.position = 'sticky'
                  style.left = cell.column.getStart()
                  style.zIndex = 20
                } else if (isStickyRight) {
                  style.position = 'sticky'
                  style.right = table.getTotalSize() - cell.column.getStart() - cell.column.getSize()
                  style.zIndex = 20
                }

                return (
                  <td
                    key={cell.id}
                    style={style}
                    className={`relative p-0 border-b border-gray-200 ${isStickyLeft ? 'border-r border-gray-300 bg-white' : ''} ${isStickyRight ? 'border-l border-gray-300 bg-white' : ''}`}
                  >
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                )
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function parseYearMonth(ym: YearMonth): { year: number, month: number } {
  const [y, m] = ym.split('/').map(Number)
  return { year: y, month: m }
}

function getMonthsList(since: YearMonth, until: YearMonth): YearMonth[] {
  const result: YearMonth[] = []
  const s = parseYearMonth(since)
  const u = parseYearMonth(until)

  let currentY = s.year
  let currentM = s.month

  while (currentY < u.year || (currentY === u.year && currentM <= u.month)) {
    result.push(`${currentY}/${currentM.toString().padStart(2, '0')}` as YearMonth)
    currentM++
    if (currentM > 12) {
      currentM = 1
      currentY++
    }
  }
  return result
}

type YearGroup = { year: number, months: YearMonth[] }
function getYearGroups(months: YearMonth[]): YearGroup[] {
  const groups: YearGroup[] = []
  let currentGroup: YearGroup | null = null

  for (const ym of months) {
    const { year } = parseYearMonth(ym)
    if (!currentGroup || currentGroup.year !== year) {
      currentGroup = { year, months: [] }
      groups.push(currentGroup)
    }
    currentGroup.months.push(ym)
  }
  return groups
}

function calculateItemPosition(item: BodyItem, months: YearMonth[]): { left: number, width: number, startYM: YearMonth, startIdx: number, endIdx: number } | null {
  const firstMonth = months[0]
  const lastMonth = months[months.length - 1]

  let effectiveStart: string
  let effectiveEnd: string

  if (item.type === 'point') {
    if (item.yearMonth < firstMonth || item.yearMonth > lastMonth) return null
    effectiveStart = item.yearMonth
    effectiveEnd = item.yearMonth
  } else {
    if (item.until < firstMonth || item.since > lastMonth) return null
    effectiveStart = item.since < firstMonth ? firstMonth : item.since
    effectiveEnd = item.until > lastMonth ? lastMonth : item.until
  }

  const startIdx = months.indexOf(effectiveStart as YearMonth)
  const endIdx = months.indexOf(effectiveEnd as YearMonth)

  const totalMonths = months.length
  const oneMonthWidth = 100 / totalMonths

  const left = startIdx * oneMonthWidth
  const width = (endIdx - startIdx + 1) * oneMonthWidth

  return { left, width, startYM: effectiveStart as YearMonth, startIdx, endIdx }
}

function calculateRowLayout(items: BodyItem[], months: YearMonth[]) {
  // 1. Calculate horizontal positions for all items
  const positionedItems = items.map(item => {
    const pos = calculateItemPosition(item, months)
    return { item, pos }
  }).filter((x): x is { item: BodyItem, pos: NonNullable<ReturnType<typeof calculateItemPosition>> } => x.pos !== null)

  // 2. Calculate vertical positions
  const itemsWithLayout: { item: BodyItem, style: React.CSSProperties, startYM: YearMonth }[] = []

  // Sort by start index
  positionedItems.sort((a, b) => {
    if (a.pos.startIdx !== b.pos.startIdx) return a.pos.startIdx - b.pos.startIdx
    return b.pos.width - a.pos.width
  })

  const placedRects: { startIdx: number, endIdx: number, top: number, height: number }[] = []

  for (const { item, pos } of positionedItems) {
    const height = item.heightPx ?? 24 // Default height
    const gap = 4 // Vertical gap

    let top = 2 // Initial top padding

    // Find a valid top position
    const horizontalOverlaps = placedRects.filter(r =>
      Math.max(r.startIdx, pos.startIdx) <= Math.min(r.endIdx, pos.endIdx)
    )

    const candidates = [2, ...horizontalOverlaps.map(r => r.top + r.height + gap)].sort((a, b) => a - b)

    let bestTop = candidates[0]
    for (const cand of candidates) {
      const collision = horizontalOverlaps.some(r =>
        Math.max(r.top, cand) < Math.min(r.top + r.height, cand + height)
      )
      if (!collision) {
        bestTop = cand
        break
      }
    }
    top = bestTop

    placedRects.push({
      startIdx: pos.startIdx,
      endIdx: pos.endIdx,
      top,
      height
    })

    itemsWithLayout.push({
      item,
      startYM: pos.startYM,
      style: {
        left: `${pos.left}%`,
        width: `${pos.width}%`,
        top: `${top}px`,
        height: `${height}px`,
        position: 'absolute',
        zIndex: 10,
        overflow: 'hidden',
        whiteSpace: 'nowrap'
      }
    })
  }

  const totalHeight = Math.max(32, ...placedRects.map(r => r.top + r.height + 2)) // Min height 32 (h-8)

  return { totalHeight, itemsWithLayout }
}

type ColumnMeta = {
  sticky?: 'left' | 'right'
}
