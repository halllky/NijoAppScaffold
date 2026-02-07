import React from "react"
import { TYPE_DATA_MODEL, TYPE_QUERY_MODEL, TYPE_COMMAND_MODEL, TYPE_STRUCTURE_MODEL } from "../types"
import { DropdownSelector } from "@nijo/ui-components"

/** モデル種類の選択肢 */
type ModelTypeOption = {
  value: string
  displayName: string
  description: string
  nameTextColor: string
  descriptionTextColor: string
}

/** モデル種類の選択肢定義 */
const MODEL_TYPE_OPTIONS: ModelTypeOption[] = [
  {
    value: TYPE_DATA_MODEL,
    displayName: "Data Model",
    description: "永続化されるデータ。EFCoreの構造定義、自動生成可能なエラーチェック、楽観的排他制御の基本機能が自動生成されます。",
    nameTextColor: "text-orange-600",
    descriptionTextColor: "text-orange-500",
  },
  {
    value: TYPE_QUERY_MODEL,
    displayName: "Query Model",
    description: "データの検索や照会に特化したモデル。一覧検索処理が自動生成されます。",
    nameTextColor: "text-emerald-600",
    descriptionTextColor: "text-emerald-500",
  },
  {
    value: TYPE_COMMAND_MODEL,
    displayName: "Command Model",
    description: "引数を受け取り戻り値を返す処理。Webサーバー・クライアント間で常に同期された型定義を提供します。",
    nameTextColor: "text-sky-600",
    descriptionTextColor: "text-sky-500",
  },
  {
    value: TYPE_STRUCTURE_MODEL,
    displayName: "Structure Model",
    description: "構造体。Webサーバー・クライアント間で常に同期されているべき構造を定義します。",
    nameTextColor: "text-gray-600",
    descriptionTextColor: "text-gray-500",
  }
]

type ModelTypeSelectorForSchemaProps = {
  value?: string
  onChange: (value: string) => void
  className?: string
}

/**
 * ルート集約のモデルの型を選択するドロップダウン
 */
export function ModelTypeSelector({
  value,
  onChange,
  className
}: ModelTypeSelectorForSchemaProps) {
  return (
    <DropdownSelector
      value={value}
      onChange={onChange}
      className={className}
    >
      {MODEL_TYPE_OPTIONS.map(option => [
        option.value,
        (
          <span className={`select-none ${option.nameTextColor}`}>
            {option.displayName}
          </span>
        ),
        (
          <div key={option.value} className="p-1">
            <div className={`font-semibold mb-1 ${option.nameTextColor}`}>
              {option.displayName}
            </div>
            <div className={`text-xs leading-relaxed ${option.descriptionTextColor}`}>
              {option.description}
            </div>
          </div>
        )
      ])}
    </DropdownSelector>
  )
}
