import React from "react"
import { ArrowDownIcon, ArrowUpIcon, MagnifyingGlassIcon, TrashIcon } from "@heroicons/react/24/solid"
import { Modal } from "../../ui/Modal"
import { Button } from "../../ui/Button"
import { CheckBox } from "../../ui/CheckBox"
import { matchesTableKeyword, type DbSchema } from "./DbSchema"
import {
  addTableToArea, removeTableFromArea, COLUMN_ATTRIBUTE_LABELS, TABLE_ATTRIBUTE_LABELS,
  type ColumnAttribute, type SubjectArea, type TableAttribute,
} from "./DbViewerSettings"

/**
 * サブジェクトエリアの設定ダイアログ。
 * 名前、表示するテーブル、ノードに表示する属性とその順番を設定する。
 * 変更はその場で onChangeArea により通知され、ダイアログを閉じる前から図に反映される。
 * ダイアログの開閉とサブジェクトエリアの削除の実処理は呼び出し側で行う。
 */
export function SubjectAreaSettingsDialog({ schema, area, onChangeArea, onTablesAdded, onDelete, onClose }: {
  schema: DbSchema
  area: SubjectArea
  onChangeArea: (updater: (area: SubjectArea) => SubjectArea) => void
  /** 表示するテーブルが追加されたときに、onChangeArea の後で、追加されたテーブルの物理名を伴って呼ばれる */
  onTablesAdded: (tableNames: string[]) => void
  /** 削除ボタンが押され、確認に同意されたときに呼ばれる */
  onDelete: () => void
  onClose: () => void
}) {
  const [keyword, setKeyword] = React.useState("")

  const selectedTableNames = new Set(area.tables.map(table => table.tableName))
  const filteredTables = schema.tables.filter(table => matchesTableKeyword(table, keyword))

  //#region イベント

  const handleToggleTable = (tableName: string, checked: boolean) => {
    onChangeArea(prev => checked ? addTableToArea(prev, tableName) : removeTableFromArea(prev, tableName))
    if (checked) onTablesAdded([tableName])
  }

  /** 絞り込まれているテーブルをまとめて選択・解除する */
  const handleToggleFilteredTables = (checked: boolean) => {
    onChangeArea(prev => filteredTables.reduce(
      (acc, table) => checked ? addTableToArea(acc, table.tableName) : removeTableFromArea(acc, table.tableName),
      prev))
    if (checked) onTablesAdded(filteredTables.map(table => table.tableName).filter(tableName => !selectedTableNames.has(tableName)))
  }

  const handleDelete = () => {
    if (!confirm(`サブジェクトエリア「${area.name}」を削除しますか？`)) return
    onDelete()
  }

  //#endregion イベント

  return (
    <Modal isOpen onClose={onClose} title="サブジェクトエリアの設定" widthClass="w-full max-w-4xl" className="h-full">
      <div className="flex-1 min-h-0 flex flex-col gap-3 p-4 overflow-hidden">

        {/* 名前 */}
        <label className="flex items-center gap-2">
          <span className="flex-none w-20 text-sm text-gray-500">名前</span>
          <input
            value={area.name}
            onChange={e => onChangeArea(prev => ({ ...prev, name: e.target.value }))}
            className="flex-1 px-1 border border-gray-300 rounded-sm"
          />
        </label>

        <div className="flex-1 min-h-0 flex gap-4">
          {/* 表示するテーブル */}
          <section className="flex-1 min-w-0 flex flex-col gap-1">
            <h3 className="text-sm font-bold">表示するテーブル（{area.tables.length}件選択中）</h3>
            <div className="flex items-center gap-2">
              <label className="flex-1 flex items-center gap-1 px-1 border border-gray-300 rounded-sm">
                <input
                  type="search"
                  value={keyword}
                  onChange={e => setKeyword(e.target.value)}
                  placeholder="テーブル名で絞り込み"
                  spellCheck={false}
                  className="flex-1 min-w-0 py-0.5 outline-none"
                />
                <MagnifyingGlassIcon className="flex-none w-4 h-4 text-gray-400" />
              </label>
              <Button mini outline onClick={() => handleToggleFilteredTables(true)}>全選択</Button>
              <Button mini outline onClick={() => handleToggleFilteredTables(false)}>全解除</Button>
            </div>
            <ul className="flex-1 min-h-0 overflow-y-auto border border-gray-200 rounded-sm p-1">
              {filteredTables.map(table => (
                <li key={table.tableName}>
                  <CheckBox
                    checked={selectedTableNames.has(table.tableName)}
                    onChange={e => handleToggleTable(table.tableName, e.target.checked)}
                  >
                    {table.logicalName}
                    {table.logicalName !== table.tableName && (
                      <span className="ml-1 text-xs text-gray-400">{table.tableName}</span>
                    )}
                  </CheckBox>
                </li>
              ))}
            </ul>
          </section>

          {/* ノードに表示する属性 */}
          <section className="flex-none w-64 flex flex-col gap-3 overflow-y-auto">
            <AttributeOrderEditor<TableAttribute>
              title="テーブルの属性"
              labels={TABLE_ATTRIBUTE_LABELS}
              value={area.tableAttributes}
              onChange={tableAttributes => onChangeArea(prev => ({ ...prev, tableAttributes }))}
            />
            <AttributeOrderEditor<ColumnAttribute>
              title="カラムの属性"
              labels={COLUMN_ATTRIBUTE_LABELS}
              value={area.columnAttributes}
              onChange={columnAttributes => onChangeArea(prev => ({ ...prev, columnAttributes }))}
            />
          </section>
        </div>

        {/* フッター */}
        <div className="flex items-center gap-2">
          <Button outline icon={TrashIcon} onClick={handleDelete} className="text-red-700">このサブジェクトエリアを削除</Button>
          <div className="flex-1" />
          <Button fill onClick={onClose}>閉じる</Button>
        </div>
      </div>
    </Modal>
  )
}

