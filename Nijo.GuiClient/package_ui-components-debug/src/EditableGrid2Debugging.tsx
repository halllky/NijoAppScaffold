import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import React from "react"
import { useFieldArray, useForm, Control, useWatch } from "react-hook-form"

export default function () {

  const gridRef = React.useRef<EG2.EditableGrid2Ref<TestRow>>(null)

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

  const { reset, watch, control, setValue, getValues } = useForm<{ rows: TestRow[] }>()
  const rows = watch("rows")
    || []

  // 行データを安全に更新するヘルパー関数
  const ufa = useFieldArray({ name: "rows", control })
  const ufaRef = React.useRef(ufa)
  ufaRef.current = ufa
  const updateRow = React.useCallback((index: number, update: (current: TestRow) => TestRow) => {
    ufaRef.current.update(index, update(ufaRef.current.fields[index]))
  }, [ufaRef])

  const getRowValue = React.useCallback((index: number) => {
    return getValues(`rows.${index}`)
  }, [getValues])

  React.useEffect(() => {
    console.log('fields', ufa.fields)
  }, [ufa.fields])
  React.useEffect(() => {
    console.log('rows: ', rows)
  }, [watch("rows")])
  React.useEffect(() => {
    console.log('rows.0: ', rows[0])
  }, [watch("rows.0")])
  React.useEffect(() => {
    console.log('rows.0.name: ', rows[0]?.name)
    console.log(ufa.fields[0]?.id)
  }, [watch("rows.0.name")])

  React.useEffect(() => {
    let rows: TestRow[]
    if (isLargeData) {
      rows = Array.from({ length: 1000 }).map((_, i) => ({
        rowId: i + 1,
        name: `商品${i + 1}`,
        price: ((i + 1) * 1000).toString(),
        date: `2024-${String((i % 12) + 1).padStart(2, '0')}-01`,
        comment: `コメント${i + 1}`,
        bool: i % 2 === 0,
      }))
    } else {
      rows = [
        { rowId: 1, name: "商品A", price: "1000", date: "2024-01-01", comment: "コメントA", bool: true },
        { rowId: 2, name: "商品B", price: "2000", date: "2024-02-01", comment: "コメントB", bool: false },
        { rowId: 3, name: "商品C", price: "3000", date: "2024-03-01", comment: "コメントC", bool: true },
      ]
    }
    reset({ rows })
  }, [isLargeData])

  return (
    <div className="flex flex-col items-start">
      <EG2.EditableGrid2
        ref={gridRef}
        rows={ufa.fields}
        getRowId={row => row.rowId?.toString() ?? ''}
        getRowValue={getRowValue}
        columns={[() => [
          buttonCell(row => row.willBeDeleted ? "復元" : "削除", (row, rowIndex) => {
            updateRow(rowIndex, r => ({ ...r, willBeDeleted: !r.willBeDeleted }))
          }, { isFixed: fixed3Cols, disableResizing: true, defaultWidth: 40 }),
          textCell("ID", r => r.rowId?.toString(), () => {/* 読み取り専用なので何もしない */ }, { defaultWidth: 80, isReadOnly: true, isFixed: fixed3Cols }),

          // textCell("商品名", r => r.name, (rowIndex, value) => updateRow(rowIndex, r => ({ ...r, name: value })), { isFixed: fixed3Cols }),
          textCell("商品名", r => r.name, (rowIndex, value) => setValue(`rows.${rowIndex}.name`, value), { isFixed: fixed3Cols, rhfControl: control, rhfName: (i) => `rows.${i}.name` }),

          textCell("価格", r => r.price, (rowIndex, value) => setValue(`rows.${rowIndex}.price`, value), { defaultWidth: 120, rhfControl: control, rhfName: (i) => `rows.${i}.price` }),
          textCell("日付", r => r.date, (rowIndex, value) => setValue(`rows.${rowIndex}.date`, value), { defaultWidth: 140, rhfControl: control, rhfName: (i) => `rows.${i}.date` }),
          textCell("フラグ", r => r.bool ? "✔" : "", (rowIndex, value) => setValue(`rows.${rowIndex}.bool`, value === "✔"), { defaultWidth: 80, rhfControl: control, rhfName: (i) => `rows.${i}.bool` }),
          textCell("コメント", r => r.comment, (rowIndex, value) => setValue(`rows.${rowIndex}.comment`, value), { defaultWidth: 320, editorOverflow: 'vertical', rhfControl: control, rhfName: (i) => `rows.${i}.comment` }),
        ], [fixed3Cols, updateRow]]}

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
          if (rows.length > 0) {
            setValue(`rows.0.comment`, `コメントをプログラムから更新しました: ${new Date().toLocaleString()}`)
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
          ufa.insert(0, { rowId: undefined, name: null, price: null, date: null, comment: null, bool: false })
        }} className="px-2 py-1 text-white bg-purple-600 border border-white cursor-pointer">
          先頭に追加
        </button>
        <button type="button" onClick={() => {
          ufa.append({ rowId: undefined, name: null, price: null, date: null, comment: null, bool: false })
        }} className="px-2 py-1 text-white bg-green-600 border border-white cursor-pointer">
          行追加
        </button>
        <button type="button" onClick={() => {
          const checkedRows = gridRef.current?.getCheckedRows() || []
          const removeRowIndexes = checkedRows.map(r => r.rowIndex)
          ufa.remove(removeRowIndexes)
        }} className="px-2 py-1 text-white bg-red-600 border border-white cursor-pointer">
          選択した行を削除
        </button>
      </div>
    </div>
  )
}

type TestRow = {
  rowId?: number | null
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
  getValue: (row: TestRow) => string | null | undefined,
  setValue: (rowIndex: number, value: string) => void,
  options?: Partial<EG2.EditableGrid2LeafColumn<TestRow>> & { rhfControl?: Control<any>, rhfName?: (index: number) => string }
): EG2.EditableGrid2LeafColumn<TestRow> {

  return {
    renderHeader: () => (
      <div className="px-1 py-px truncate text-gray-700">
        {header}
      </div>
    ),
    renderBody: (cell) => {
      if (options?.rhfControl && options?.rhfName) {
        return <RHFTextCell control={options.rhfControl} name={options.rhfName(cell.row.index)} />
      }
      return (
        <div className="px-1 py-px truncate">
          {getValue(cell.row.original)}
        </div>
      )
    },
    getValueForEditor: ({ row }) => getValue(row) ?? '',
    setValueFromEditor: ({ rowIndex, value }) => setValue(rowIndex, value),
    ...options,
  }
}

const RHFTextCell = ({ control, name }: { control: Control<any>, name: string }) => {
  const value = useWatch({ control, name })
  return <div className="px-1 py-px truncate">{value as string}</div>
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
