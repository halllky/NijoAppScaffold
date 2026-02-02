import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import React from "react"
import { useForm } from "react-hook-form"
import { UUID } from "uuidjs"

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

  // フォーカスアウトで選択解除するかどうか
  const [clearSelectionOnBlur, setClearSelectionOnBlur] = React.useState(true)
  const handleChangeClearSelectionOnBlur = (e: React.ChangeEvent<HTMLInputElement>) => {
    setClearSelectionOnBlur(e.target.checked)
  }

  const { control, setValue, getValues } = useForm<{ rows: TestRow[] }>()

  // useFieldArrayForEditableGrid2 フックの使用
  const {
    fieldArrayReturn: { fields, append, insert, remove, replace },
    editableGrid2Props,
    gridRef,
  } = EG2.useFieldArrayForEditableGrid2({
    name: "rows", control, getValues, setValue
  }, (helper) => [
    helper.buttonCell(
      row => row.willBeDeleted ? "復元" : "無効化",
      (row, rowIndex) => setValue(`rows.${rowIndex}.willBeDeleted`, !row.willBeDeleted),
      { isFixed: fixed3Cols, disableResizing: true, defaultWidth: 56 }
    ),

    helper.textCell("ID", "rowId", { defaultWidth: 80, isReadOnly: true, isFixed: fixed3Cols }),
    helper.textCell("商品名", "name", { isFixed: fixed3Cols }),
    helper.textCell("価格", "price", { defaultWidth: 120 }),
    {
      renderHeader: () => <span className="px-1 text-gray-700">グルーピングされた列</span>,
      columns: [
        helper.textCell("日付", "date", { defaultWidth: 140 }),
        helper.booleanCell("フラグ", "bool", { defaultWidth: 80 }),
      ],
    },
    helper.textCell("コメント", "comment", { defaultWidth: 320, wrap: true }),
    helper.textCell("価格(同じ項目を複数回指定する例)", "price", { defaultWidth: 252 }),
  ], [fixed3Cols])

  React.useEffect(() => {
    let rows: TestRow[]
    if (isLargeData) {
      rows = Array.from({ length: 1000 }).map((_, i) => ({
        rowId: (i + 1).toString(),
        name: `商品${i + 1}`,
        price: ((i + 1) * 1000).toString(),
        date: `2024-${String((i % 12) + 1).padStart(2, '0')}-01`,
        comment: i % 10 === 4 ? `折り返しが発生する長いコメントです。折り返しが発生する長いコメントです。折り返しが発生する長いコメントです。\n\nこのコメントは複数行にわたって表示されます。` : `コメント${i + 1}`,
        bool: i % 2 === 0,
      }))
    } else {
      rows = [
        { rowId: "1", name: "商品A", price: "1000", date: "2024-01-01", comment: "コメントA", bool: true },
        { rowId: "2", name: "商品B", price: "2000", date: "2024-02-01", comment: "コメントB", bool: false },
        { rowId: "3", name: "商品C", price: "3000", date: "2024-03-01", comment: "コメントC", bool: true },
      ]
    }
    replace(rows)
  }, [isLargeData, replace])

  return (
    <div className="flex flex-col items-start">
      <EG2.EditableGrid2
        {...editableGrid2Props}
        showCheckBox
        clearSelectionOnBlur={clearSelectionOnBlur}
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

      <label>
        <input type="checkbox" checked={clearSelectionOnBlur} onChange={handleChangeClearSelectionOnBlur} />
        フォーカスアウトで選択解除
      </label>

      <span className="text-sm font-bold mt-4">
        プログラムから特定の値をセットする例
      </span>
      <div>
        <button type="button" onClick={() => {
          if (fields.length > 0) {
            setValue(`rows.0.comment`, `コメントをプログラムから更新しました: ${new Date().toLocaleString()}`)
            // 外部からの更新後は再描画が必要
            gridRef.current?.forceUpdate()
          }
        }} className="px-2 py-1 text-white bg-blue-600 border border-white cursor-pointer">
          先頭行のコメント列を更新
        </button>
      </div>

      <span className="text-sm font-bold mt-4">
        行追加、削除、入れ替え
      </span>
      <div className="flex gap-2">
        <button type="button" onClick={() => {
          insert(0, { rowId: UUID.generate(), name: null, price: null, date: null, comment: null, bool: false })
        }} className="px-2 py-1 text-white bg-purple-600 border border-white cursor-pointer">
          先頭に追加
        </button>
        <button type="button" onClick={() => {
          append({ rowId: UUID.generate(), name: null, price: null, date: null, comment: null, bool: false })
        }} className="px-2 py-1 text-white bg-green-600 border border-white cursor-pointer">
          行追加
        </button>
        <button type="button" onClick={() => {
          const checkedRows = gridRef.current?.getCheckedRows() || []
          const removeRowIndexes = checkedRows.map(r => r.rowIndex)
          remove(removeRowIndexes)
        }} className="px-2 py-1 text-white bg-red-600 border border-white cursor-pointer">
          選択した行を削除
        </button>
      </div>
    </div>
  )
}

type TestRow = {
  rowId: string
  name?: string | null
  price?: string | null
  date?: string | null
  comment?: string | null
  bool?: boolean
  willBeDeleted?: boolean
}
