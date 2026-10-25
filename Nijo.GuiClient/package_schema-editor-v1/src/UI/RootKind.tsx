import React from "react"
import { ROOT_AGGREGATE_COLORS } from "./rootAggregateColors"

/** ルート集約の種類（データ構造・コマンド）のうち、データ構造を表す値 */
export const ROOT_KIND_DATA_STRUCTURE = 'data-structure'
/** ルート集約の種類（データ構造・コマンド）のうち、コマンドを表す値 */
export const ROOT_KIND_COMMAND = 'command'

/**
 * ルート集約の種類。データ構造かコマンドか。
 * データ構造から何が生成されるかはさらに IsStored, IsQueryable 属性の組み合わせで決まる（{@link DataStructureFlags}）。
 */
export type RootKind = typeof ROOT_KIND_DATA_STRUCTURE | typeof ROOT_KIND_COMMAND

/** データ構造の IsStored, IsQueryable 属性の値 */
export type DataStructureFlags = {
  isStored: boolean
  isQueryable: boolean
}

/** ルート集約の種類の選択肢 */
type RootKindOption = {
  displayName: string
  description: string
  colors: typeof ROOT_AGGREGATE_COLORS[keyof typeof ROOT_AGGREGATE_COLORS]
}

/** ルート集約の種類の選択肢定義 */
export const ROOT_KIND_OPTIONS: Record<RootKind, RootKindOption> = {
  [ROOT_KIND_DATA_STRUCTURE]: {
    displayName: "Data Structure",
    description: "データの構造。永続化するか、検索可能にするかを選べます。どちらも選ばない場合は、Webサーバー・クライアント間で常に同期されているべき構造体になります。",
    colors: ROOT_AGGREGATE_COLORS.structure,
  },
  [ROOT_KIND_COMMAND]: {
    displayName: "Command",
    description: "引数を受け取り戻り値を返す処理。Webサーバー・クライアント間で常に同期された型定義を提供します。",
    colors: ROOT_AGGREGATE_COLORS.command,
  },
}

/** データ構造の IsStored, IsQueryable 属性の選択肢定義 */
const DATA_STRUCTURE_FLAG_OPTIONS: { key: keyof DataStructureFlags, displayName: string, description: string }[] = [{
  key: 'isStored',
  displayName: "永続化する",
  description: "データベースに永続化されます。EFCoreの構造定義、自動生成可能なエラーチェック、楽観的排他制御の基本機能が自動生成されます。",
}, {
  key: 'isQueryable',
  displayName: "検索可能",
  description: "画面や帳票から検索・照会されます。一覧検索処理が自動生成されます。",
}]

/**
 * ルート集約の種類を選択するラジオボタン
 */
export function RootKindRadioButtonGroup({ onChange }: {
  onChange: (value: RootKind) => void
}) {

  React.useEffect(() => {
    const firstButton = document.querySelector<HTMLButtonElement>('button[name="rootKind"]')
    firstButton?.focus()
  }, [])

  return (
    <div className="flex flex-col gap-2">
      {Object.entries(ROOT_KIND_OPTIONS).map(([value, option]) => (
        <button
          key={value}
          type="button"
          name="rootKind"
          onClick={() => onChange(value as RootKind)}
          className="flex flex-col items-start gap-1 p-2 rounded border cursor-pointer transition-colors text-left bg-white border-gray-200 hover:bg-gray-50"
        >
          <div className={`font-bold ${option.colors.nameText}`}>
            {option.displayName}
          </div>
          <div className={`text-xs mt-1 leading-snug ${option.colors.descriptionText}`}>
            {option.description}
          </div>
        </button>
      ))}
    </div>
  )
}

/**
 * データ構造の IsStored, IsQueryable 属性を編集するチェックボックス
 */
export function DataStructureFlagsCheckBoxes({ value, onChange, showDescription, className }: {
  value: DataStructureFlags
  onChange: (value: DataStructureFlags) => void
  /** 各属性の説明文を表示するかどうか */
  showDescription?: boolean
  className?: string
}) {
  return (
    <div className={`flex ${showDescription ? 'flex-col gap-2' : 'items-center gap-3'} ${className ?? ''}`}>
      {DATA_STRUCTURE_FLAG_OPTIONS.map(option => (
        <label key={option.key} className="flex items-start gap-1 select-none cursor-pointer">
          <input
            type="checkbox"
            className="h-4 w-4 mt-0.5"
            checked={value[option.key]}
            onChange={e => onChange({ ...value, [option.key]: e.target.checked })}
          />
          <span className="flex flex-col">
            <span className="text-sm whitespace-nowrap">{option.displayName}</span>
            {showDescription && (
              <span className="text-xs text-gray-500 leading-snug">{option.description}</span>
            )}
          </span>
        </label>
      ))}
    </div>
  )
}