// -------------------------------------

/**
 * 表示する属性とその順番の編集欄。
 * 表示する属性が上に設定された順で並び、その下に表示しない属性が並ぶ。
 */
function AttributeOrderEditor<T extends string>({ title, labels, value, onChange }: {
  title: string
  /** 選択肢の全属性とその表示名 */
  labels: { [key in T]: string }
  /** 表示する属性。配列の順に表示される */
  value: T[]
  onChange: (value: T[]) => void
}) {
  const allAttributes = Object.keys(labels) as T[]
  const hiddenAttributes = allAttributes.filter(attribute => !value.includes(attribute))

  /** 表示する属性の中で、指定の属性を1つ上 (-1) または下 (+1) に移動する */
  const move = (index: number, offset: -1 | 1) => {
    const next = [...value]
    const [moved] = next.splice(index, 1)
    next.splice(index + offset, 0, moved)
    onChange(next)
  }

  return (
    <div className="flex flex-col gap-1">
      <h3 className="text-sm font-bold">{title}</h3>
      <ul className="flex flex-col border border-gray-200 rounded-sm p-1">
        {/* 表示する属性 */}
        {value.map((attribute, index) => (
          <li key={attribute} className="flex items-center gap-1">
            <CheckBox checked onChange={() => onChange(value.filter(a => a !== attribute))} className="flex-1">
              {labels[attribute]}
            </CheckBox>
            <Button mini hideText icon={ArrowUpIcon} disabled={index === 0} onClick={() => move(index, -1)}>上へ</Button>
            <Button mini hideText icon={ArrowDownIcon} disabled={index === value.length - 1} onClick={() => move(index, 1)}>下へ</Button>
          </li>
        ))}

        {/* 表示しない属性 */}
        {hiddenAttributes.map(attribute => (
          <li key={attribute} className="flex items-center gap-1">
            <CheckBox checked={false} onChange={() => onChange([...value, attribute])} className="flex-1">
              {labels[attribute]}
            </CheckBox>
          </li>
        ))}
      </ul>
    </div>
  )
}
