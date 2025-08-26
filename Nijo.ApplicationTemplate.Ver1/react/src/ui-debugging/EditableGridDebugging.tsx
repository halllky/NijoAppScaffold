import * as React from "react"
import * as ReactHookForm from "react-hook-form"
import { EditableGrid, EditableGridRef } from "../layout/EditableGrid"
import { GetColumnDefsFunction, RowChangeEvent } from "../layout/EditableGrid/types"
import type { ColumnDefFactories } from "../layout/EditableGrid/useCellTypes"
import { CellContext, HeaderContext } from "@tanstack/react-table"

// テスト用のデータ型定義
type BasicRowData = {
  id: number
  name: string
  age: number
  email: string
  isActive: boolean
  joinDate: string
  notes: string
  category: string
  progress: number
}

type ProductRowData = {
  id: number
  productName: string
  price: number
  category: string
  inStock: boolean
  description: string
}

export default function EditableGridDebugging() {
  const [activeSection, setActiveSection] = React.useState<string>('basic')

  return (
    <div className="w-full h-full flex flex-col gap-4 p-4">

      <h1 className="text-2xl font-bold">EditableGrid デバッグ画面</h1>

      {/* ナビゲーション */}
      <div className="flex gap-2 border-b border-gray-300 pb-2">
        {[
          { id: 'basic', label: '基本機能' },
          { id: 'advanced', label: '高度な機能' },
          { id: 'readonly', label: '読み取り専用' },
          { id: 'custom', label: 'カスタムレンダリング' },
          { id: 'keyboard', label: 'キーボード操作' },
          { id: 'selection', label: '行選択機能' },
        ].map(section => (
          <button
            key={section.id}
            onClick={() => setActiveSection(section.id)}
            className={`px-4 py-2 rounded ${activeSection === section.id
              ? 'bg-blue-500 text-white'
              : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
              }`}
          >
            {section.label}
          </button>
        ))}
      </div>

      {/* コンテンツエリア */}
      <div className="flex-1 overflow-auto">
        {activeSection === 'basic' && <BasicGridSection />}
        {activeSection === 'advanced' && <AdvancedGridSection />}
        {activeSection === 'readonly' && <ReadOnlyGridSection />}
        {activeSection === 'custom' && <CustomRenderingSection />}
        {activeSection === 'keyboard' && <KeyboardTestSection />}
        {activeSection === 'selection' && <SelectionTestSection />}
      </div>
    </div>
  )
}

