import React from "react"
import { ArrowDownIcon, ArrowUpIcon, MagnifyingGlassIcon, TrashIcon } from "@heroicons/react/24/solid"
import { Button, CheckBox, Modal } from "../../ui"
import { matchesTableKeyword, type DbSchema } from "./DbSchema"
import {
  addTableToArea, isAllTablesArea, removeTableFromArea, COLUMN_ATTRIBUTE_LABELS, TABLE_ATTRIBUTE_LABELS,
  type ColumnAttribute, type SubjectArea, type TableAttribute,
} from "./DbViewerSettings"

/**
 * サブジェクトエリアの設定ダイアログ。
 * 名前、表示するテーブル、ノードに表示する属性とその順番を設定する。
 * 全テーブルのサブジェクトエリアでは、名前と表示するテーブルは変更できず、削除もできない。
 *
 * 変更はダイアログの中の下書きにだけ反映され、保存ボタンが押されたときに onSave で通知される。
 * 保存ボタン以外で閉じた場合、下書きは破棄される。
 * ダイアログの開閉、保存・削除の実処理は呼び出し側で行う。
 */
export function SubjectAreaSettingsDialog({ schema, area, isNew, onSave, onDelete, onClose }: {
  schema: DbSchema
  /** 設定するサブジェクトエリアの、ダイアログを開いた時点の設定。下書きの初期値になる */
  area: SubjectArea
  /** まだ作成されていないサブジェクトエリアの場合は true。削除ボタンを表示しない */
  isNew: boolean
  /** 保存ボタンが押されたときに、下書きを伴って呼ばれる */
  onSave: (area: SubjectArea) => void
  /** 削除ボタンが押され、確認に同意されたときに呼ばれる */
  onDelete: () => void
  onClose: () => void
}) {
  // 下書き。ダイアログは開くたびに作り直されるため、開いた時点の設定が初期値になる
  const [draft, setDraft] = React.useState(area)
  const [keyword, setKeyword] = React.useState("")

  const isAllTables = isAllTablesArea(area)
  const selectedTableNames = new Set(draft.tables.map(table => table.tableName))
  const filteredTables = schema.tables.filter(table => matchesTableKeyword(table, keyword))

  //#region イベント

  const handleToggleTable = (tableName: string, checked: boolean) => {
    setDraft(prev => checked ? addTableToArea(prev, tableName) : removeTableFromArea(prev, tableName))
  }

  /** 絞り込まれているテーブルをまとめて選択・解除する */
  const handleToggleFilteredTables = (checked: boolean) => {
    setDraft(prev => filteredTables.reduce(
      (acc, table) => checked ? addTableToArea(acc, table.tableName) : removeTableFromArea(acc, table.tableName),
      prev))
  }

  /** 下書きを破棄して閉じる。下書きに変更がある場合は確認する */
  const handleCancel = () => {
    if (draft !== area && !confirm("変更内容を破棄して閉じますか？")) return
    onClose()
  }

  const handleDelete = () => {
    if (!confirm(`サブジェクトエリア「${area.name}」を削除しますか？`)) return
    onDelete()
  }

  //#endregion イベント

  return (
    <Modal isOpen onClose={handleCancel} title="サブジェクトエリアの設定" widthClass="w-full max-w-4xl" className="h-full">
      <div className="flex-1 min-h-0 flex flex-col gap-3 p-4 overflow-hidden">

        {/* 名前 */}
        <label className="flex items-center gap-2">
          <span className="flex-none w-20 text-sm text-gray-500">名前</span>
          {isAllTables ? (
            <span className="flex-1 px-1">{draft.name}</span>
          ) : (
            <input
              value={draft.name}
              onChange={e => setDraft(prev => ({ ...prev, name: e.target.value }))}
              className="flex-1 px-1 border border-gray-300 rounded-sm"
            />
          )}
        </label>

        <div className="flex-1 min-h-0 flex gap-4">
          {/* 表示するテーブル */}
          {isAllTables ? (
            <section className="flex-1 min-w-0 text-sm text-gray-500">
              このサブジェクトエリアにはDB定義の全テーブルが表示されます。
            </section>
          ) : (
            <section className="flex-1 min-w-0 flex flex-col gap-1">
              <h3 className="text-sm font-bold">表示するテーブル（{draft.tables.length}件選択中）</h3>
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
          )}

          {/* ノードに表示する属性 */}
          <section className="flex-none w-64 flex flex-col gap-3 overflow-y-auto">
            <AttributeOrderEditor<TableAttribute>
              title="テーブルの属性"
              labels={TABLE_ATTRIBUTE_LABELS}
              value={draft.tableAttributes}
              onChange={tableAttributes => setDraft(prev => ({ ...prev, tableAttributes }))}
            />
            <AttributeOrderEditor<ColumnAttribute>
              title="カラムの属性"
              labels={COLUMN_ATTRIBUTE_LABELS}
              value={draft.columnAttributes}
              onChange={columnAttributes => setDraft(prev => ({ ...prev, columnAttributes }))}
            />
          </section>
        </div>

        {/* フッター */}
        <div className="flex items-center gap-2">
          {!isNew && !isAllTables && (
            <Button outline icon={TrashIcon} onClick={handleDelete} className="text-red-700">このサブジェクトエリアを削除</Button>
          )}
          <div className="flex-1" />
          <Button outline onClick={handleCancel}>キャンセル</Button>
          <Button fill onClick={() => onSave(draft)}>保存</Button>
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
