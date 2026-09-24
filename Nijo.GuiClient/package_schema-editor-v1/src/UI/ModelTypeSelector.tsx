import React from "react"
import { MODEL_DATA, MODEL_QUERY, MODEL_COMMAND, MODEL_STRUCTURE } from "../backend"
import { DropdownSelector } from "@nijo/ui-components"
import { MODEL_COLORS } from "./modelColors"

/** モデル種類の選択肢 */
type ModelTypeOption = {
  displayName: string
  description: string
}

/** モデル種類の選択肢定義 */
export const MODEL_TYPE_OPTIONS: { [key: string]: ModelTypeOption } = {
  [MODEL_DATA]: {
    displayName: "Data Model",
    description: "永続化されるデータ。EFCoreの構造定義、自動生成可能なエラーチェック、楽観的排他制御の基本機能が自動生成されます。",
  },
  [MODEL_QUERY]: {
    displayName: "Query Model",
    description: "データの検索や照会に特化したモデル。一覧検索処理が自動生成されます。",
  },
  [MODEL_COMMAND]: {
    displayName: "Command Model",
    description: "引数を受け取り戻り値を返す処理。Webサーバー・クライアント間で常に同期された型定義を提供します。",
  },
  [MODEL_STRUCTURE]: {
    displayName: "Structure Model",
    description: "構造体。Webサーバー・クライアント間で常に同期されているべき構造を定義します。",
  },
}

type ModelTypeSelectorForSchemaProps = {
  value?: string
  onChange: (value: string) => void
  className?: string
  autoFocus?: boolean
  onKeyDown?: React.KeyboardEventHandler<HTMLButtonElement>
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
      {Object.entries(MODEL_TYPE_OPTIONS).map(([value, option]) => [
        value,
        (
          <span className={`select-none ${MODEL_COLORS[value].nameText}`}>
            {option.displayName}
          </span>
        ),
        (
          <div key={value} className="p-1">
            <div className={`font-semibold mb-1 ${MODEL_COLORS[value].nameText}`}>
              {option.displayName}
            </div>
            <div className={`text-xs leading-relaxed ${MODEL_COLORS[value].descriptionText}`}>
              {option.description}
            </div>
          </div>
        )
      ])}
    </DropdownSelector>
  )
}

/**
 * ルート集約のモデルの型を選択するラジオボタン
 */
export function ModelTypeRadioButtonGroup({ onChange }: {
  onChange: (value: string) => void
}) {

  React.useEffect(() => {
    const firstButton = document.querySelector<HTMLButtonElement>('button[name="modelType"]')
    firstButton?.focus()
  }, [])

  return (
    <div className="flex flex-col gap-2">
      {Object.entries(MODEL_TYPE_OPTIONS).map(([value, option]) => (
        <button
          key={value}
          type="button"
          name="modelType"
          onClick={() => onChange(value)}
          className="flex flex-col items-start gap-1 p-2 rounded border cursor-pointer transition-colors text-left bg-white border-gray-200 hover:bg-gray-50"
        >
          <div className={`font-bold ${MODEL_COLORS[value].nameText}`}>
            {option.displayName}
          </div>
          <div className={`text-xs mt-1 leading-snug ${MODEL_COLORS[value].descriptionText}`}>
            {option.description}
          </div>
        </button>
      ))}
    </div>
  )
}