// 基本機能テストセクション
function BasicGridSection() {
  const [rows, setRows] = React.useState<BasicRowData[]>([
    {
      id: 1,
      name: "田中太郎",
      age: 30,
      email: "tanaka@example.com",
      isActive: true,
      joinDate: "2023-01-15",
      notes: "営業部のリーダー",
      category: "sales",
      progress: 75
    },
    {
      id: 2,
      name: "佐藤花子",
      age: 28,
      email: "sato@example.com",
      isActive: false,
      joinDate: "2023-03-20",
      notes: "開発チームのエンジニア",
      category: "engineering",
      progress: 90
    },
    {
      id: 3,
      name: "鈴木一郎",
      age: 35,
      email: "suzuki@example.com",
      isActive: true,
      joinDate: "2022-12-01",
      notes: "",
      category: "marketing",
      progress: 45
    }
  ])

  const getColumnDefs: GetColumnDefsFunction<BasicRowData> = React.useCallback((cellType) => [
    cellType.number('id', 'ID', { defaultWidth: 80, isReadOnly: true }),
    cellType.text('name', '名前', { defaultWidth: 150, required: true }),
    cellType.number('age', '年齢', { defaultWidth: 100 }),
    cellType.text('email', 'メールアドレス', { defaultWidth: 200 }),
    cellType.boolean('isActive', 'アクティブ', { defaultWidth: 100 }),
    cellType.date('joinDate', '入社日', { defaultWidth: 140 }),
    cellType.text('notes', '備考', {
      defaultWidth: 200,
      editorOverflow: 'vertical' as const
    }),
    cellType.text('category', 'カテゴリ', {
      defaultWidth: 130,
      getOptions: () => [
        { value: 'sales', label: '営業' },
        { value: 'engineering', label: '開発' },
        { value: 'marketing', label: 'マーケティング' },
        { value: 'hr', label: '人事' }
      ]
    }),
    cellType.number('progress', '進捗', { defaultWidth: 100 })
  ], [])

  const handleRowChange: RowChangeEvent<BasicRowData> = React.useCallback((e) => {
    const newRows = [...rows]
    e.changedRows.forEach((changedRow) => {
      newRows[changedRow.rowIndex] = changedRow.newRow
    })
    setRows(newRows)
    console.log('行が変更されました:', e.changedRows)
  }, [rows])

  const addRow = () => {
    const newRow: BasicRowData = {
      id: Math.max(...rows.map(r => r.id), 0) + 1,
      name: "",
      age: 25,
      email: "",
      isActive: true,
      joinDate: new Date().toISOString().split('T')[0],
      notes: "",
      category: "",
      progress: 0
    }
    setRows([...rows, newRow])
  }

  const removeLastRow = () => {
    if (rows.length > 0) {
      setRows(rows.slice(0, -1))
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <h2 className="text-xl font-semibold">基本機能テスト</h2>

      <div className="flex gap-2">
        <button
          onClick={addRow}
          className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
        >
          行を追加
        </button>
        <button
          onClick={removeLastRow}
          className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
          disabled={rows.length === 0}
        >
          最後の行を削除
        </button>
      </div>

      <div className="border border-gray-300 rounded">
        <EditableGrid
          rows={rows}
          getColumnDefs={getColumnDefs}
          onChangeRow={handleRowChange}
          onActiveCellChanged={(cell) => console.log('アクティブセルが変更されました:', cell)}
          showCheckBox={true}
          className="w-full h-64"
          showHorizontalBorder={true}
        />
      </div>

      <div className="text-sm text-gray-600">
        <p>💡 テスト項目:</p>
        <ul className="list-disc list-inside ml-4">
          <li>各種セル型（テキスト、数値、日付、真偽値）の編集</li>
          <li>必須マーク表示</li>
          <li>読み取り専用セル</li>
          <li>オプション付きセル（カテゴリ列）</li>
          <li>エディタのオーバーフロー設定（備考列）</li>
          <li>行の追加・削除</li>
        </ul>
      </div>
    </div>
  )
}

// 高度な機能テストセクション
function AdvancedGridSection() {
  const [rows, setRows] = React.useState<ProductRowData[]>([
    {
      id: 1,
      productName: "ノートパソコン",
      price: 120000,
      category: "electronics",
      inStock: true,
      description: "高性能なビジネス向けノートパソコン"
    },
    {
      id: 2,
      productName: "マウス",
      price: 3000,
      category: "electronics",
      inStock: false,
      description: "ワイヤレスマウス"
    }
  ])

  const getColumnDefs: GetColumnDefsFunction<ProductRowData> = React.useCallback((cellType) => [
    cellType.number('id', 'ID', {
      defaultWidth: 80,
      isFixed: true,
      isReadOnly: true
    }),
    cellType.text('productName', '商品名', {
      defaultWidth: 200,
      required: true,
      isFixed: true
    }),
    cellType.number('price', '価格', {
      defaultWidth: 120,
      renderCell: (context: CellContext<ProductRowData, unknown>) => (
        <div className="text-right font-mono">
          ¥{Number(context.getValue()).toLocaleString()}
        </div>
      )
    }),
    cellType.text('category', 'カテゴリ', {
      defaultWidth: 140,
      getOptions: () => [
        { value: 'electronics', label: '家電' },
        { value: 'clothing', label: '衣類' },
        { value: 'food', label: '食品' },
        { value: 'books', label: '書籍' }
      ]
    }),
    cellType.boolean('inStock', '在庫あり', {
      defaultWidth: 100,
      renderCell: (context: CellContext<ProductRowData, unknown>) => (
        <div className="text-center">
          {context.getValue() ? (
            <span className="text-green-600 font-bold">✓ あり</span>
          ) : (
            <span className="text-red-600">✗ なし</span>
          )}
        </div>
      )
    }),
    cellType.text('description', '説明', {
      defaultWidth: 250,
      editorOverflow: 'vertical' as const
    })
  ], [])

  const handleRowChange: RowChangeEvent<ProductRowData> = React.useCallback((e) => {
    const newRows = [...rows]
    e.changedRows.forEach((changedRow) => {
      newRows[changedRow.rowIndex] = changedRow.newRow
    })
    setRows(newRows)
  }, [rows])

  const addRow = () => {
    const newRow: ProductRowData = {
      id: Math.max(...rows.map(r => r.id), 0) + 1,
      productName: "",
      price: 0,
      category: "",
      inStock: true,
      description: ""
    }
    setRows([...rows, newRow])
  }

  const removeLastRow = () => {
    if (rows.length > 0) {
      setRows(rows.slice(0, -1))
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <h2 className="text-xl font-semibold">高度な機能テスト</h2>

      <div className="flex gap-2">
        <button
          onClick={addRow}
          className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
        >
          商品を追加
        </button>
        <button
          onClick={removeLastRow}
          className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
          disabled={rows.length === 0}
        >
          最後の商品を削除
        </button>
      </div>

      <div className="border border-gray-300 rounded">
        <EditableGrid
          rows={rows}
          getColumnDefs={getColumnDefs}
          onChangeRow={handleRowChange}
          showCheckBox={true}
          className="w-full h-64"
        />
      </div>

      <div className="text-sm text-gray-600">
        <p>💡 テスト項目:</p>
        <ul className="list-disc list-inside ml-4">
          <li>固定列（IDと商品名）</li>
          <li>カスタムセルレンダリング（価格と在庫）</li>
          <li>オプション付きセル</li>
          <li>エディタオーバーフロー</li>
          <li>行の追加・削除機能</li>
        </ul>
      </div>
    </div>
  )
}

// 読み取り専用テストセクション
function ReadOnlyGridSection() {
  const [rows, setRows] = React.useState<BasicRowData[]>([
    {
      id: 1,
      name: "田中太郎",
      age: 30,
      email: "tanaka@example.com",
      isActive: true,
      joinDate: "2023-01-15",
      notes: "このデータは一部読み取り専用です",
      category: "sales",
      progress: 75
    }
  ])

  const [isGridReadOnly, setIsGridReadOnly] = React.useState(false)

  const getColumnDefs: GetColumnDefsFunction<BasicRowData> = React.useCallback((cellType) => [
    cellType.number('id', 'ID', { defaultWidth: 80, isReadOnly: true }),
    cellType.text('name', '名前', { defaultWidth: 150 }),
    cellType.number('age', '年齢', { defaultWidth: 100 }),
    cellType.text('email', 'メール', {
      defaultWidth: 200,
      isReadOnly: (row: BasicRowData) => row.id === 1 // ID=1の行は読み取り専用
    }),
    cellType.boolean('isActive', 'アクティブ', { defaultWidth: 100 }),
    cellType.date('joinDate', '入社日', { defaultWidth: 140, isReadOnly: true }),
    cellType.text('notes', '備考', { defaultWidth: 200 })
  ], [])

  const handleRowChange = React.useCallback((e: any) => {
    const newRows = [...rows]
    e.changedRows.forEach((changedRow: any) => {
      newRows[changedRow.rowIndex] = changedRow.newRow
    })
    setRows(newRows)
  }, [rows])

  return (
    <div className="flex flex-col gap-4">
      <h2 className="text-xl font-semibold">読み取り専用テスト</h2>

      <div className="flex gap-2">
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={isGridReadOnly}
            onChange={(e) => setIsGridReadOnly(e.target.checked)}
          />
          <span>グリッド全体を読み取り専用にする</span>
        </label>
      </div>

      <div className="border border-gray-300 rounded">
        <EditableGrid
          rows={rows}
          getColumnDefs={getColumnDefs}
          onChangeRow={handleRowChange}
          isReadOnly={isGridReadOnly}
          showCheckBox={true}
          className="w-full"
        />
      </div>

      <div className="text-sm text-gray-600">
        <p>💡 テスト項目:</p>
        <ul className="list-disc list-inside ml-4">
          <li>常に読み取り専用の列（IDと入社日）</li>
          <li>条件付きで読み取り専用の列（メール - ID=1の行のみ）</li>
          <li>グリッド全体の読み取り専用モード</li>
        </ul>
      </div>
    </div>
  )
}

// カスタムレンダリングテストセクション
function CustomRenderingSection() {
  const [rows, setRows] = React.useState<BasicRowData[]>([
    {
      id: 1,
      name: "田中太郎",
      age: 30,
      email: "tanaka@example.com",
      isActive: true,
      joinDate: "2023-01-15",
      notes: "カスタムレンダリングのテスト",
      category: "high",
      progress: 75
    },
    {
      id: 2,
      name: "佐藤花子",
      age: 28,
      email: "sato@example.com",
      isActive: false,
      joinDate: "2023-03-20",
      notes: "UI/UXデザイナー",
      category: "medium",
      progress: 90
    }
  ])

  const getColumnDefs: GetColumnDefsFunction<BasicRowData> = React.useCallback((cellType) => [
    {
      ...cellType.text('name', '名前', {
        defaultWidth: 150,
        renderCell: (context: CellContext<BasicRowData, unknown>) => (
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center text-white text-sm font-bold">
              {context.getValue()?.toString().charAt(0) || '?'}
            </div>
            <span>{context.getValue() as string}</span>
          </div>
        )
      }),
      header: ctx => (
        <div className="flex items-center gap-2 text-blue-700">
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clipRule="evenodd" />
          </svg>
          <span className="font-semibold">👤 名前</span>
        </div>
      )
    },
    {
      ...cellType.number('age', '年齢', {
        defaultWidth: 100,
        renderCell: (context: any) => {
          const age = context.getValue() as number
          const color = age >= 30 ? 'text-purple-600' : 'text-green-600'
          return <span className={`font-semibold ${color}`}>{age}歳</span>
        }
      }),
      header: ctx => (
        <div className="flex items-center text-green-700">
          <span className="text-lg">🎂</span>
          <span className="text-xs font-medium">年齢</span>
        </div>
      )
    },
    {
      ...cellType.text('category', '優先度', {
        defaultWidth: 120,
        getOptions: () => [
          { value: 'low', label: '低' },
          { value: 'medium', label: '中' },
          { value: 'high', label: '高' },
          { value: 'urgent', label: '緊急' }
        ],
        renderCell: (context: any) => {
          const value = context.getValue() as string
          const styles = {
            low: 'bg-gray-100 text-gray-700',
            medium: 'bg-blue-100 text-blue-700',
            high: 'bg-orange-100 text-orange-700',
            urgent: 'bg-red-100 text-red-700'
          }
          const labels = {
            low: '低',
            medium: '中',
            high: '高',
            urgent: '緊急'
          }
          return (
            <span className={`px-2 py-1 rounded-full text-xs font-semibold ${styles[value as keyof typeof styles] || styles.low}`}>
              {labels[value as keyof typeof labels] || value}
            </span>
          )
        }
      }),
      header: ctx => (
        <div className="flex items-center justify-center">
          <div className="bg-gradient-to-r from-red-500 to-yellow-500 text-white px-2 py-1 rounded text-xs font-bold">
            ⚡ 優先度
          </div>
        </div>
      )
    },
    {
      ...cellType.number('progress', '進捗', {
        defaultWidth: 150,
        renderCell: (context: any) => {
          const value = Number(context.getValue()) || 0
          return (
            <div className="w-full">
              <div className="flex justify-between text-xs mb-1">
                <span>{value}%</span>
                <span className={value >= 80 ? 'text-green-600' : value >= 50 ? 'text-yellow-600' : 'text-red-600'}>
                  {value >= 80 ? '良好' : value >= 50 ? '普通' : '要改善'}
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full transition-all ${value >= 80 ? 'bg-green-500' : value >= 50 ? 'bg-yellow-500' : 'bg-red-500'
                    }`}
                  style={{ width: `${Math.min(100, Math.max(0, value))}%` }}
                />
              </div>
            </div>
          )
        }
      }),
      header: ctx => (
        <div className="flex gap-1">
          <div className="flex items-center justify-center gap-1 text-purple-700">
            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
            </svg>
            <span className="font-semibold">進捗</span>
          </div>
          <div className="text-xs text-gray-500 mt-1">Completion</div>
        </div>
      )
    },
    {
      ...cellType.boolean('isActive', 'ステータス', {
        defaultWidth: 100,
        renderCell: (context: any) => (
          <div className="flex items-center justify-center">
            <div className={`w-3 h-3 rounded-full ${context.getValue() ? 'bg-green-500' : 'bg-gray-400'}`} />
            <span className="ml-2 text-sm">
              {context.getValue() ? 'アクティブ' : '非アクティブ'}
            </span>
          </div>
        )
      }),
      header: ctx => (
        <div className="text-center">
          <div className="inline-flex items-center gap-1 bg-green-100 text-green-800 px-2 py-1 rounded-full text-xs">
            <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
            <span className="font-medium">ステータス</span>
          </div>
        </div>
      )
    }
  ], [])

  const handleRowChange = React.useCallback((e: any) => {
    const newRows = [...rows]
    e.changedRows.forEach((changedRow: any) => {
      newRows[changedRow.rowIndex] = changedRow.newRow
    })
    setRows(newRows)
  }, [rows])

  return (
    <div className="flex flex-col gap-4">
      <h2 className="text-xl font-semibold">カスタムレンダリングテスト</h2>

      <div className="border border-gray-300 rounded">
        <EditableGrid
          rows={rows}
          getColumnDefs={getColumnDefs}
          onChangeRow={handleRowChange}
          className="w-full"
        />
      </div>

      <div className="text-sm text-gray-600">
        <p>💡 テスト項目:</p>
        <ul className="list-disc list-inside ml-4">
          <li>カスタム列ヘッダーレンダリング（アイコン、絵文字、グラデーション、アニメーション）</li>
          <li>アバター付き名前表示</li>
          <li>条件付きスタイル（年齢）</li>
          <li>優先度バッジ表示</li>
          <li>プログレスバー表示</li>
          <li>ステータスインジケーター</li>
        </ul>
      </div>
    </div>
  )
}

// キーボード操作テストセクション
function KeyboardTestSection() {
  const [rows, setRows] = React.useState<BasicRowData[]>([
    { id: 1, name: "A1", age: 10, email: "a@example.com", isActive: true, joinDate: "2023-01-01", notes: "Test", category: "test", progress: 10 },
    { id: 2, name: "B2", age: 20, email: "b@example.com", isActive: false, joinDate: "2023-02-01", notes: "Test", category: "test", progress: 20 },
    { id: 3, name: "C3", age: 30, email: "c@example.com", isActive: true, joinDate: "2023-03-01", notes: "Test", category: "test", progress: 30 }
  ])

  const [keyboardEvents, setKeyboardEvents] = React.useState<string[]>([])
  const gridRef = React.useRef<EditableGridRef<BasicRowData>>(null)

  const getColumnDefs: GetColumnDefsFunction<BasicRowData> = React.useCallback((cellType) => [
    cellType.text('name', '名前', { defaultWidth: 150 }),
    cellType.number('age', '年齢', { defaultWidth: 100 }),
    cellType.text('email', 'メール', { defaultWidth: 200 }),
    cellType.boolean('isActive', 'アクティブ', { defaultWidth: 100 })
  ], [])

  const handleRowChange = React.useCallback((e: any) => {
    const newRows = [...rows]
    e.changedRows.forEach((changedRow: any) => {
      newRows[changedRow.rowIndex] = changedRow.newRow
    })
    setRows(newRows)
  }, [rows])

  const handleKeyDown = React.useCallback((nativeEvent: React.KeyboardEvent, isEditing: boolean) => {
    const eventLog = `${nativeEvent.key} (editing: ${isEditing})`
    setKeyboardEvents(prev => [...prev.slice(-9), eventLog])

    // カスタムキーボード処理の例
    if (nativeEvent.key === 'Delete' && !isEditing) {
      const activeCell = gridRef.current?.getActiveCell()
      if (activeCell) {
        console.log('Deleteキーが押されました:', activeCell)
      }
      return { handled: false }
    }

    return { handled: false }
  }, [])

  const getSelectedInfo = () => {
    if (!gridRef.current) return null
    const selectedRows = gridRef.current.getSelectedRows()
    const activeCell = gridRef.current.getActiveCell()
    const selectedRange = gridRef.current.getSelectedRange()

    return { selectedRows, activeCell, selectedRange }
  }

  return (
    <div className="flex flex-col gap-4">
      <h2 className="text-xl font-semibold">キーボード操作テスト</h2>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <h3 className="text-lg font-medium mb-2">グリッド</h3>
          <div className="border border-gray-300 rounded">
            <EditableGrid
              ref={gridRef}
              rows={rows}
              getColumnDefs={getColumnDefs}
              onChangeRow={handleRowChange}
              onKeyDown={handleKeyDown}
              className="w-full"
            />
          </div>
        </div>

        <div>
          <h3 className="text-lg font-medium mb-2">キーボードイベントログ</h3>
          <div className="border border-gray-300 rounded p-2 h-40 overflow-y-auto bg-gray-50">
            {keyboardEvents.map((event, index) => (
              <div key={index} className="text-sm font-mono">
                {event}
              </div>
            ))}
          </div>

          <button
            onClick={() => console.log('選択情報:', getSelectedInfo())}
            className="mt-2 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
          >
            選択情報をコンソールに出力
          </button>
        </div>
      </div>

      <div className="text-sm text-gray-600">
        <p>💡 テスト項目:</p>
        <ul className="list-disc list-inside ml-4">
          <li>矢印キーでのセル移動</li>
          <li>Enter/Escapeでの編集開始・終了</li>
          <li>Tab/Shift+Tabでの移動</li>
          <li>Ctrl+C/Ctrl+Vでのコピーペースト</li>
          <li>キーボードイベントのカスタムハンドリング</li>
        </ul>
      </div>
    </div>
  )
}

// 行選択機能テストセクション
function SelectionTestSection() {
  const [rows, setRows] = React.useState<BasicRowData[]>([
    { id: 1, name: "田中太郎", age: 30, email: "tanaka@example.com", isActive: true, joinDate: "2023-01-15", notes: "", category: "sales", progress: 75 },
    { id: 2, name: "佐藤花子", age: 28, email: "sato@example.com", isActive: false, joinDate: "2023-03-20", notes: "", category: "engineering", progress: 90 },
    { id: 3, name: "鈴木一郎", age: 35, email: "suzuki@example.com", isActive: true, joinDate: "2022-12-01", notes: "", category: "marketing", progress: 45 },
    { id: 4, name: "高橋美咲", age: 32, email: "takahashi@example.com", isActive: true, joinDate: "2023-05-10", notes: "", category: "hr", progress: 60 }
  ])

  const [currentSelectionInfo, setCurrentSelectionInfo] = React.useState<string>('')
  const gridRef = React.useRef<EditableGridRef<BasicRowData>>(null)

  const getColumnDefs = React.useCallback((cellType: any) => [
    cellType.number('id', 'ID', { defaultWidth: 80 }),
    cellType.text('name', '名前', { defaultWidth: 150 }),
    cellType.number('age', '年齢', { defaultWidth: 100 }),
    cellType.text('email', 'メール', { defaultWidth: 200 }),
    cellType.boolean('isActive', 'アクティブ', { defaultWidth: 100 })
  ], [])

  const handleRowChange = React.useCallback((e: any) => {
    const newRows = [...rows]
    e.changedRows.forEach((changedRow: any) => {
      newRows[changedRow.rowIndex] = changedRow.newRow
    })
    setRows(newRows)
  }, [rows])

  const selectSpecificRows = () => {
    if (gridRef.current) {
      gridRef.current.selectRow(1, 2) // 2行目から3行目を選択
    }
  }

  const getSelectedRowsInfo = () => {
    if (gridRef.current) {
      const selected = gridRef.current.getSelectedRows()
      const checked = gridRef.current.getCheckedRows()
      console.log('選択された行（範囲選択）:', selected)
      console.log('チェックされた行（チェックボックス）:', checked)

      // 現在の選択状態を表示用に更新
      const checkedIndices = checked.map(item => item.rowIndex)
      const selectedIndices = selected.map(item => item.rowIndex)

      let info = ''
      if (checkedIndices.length > 0) {
        info += `チェック選択: ${checkedIndices.length}件 (${checkedIndices.join(', ')}行目)`
      }
      if (selectedIndices.length > 0) {
        if (info) info += ' | '
        info += `範囲選択: ${selectedIndices.length}件 (${selectedIndices.join(', ')}行目)`
      }
      if (!info) {
        info = '選択なし'
      }

      setCurrentSelectionInfo(info)
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <h2 className="text-xl font-semibold">行選択機能テスト</h2>

      <div className="flex gap-2 flex-wrap">
        <button
          onClick={selectSpecificRows}
          className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
        >
          2-3行目を選択
        </button>
        <button
          onClick={getSelectedRowsInfo}
          className="px-4 py-2 bg-purple-500 text-white rounded hover:bg-purple-600"
        >
          選択情報を確認・表示
        </button>
      </div>

      {currentSelectionInfo && (
        <div className="p-2 bg-blue-50 border border-blue-200 rounded text-sm">
          {currentSelectionInfo}
        </div>
      )}

      <div className="border border-gray-300 rounded">
        <EditableGrid
          ref={gridRef}
          rows={rows}
          getColumnDefs={getColumnDefs}
          onChangeRow={handleRowChange}
          showCheckBox={true}
          className="w-full"
        />
      </div>

      <div className="text-sm text-gray-600">
        <p>💡 テスト項目:</p>
        <ul className="list-disc list-inside ml-4">
          <li>チェックボックスによる行選択（EditableGrid内部で管理）</li>
          <li>プログラムによる行選択（refを通した操作）</li>
          <li>複数行の範囲選択（マウスドラッグ）</li>
          <li>選択状態の取得（refを通した情報取得）</li>
        </ul>
        <p className="mt-2 text-xs text-gray-500">
          💡 Tips: チェックボックスをクリックしてから「選択情報を確認・表示」ボタンを押すと、選択状態が表示されます。
        </p>
      </div>
    </div>
  )
}
