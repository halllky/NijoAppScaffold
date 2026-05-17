import React from "react"
import { TYPE_WRITE_MODEL2, TYPE_READ_MODEL2, TYPE_COMMAND_MODEL2, TYPE_STRUCTURE_MODEL } from "../types"
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
    value: TYPE_WRITE_MODEL2,
    displayName: "Write Model 2",
    description: "旧版互換の更新系モデル。永続化対象の構造、バリデーション、参照生成の移植先として扱います。",
    nameTextColor: "text-orange-600",
    descriptionTextColor: "text-orange-500",
  },
  {
    value: TYPE_READ_MODEL2,
    displayName: "Read Model 2",
    description: "旧版互換の参照系モデル。検索条件や表示用データを一体で定義します。",
    nameTextColor: "text-emerald-600",
    descriptionTextColor: "text-emerald-500",
  },
  {
    value: TYPE_COMMAND_MODEL2,
    displayName: "Command Model 2",
    description: "旧版互換のコマンドモデル。引数や step を子要素としてこの画面でまとめて編集します。",
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
  autoFocus?: boolean
  onKeyDown?: React.KeyboardEventHandler<HTMLInputElement>
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

/**
 * ルート集約のモデルの型を選択するラジオボタン
 */
export function ModelTypeRadioButtonGroup({
  value,
  onChange,
  autoFocus,
  className,
  onKeyDown,
}: ModelTypeSelectorForSchemaProps) {

  React.useEffect(() => {
    if (autoFocus) {
      const firstRadio = document.querySelector<HTMLInputElement>('input[name="modelType"]')
      firstRadio?.focus()
    }
  }, [autoFocus])

  return (
    <div className={`flex flex-col gap-2 ${className ?? ''}`}>
      {MODEL_TYPE_OPTIONS.map(option => (
        <label
          key={option.value}
          className={`
            flex items-start gap-3 p-3 rounded border cursor-pointer transition-colors
            ${value === option.value ? 'bg-blue-50 border-blue-400 ring-1 ring-blue-400' : 'bg-white border-gray-200 hover:bg-gray-50'}
          `}
        >
          <input
            type="radio"
            name="modelType"
            value={option.value}
            checked={value === option.value}
            onChange={() => onChange(option.value)}
            onKeyDown={onKeyDown}
            className="mt-1"
          />
          <div>
            <div className={`font-bold ${option.nameTextColor}`}>
              {option.displayName}
            </div>
            <div className={`text-xs mt-1 leading-snug ${option.descriptionTextColor}`}>
              {option.description}
            </div>
          </div>
        </label>
      ))}
    </div>
  )
}
