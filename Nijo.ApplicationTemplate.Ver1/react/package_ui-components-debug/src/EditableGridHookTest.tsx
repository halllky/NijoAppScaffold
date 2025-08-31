import * as React from "react"
import { createPortal } from "react-dom"
import { EditableGrid } from "@nijo/ui-components/layout/EditableGrid"
import { GetColumnDefsFunction, RowChangeEvent } from "@nijo/ui-components/layout/EditableGrid/types"

// テスト用のデータ型定義
type OrderRowData = {
  orderTime: string
  quantity: number
  item: {
    itemId: string
    itemName: string
  } | undefined
}

/**
 * 列定義の中でフックを使用するテスト
 */
export default function EditableGridHookTest() {

  // データ
  const [rows, setRows] = React.useState<OrderRowData[]>([
    {
      orderTime: "2024-01-15 09:00",
      quantity: 5,
      item: { itemId: "ITEM001", itemName: "ノートパソコン" }
    },
    {
      orderTime: "2024-01-15 10:30",
      quantity: 10,
      item: { itemId: "ITEM002", itemName: "マウス" }
    },
    {
      orderTime: "2024-01-15 14:15",
      quantity: 3,
      item: { itemId: "ITEM003", itemName: "キーボード" }
    }
  ])

  // 列定義
  const getColumnDefs: GetColumnDefsFunction<OrderRowData> = React.useCallback((cellType) => [
    // 発注時刻列
    cellType.text('orderTime', '発注時刻', {
      defaultWidth: 180,
      required: true
    }),

    // 数量列
    cellType.number('quantity', '数量', {
      defaultWidth: 100,
      required: true
    }),

    // 商品ID（フックを使用）
    {
      ...cellType.other('商品', {
        defaultWidth: 250,
        onStartEditing: ctx => {
          ctx.setEditorInitialValue(ctx.row.item?.itemId ?? '')
        },
        onEndEditing: ctx => {
          const clone = window.structuredClone(ctx.row)
          clone.item = { itemId: ctx.value, itemName: '' }
          ctx.setEditedRow(clone)
        },
        renderCell: ctx => {

          // フックを使用してダイアログの開閉状態を管理
          const [isDialogOpen, setIsDialogOpen] = React.useState(false)

          const handleOpenDialog = React.useCallback(() => {
            setIsDialogOpen(true)
          }, [])

          const handleCloseDialog = React.useCallback(() => {
            setIsDialogOpen(false)
          }, [])

          const handleItemSelect = React.useCallback((item: { itemId: string; itemName: string }) => {
            setRows(prev => {
              const newRows = [...prev]
              newRows[ctx.row.index] = {
                ...newRows[ctx.row.index],
                item: item
              }
              return newRows
            })
          }, [])

          return (
            <div className="flex items-center gap-2 px-1">
              <button
                onClick={handleOpenDialog}
                onMouseDown={e => e.stopPropagation()} // セル選択されてしまうのを防ぐ
                className="p-1 text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded cursor-pointer"
                title="商品を検索"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </button>

              <div className="flex-1 min-w-0">
                <div className="text-xs text-gray-500 truncate">{ctx.row.original.item?.itemId || "未選択"}</div>
                <div className="text-sm truncate">{ctx.row.original.item?.itemName || "商品を選択してください"}</div>
              </div>

              <ItemSearchDialog
                isOpen={isDialogOpen}
                onClose={handleCloseDialog}
                onSelect={handleItemSelect}
                onMouseDown={e => e.stopPropagation()} // セル選択されてしまうのを防ぐ
                currentItemId={ctx.row.original.item?.itemId}
              />
            </div>
          )
        }
      }),
      fieldPath: 'item',
    }
  ], [])

  const handleRowChange: RowChangeEvent<OrderRowData> = React.useCallback((e) => {
    const newRows = [...rows]
    e.changedRows.forEach((changedRow) => {
      newRows[changedRow.rowIndex] = changedRow.newRow
    })
    setRows(newRows)
    // console.log('行が変更されました:', e.changedRows)
  }, [rows])

  const addRow = () => {
    const newRow: OrderRowData = {
      orderTime: new Date().toISOString().slice(0, 16).replace('T', ' '),
      quantity: 1,
      item: { itemId: "", itemName: "" }
    }
    setRows([...rows, newRow])
  }

  const removeLastRow = () => {
    if (rows.length > 0) {
      setRows(rows.slice(0, -1))
    }
  }

  return (
    <div className="w-full h-full flex flex-col gap-4 p-4">
      <div className="flex-1 overflow-auto">
        <div className="flex flex-col gap-4">
          <h2 className="text-xl font-semibold">EditableGrid フック使用テスト</h2>

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
              showCheckBox={true}
              className="w-full h-96"
              showHorizontalBorder={true}
            />
          </div>

          <div className="text-sm text-gray-600">
            <p>💡 テスト項目:</p>
            <ul className="list-disc list-inside ml-4">
              <li>renderCell内でReactフック（useState）の使用</li>
              <li>商品検索ダイアログの開閉状態管理</li>
              <li>createPortalを使用したモーダルダイアログ</li>
              <li>onStartEditing/onEndEditingの動作確認</li>
              <li>商品選択時の行データ更新</li>
              <li>非同期検索処理（Promise）のシミュレーション</li>
            </ul>
            <p className="mt-2 text-xs text-gray-500">
              💡 Tips: 商品列の虫眼鏡ボタンをクリックして商品検索ダイアログを開き、商品を選択してみてください。
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

// 商品検索ダイアログのコンポーネント
const ItemSearchDialog = ({ isOpen, onClose, onSelect, onMouseDown, currentItemId }: {
  onMouseDown: React.MouseEventHandler<HTMLDivElement>
  isOpen: boolean
  onClose: () => void
  onSelect: (item: { itemId: string; itemName: string }) => void
  currentItemId?: string
}) => {

  console.log('ItemSearchDialog render, isOpen:', isOpen)

  const [searchText, setSearchText] = React.useState("")
  const [searchResults, setSearchResults] = React.useState<Array<{ itemId: string; itemName: string }>>([])
  const [isSearching, setIsSearching] = React.useState(false)

  // 商品データ（実際のアプリではAPIから取得）
  const mockItems = React.useMemo(() => [
    { itemId: "ITEM001", itemName: "ノートパソコン" },
    { itemId: "ITEM002", itemName: "マウス" },
    { itemId: "ITEM003", itemName: "キーボード" },
    { itemId: "ITEM004", itemName: "モニター" },
    { itemId: "ITEM005", itemName: "プリンター" },
    { itemId: "ITEM006", itemName: "スキャナー" },
    { itemId: "ITEM007", itemName: "USBメモリ" },
    { itemId: "ITEM008", itemName: "外付けHDD" },
  ], [])

  // 検索処理（Promiseで非同期処理をシミュレート）
  const searchItems = React.useCallback(async (query: string): Promise<Array<{ itemId: string; itemName: string }>> => {
    // 実際のAPI呼び出しをシミュレート
    await new Promise(resolve => setTimeout(resolve, 300))

    if (!query.trim()) return []

    return mockItems.filter(item =>
      item.itemName.toLowerCase().includes(query.toLowerCase()) ||
      item.itemId.toLowerCase().includes(query.toLowerCase())
    )
  }, [mockItems])

  // 検索実行
  const handleSearch = React.useCallback(async () => {
    if (!searchText.trim()) {
      setSearchResults([])
      return
    }

    setIsSearching(true)
    try {
      const results = await searchItems(searchText)
      setSearchResults(results)
    } catch (error) {
      console.error("検索エラー:", error)
      setSearchResults([])
    } finally {
      setIsSearching(false)
    }
  }, [searchText, searchItems])

  // Enterキーで検索
  const handleKeyDown = React.useCallback((e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch()
    }
  }, [handleSearch])

  // 商品選択
  const handleItemSelect = React.useCallback((item: { itemId: string; itemName: string }) => {
    onSelect(item)
    onClose()
    setSearchText("")
    setSearchResults([])
  }, [onSelect, onClose])

  // ダイアログが開かれたときの初期化
  React.useEffect(() => {
    if (isOpen) {
      setSearchText("")
      setSearchResults([])
      setIsSearching(false)
    }
  }, [isOpen])

  // console.log('ItemSearchDialog render, isOpen:', isOpen)
  if (!isOpen) return null

  return createPortal(
    <div
      onMouseDown={onMouseDown}
      className="fixed inset-0 flex items-center bg-black/25 justify-center z-50"
    >
      <div className="bg-white rounded-lg shadow-xl w-96 max-h-96 flex flex-col">
        {/* ヘッダー */}
        <div className="flex items-center justify-between p-4 border-b">
          <h3 className="text-lg font-semibold">商品検索</h3>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700 text-xl"
          >
            ×
          </button>
        </div>

        {/* 検索フォーム */}
        <div className="p-4 border-b">
          <div className="flex gap-2">
            <input
              type="text"
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder="商品名または商品IDで検索..."
              className="flex-1 px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button
              onClick={handleSearch}
              disabled={isSearching}
              className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:bg-gray-400"
            >
              {isSearching ? "検索中..." : "検索"}
            </button>
          </div>
        </div>

        {/* 検索結果 */}
        <div className="flex-1 overflow-y-auto p-4">
          {searchResults.length === 0 && searchText.trim() && !isSearching && (
            <div className="text-center text-gray-500 py-8">
              商品が見つかりませんでした
            </div>
          )}

          {searchResults.map((item) => (
            <div
              key={item.itemId}
              onClick={() => handleItemSelect(item)}
              className="p-3 border border-gray-200 rounded mb-2 cursor-pointer hover:bg-gray-50 hover:border-blue-300"
            >
              <div className="font-medium text-blue-600">{item.itemId}</div>
              <div className="text-sm text-gray-600">{item.itemName}</div>
            </div>
          ))}
        </div>

        {/* フッター */}
        <div className="p-4 border-t bg-gray-50">
          <div className="text-sm text-gray-500">
            現在選択中: {currentItemId || "なし"}
          </div>
        </div>
      </div>
    </div>,
    document.body
  )
}