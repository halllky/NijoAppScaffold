import React from "react"
import { ChevronDownIcon, ChevronRightIcon, ExclamationTriangleIcon, MagnifyingGlassIcon, PlusIcon } from "@heroicons/react/24/solid"
import { matchesTableKeyword, type DbSchema, type DbSchemaTable } from "./DbSchema"
import type { SubjectArea } from "./DbViewerSettings"

/**
 * DBビューアのサイドメニュー。
 * 全テーブルの一覧と、サブジェクトエリアごとのテーブルの一覧を、それぞれテーブル名で絞り込めるツリーとして表示する。
 * サブジェクトエリアの中身はここでは編集しない。
 */
export function SideMenu({ schema, subjectAreas, currentAreaId, tablesInCurrentArea, onSelectArea, onSelectTable, onAddArea }: {
  schema: DbSchema
  subjectAreas: SubjectArea[]
  /** 表示中のサブジェクトエリアのID。無い場合は undefined */
  currentAreaId: string | undefined
  /** 表示中のサブジェクトエリアの図の中にノードとして存在するテーブルの物理名 */
  tablesInCurrentArea: ReadonlySet<string>
  /** サブジェクトエリアの名前がクリックされたときに呼ばれる */
  onSelectArea: (areaId: string) => void
  /** テーブル名がクリックされたときに呼ばれる。全テーブルの一覧の中でクリックされた場合、areaId は undefined */
  onSelectTable: (areaId: string | undefined, tableName: string) => void
  /** 新規追加ボタンが押されたときに呼ばれる */
  onAddArea: () => void
}) {
  const tableByName = React.useMemo(() => new Map(schema.tables.map(table => [table.tableName, table])), [schema])

  return (
    <nav className="flex flex-col h-full overflow-y-auto bg-gray-50 text-sm select-none">
      {/* 全てのテーブル */}
      <SideMenuGroup
        title="全てのテーブル"
        items={schema.tables.map(table => ({
          tableName: table.tableName,
          table,
          // 表示中のサブジェクトエリアに無いテーブルはフォーカスできない
          disabledReason: tablesInCurrentArea.has(table.tableName) ? undefined : "表示中のサブジェクトエリアに含まれていません",
        }))}
        onSelectTable={tableName => onSelectTable(undefined, tableName)}
      />

      {/* サブジェクトエリアごと */}
      {subjectAreas.map(area => (
        <SideMenuGroup
          key={area.id}
          title={area.name}
          isCurrent={area.id === currentAreaId}
          onClickTitle={() => onSelectArea(area.id)}
          items={area.tables.map(({ tableName }) => ({
            tableName,
            table: tableByName.get(tableName),
            disabledReason: tableByName.has(tableName) ? undefined : "DB定義に存在しません",
          }))}
          onSelectTable={tableName => onSelectTable(area.id, tableName)}
        />
      ))}

      {/* 新規追加 */}
      <button
        type="button"
        onClick={onAddArea}
        className="flex items-center gap-1 px-2 py-1 text-left text-sky-700 hover:bg-sky-50 cursor-pointer"
      >
        <PlusIcon className="w-4 h-4" />
        新規追加
      </button>
    </nav>
  )
}

// -------------------------------------

/** サイドメニューのグループに並べるテーブル1個 */
type SideMenuItem = {
  tableName: string
  /** DB定義に存在しないテーブルの場合は undefined */
  table: DbSchemaTable | undefined
  /** クリックできない場合はその理由。クリックできる場合は undefined */
  disabledReason: string | undefined
}

/**
 * サイドメニューの1グループ。見出し、テーブル名の絞り込み欄、テーブルの一覧からなる。
 * 開閉状態と絞り込みの入力内容はこのグループの中だけで保持する。
 */
function SideMenuGroup({ title, isCurrent, onClickTitle, items, onSelectTable }: {
  title: string
  /** 表示中のサブジェクトエリアのグループかどうか */
  isCurrent?: boolean
  /** 未指定の場合、見出しをクリックしても開閉するだけ */
  onClickTitle?: () => void
  items: SideMenuItem[]
  onSelectTable: (tableName: string) => void
}) {
  const [expanded, setExpanded] = React.useState(true)
  const [keyword, setKeyword] = React.useState("")

  const filteredItems = items.filter(item => item.table
    ? matchesTableKeyword(item.table, keyword)
    : item.tableName.toLowerCase().includes(keyword.trim().toLowerCase()))

  return (
    <div className="flex flex-col border-b border-gray-200">
      {/* 見出し */}
      <div className={`flex items-center gap-1 px-1 py-1 ${isCurrent ? "bg-sky-100" : ""}`}>
        <button type="button" onClick={() => setExpanded(!expanded)} className="flex-none p-0.5 rounded-sm hover:bg-gray-200 cursor-pointer">
          {expanded ? <ChevronDownIcon className="w-4 h-4" /> : <ChevronRightIcon className="w-4 h-4" />}
        </button>
        <button
          type="button"
          onClick={onClickTitle ?? (() => setExpanded(!expanded))}
          className={`flex-1 min-w-0 truncate text-left font-bold cursor-pointer ${onClickTitle ? "hover:text-sky-700" : ""}`}
          title={title}
        >
          {title}
        </button>
      </div>

      {expanded && (
        <>
          {/* 絞り込み */}
          <label className="flex items-center gap-1 mx-2 mb-1 px-1 bg-white border border-gray-300 rounded-sm">
            <input
              type="search"
              value={keyword}
              onChange={e => setKeyword(e.target.value)}
              placeholder="検索"
              spellCheck={false}
              className="flex-1 min-w-0 py-0.5 outline-none"
            />
            <MagnifyingGlassIcon className="flex-none w-4 h-4 text-gray-400" />
          </label>

          {/* テーブルの一覧 */}
          <ul className="flex flex-col pb-1">
            {filteredItems.map(item => (
              <li key={item.tableName}>
                <button
                  type="button"
                  disabled={item.disabledReason !== undefined}
                  onClick={() => onSelectTable(item.tableName)}
                  title={item.disabledReason ?? item.tableName}
                  className="flex items-center gap-1 w-full pl-6 pr-2 py-0.5 text-left enabled:hover:bg-sky-50 enabled:cursor-pointer disabled:text-gray-400"
                >
                  {!item.table && <ExclamationTriangleIcon className="flex-none w-4 h-4 text-amber-600" />}
                  <span className="truncate">{item.table?.logicalName ?? item.tableName}</span>
                  {item.table && item.table.logicalName !== item.tableName && (
                    <span className="truncate text-xs text-gray-400">{item.tableName}</span>
                  )}
                </button>
              </li>
            ))}
            {filteredItems.length === 0 && (
              <li className="pl-6 pr-2 py-0.5 text-gray-400">
                {items.length === 0 ? "テーブルがありません" : "該当するテーブルがありません"}
              </li>
            )}
          </ul>
        </>
      )}
    </div>
  )
}
