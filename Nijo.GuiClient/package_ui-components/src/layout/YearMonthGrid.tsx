
import React, { useMemo } from 'react'

/** YYYY/MM 形式の文字列 */
export type YearMonth = `${number}/${number}`

export type GridProps<TRow extends Row<TItem>, TItem extends BodyItem> = {
  /** 表示範囲の始点 */
  since: YearMonth
  /** 表示範囲の終点 */
  until: YearMonth
  /** グリッドの各行 */
  rows?: TRow[]
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
}

/** カレンダーの横1行分のオブジェクトが備えるべき属性 */
export type Row<TItem extends BodyItem> = {
  /** カレンダーのボディ部分に表示されるアイテム */
  bodyItems: TItem[]
}

/** カレンダーのボディ部分に表示されるアイテムが備えるべき属性 */
export type BodyItem =
  | { type: 'point', yearMonth: YearMonth }
  | { type: 'range', since: YearMonth, until: YearMonth }

// ----------------------------------------
// イベント引数型

/** カレンダー左右の固定列のレンダリング処理の引数 */
export type FixedColumnRenderer<TRow extends Row<TItem>, TItem extends BodyItem> = {
  header: React.ReactNode
  body: (props: { row: TRow, rowIndex: number }) => React.ReactNode
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
    rows,
    renderBodyItem,
    renderLeftColumns,
    renderRightColumns,
    onBodyItemClick,
    onBodyItemDoubleClick,
    onBodyClick,
    onBodyDoubleClick,
  } = props

  const months = useMemo(() => getMonthsList(since, until), [since, until])
  const yearGroups = useMemo(() => getYearGroups(months), [months])

  return (
    <div style={{ overflow: 'auto', maxWidth: '100%', maxHeight: '100%' }}>
      <table style={{ borderCollapse: 'separate', borderSpacing: 0 }}>
        <thead>
          {/* Years Header */}
          <tr>
            {renderLeftColumns?.map((_, i) => (
              <th key={`left-header-empty-${i}`} style={{ position: 'sticky', left: 0, zIndex: 3, background: '#f0f0f0', borderBottom: '1px solid #ccc' }} />
            ))}
            {yearGroups.map(group => (
              <th
                key={group.year}
                colSpan={group.months.length}
                style={{ borderLeft: '1px solid #ccc', borderBottom: '1px solid #ccc', textAlign: 'center', background: '#f9f9f9' }}
              >
                {group.year}
              </th>
            ))}
            {renderRightColumns?.map((_, i) => (
              <th key={`right-header-empty-${i}`} style={{ position: 'sticky', right: 0, zIndex: 3, background: '#f0f0f0', borderBottom: '1px solid #ccc' }} />
            ))}
          </tr>
          {/* Months Header */}
          <tr>
            {renderLeftColumns?.map((col, i) => (
              <th key={`left-header-${i}`} style={{ position: 'sticky', left: 0, zIndex: 3, background: '#f0f0f0', borderBottom: '1px solid #ccc', borderRight: '1px solid #ccc' }}>
                {col.header}
              </th>
            ))}
            {months.map(ym => (
              <th
                key={ym}
                style={{ minWidth: '40px', borderLeft: '1px solid #eee', borderBottom: '1px solid #ccc', textAlign: 'center', fontSize: '0.8em', background: '#fff' }}
              >
                {parseYearMonth(ym).month}
              </th>
            ))}
            {renderRightColumns?.map((col, i) => (
              <th key={`right-header-${i}`} style={{ position: 'sticky', right: 0, zIndex: 3, background: '#f0f0f0', borderBottom: '1px solid #ccc', borderLeft: '1px solid #ccc' }}>
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows?.map((row, rowIndex) => (
            <tr key={rowIndex}>
              {/* Left Fixed Columns */}
              {renderLeftColumns?.map((col, colIndex) => (
                <td
                  key={`left-${rowIndex}-${colIndex}`}
                  style={{ position: 'sticky', left: 0, zIndex: 2, background: '#fff', borderBottom: '1px solid #eee', borderRight: '1px solid #ccc' }}
                >
                  {col.body({ row, rowIndex })}
                </td>
              ))}

              {/* Timeline Body */}
              <td colSpan={months.length} style={{ padding: 0, position: 'relative', height: '30px', borderBottom: '1px solid #eee' }}>
                <div style={{ position: 'relative', width: '100%', height: '100%', display: 'flex' }}>
                  {/* Background Grid */}
                  {months.map(ym => (
                    <div
                      key={ym}
                      style={{ flex: 1, borderLeft: '1px solid #eee', height: '100%', boxSizing: 'border-box' }}
                      onClick={() => onBodyClick?.({ row, rowIndex, yearMonth: ym })}
                      onDoubleClick={() => onBodyDoubleClick?.({ row, rowIndex, yearMonth: ym })}
                    />
                  ))}

                  {/* Items */}
                  {row.bodyItems.map((item, itemIndex) => {
                    const position = calculateItemPosition(item, months)
                    if (!position) return null
                    return (
                      <div
                        key={itemIndex}
                        style={{
                          position: 'absolute',
                          left: `${position.left}%`,
                          width: `${position.width}%`,
                          top: '2px',
                          bottom: '2px',
                          zIndex: 1,
                          overflow: 'hidden',
                          whiteSpace: 'nowrap',
                        }}
                        onClick={(e) => {
                          e.stopPropagation()
                          onBodyItemClick?.({ row, rowIndex, item, yearMonth: position.startYM })
                        }}
                        onDoubleClick={(e) => {
                          e.stopPropagation()
                          onBodyItemDoubleClick?.({ row, rowIndex, item, yearMonth: position.startYM })
                        }}
                      >
                        {renderBodyItem({ row, rowIndex, item })}
                      </div>
                    )
                  })}
                </div>
              </td>

              {/* Right Fixed Columns */}
              {renderRightColumns?.map((col, colIndex) => (
                <td
                  key={`right-${rowIndex}-${colIndex}`}
                  style={{ position: 'sticky', right: 0, zIndex: 2, background: '#fff', borderBottom: '1px solid #eee', borderLeft: '1px solid #ccc' }}
                >
                  {col.body({ row, rowIndex })}
                </td>
              ))}
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

function calculateItemPosition(item: BodyItem, months: YearMonth[]): { left: number, width: number, startYM: YearMonth } | null {
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

  return { left, width, startYM: effectiveStart as YearMonth }
}
