import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import React from "react"
import { useForm } from "react-hook-form"

export default function () {

  // 商品名まで固定するかどうか
  const [fixed3Cols, setFixed2Cols] = React.useState(true)
  const handleChangeFixedCols = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFixed2Cols(e.target.checked)
  }

  // 大量データにするかどうか
  const [isLargeData, setIsLargeData] = React.useState(true)
  const handleChangeIsLargeData = (e: React.ChangeEvent<HTMLInputElement>) => {
    setIsLargeData(e.target.checked)
  }

  const { reset, watch, control, setValue } = useForm<{ rows: TestRow[] }>()
  const rows = watch("rows") || []

  React.useEffect(() => {
    let rows: TestRow[]
    if (isLargeData) {
      rows = Array.from({ length: 1000 }).map((_, i) => ({
        id: i + 1,
        name: `商品${i + 1}`,
        price: ((i + 1) * 1000).toString(),
        date: `2024-${String((i % 12) + 1).padStart(2, '0')}-01`,
        comment: `コメント${i + 1}`,
        bool: i % 2 === 0,
      }))
    } else {
      rows = [
        { id: 1, name: "商品A", price: "1000", date: "2024-01-01", comment: "コメントA", bool: true },
        { id: 2, name: "商品B", price: "2000", date: "2024-02-01", comment: "コメントB", bool: false },
        { id: 3, name: "商品C", price: "3000", date: "2024-03-01", comment: "コメントC", bool: true },
      ]
    }
    reset({ rows })
  }, [isLargeData])

  return (
    <div className="flex flex-col items-start">
      <EG2.EditableGrid2
        rows={rows}
        columns={[() => [
          buttonCell(row => row.willBeDeleted ? "復元" : "削除", (row, rowIndex) => {
            setValue(`rows.${rowIndex}.willBeDeleted`, row.willBeDeleted ? false : true)
          }, { isFixed: fixed3Cols, defaultWidth: 40 }),
          textCell("ID", r => r.id, { defaultWidth: 80, isReadOnly: true, isFixed: fixed3Cols }),
          textCell("商品名", r => r.name, { isFixed: fixed3Cols }),
          textCell("価格", r => r.price, { defaultWidth: 120 }),
          textCell("日付", r => r.date, { defaultWidth: 140 }),
          textCell("フラグ", r => r.bool ? "✔" : "", { defaultWidth: 80 }),
          textCell("コメント", r => r.comment, { defaultWidth: 480 }),
        ], [fixed3Cols]]}

        showCheckBox
        isReadOnly={row => row.willBeDeleted === true}
        className="h-96 w-1/2 resize"
      />

      <label>
        <input type="checkbox" checked={fixed3Cols} onChange={handleChangeFixedCols} />
        商品名まで固定
      </label>

      <label>
        <input type="checkbox" checked={isLargeData} onChange={handleChangeIsLargeData} />
        大量データ
      </label>
    </div>
  )
}

type TestRow = {
  id?: number | null
  name?: string | null
  price?: string | null
  date?: string | null
  comment?: string | null
  bool?: boolean
  willBeDeleted?: boolean
}

/** 列定義ヘルパー */
function textCell(
  header: string,
  getValue: (row: TestRow) => React.ReactNode,
  options?: Partial<EG2.EditableGrid2LeafColumn<TestRow>>
): EG2.EditableGrid2LeafColumn<TestRow> {

  return {
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">
        {header}
      </div>
    ),
    renderBody: (cell) => (
      <div className="px-1 py-px truncate">
        {getValue(cell.row.original)}
      </div>
    ),
    ...options,
  }
}

function buttonCell(
  text: (row: TestRow, rowIndex: number) => React.ReactNode,
  onClick: (row: TestRow, rowIndex: number) => void,
  options?: Partial<EG2.EditableGrid2LeafColumn<TestRow>>
): EG2.EditableGrid2LeafColumn<TestRow> {

  return {
    renderHeader: () => null,
    renderBody: cell => (
      <button type="button"
        onClick={() => onClick(cell.row.original, cell.row.index)}
        className="w-full h-full text-sm text-white bg-teal-700 border border-white cursor-pointer"
      >
        {text(cell.row.original, cell.row.index)}
      </button>
    ),
    disableResizing: true,
    ...options,
  }
}
