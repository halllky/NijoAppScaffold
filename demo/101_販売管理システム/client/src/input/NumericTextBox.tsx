import React from "react"

export type NumericTextBoxProps = React.InputHTMLAttributes<HTMLInputElement> & {
  /** 3桁カンマ区切り表示 */
  commaSeparated?: boolean
}

/**
 * 数値入力テキストボックス。
 * 通常の `input type="text"` に加え、以下の機能を持つ。
 *
 * - 基本的な共通レイアウト（以下、例）
 *   - 右寄せ
 *   - 入力可能なら枠と背景色を変更
 *   - 入力不可能なら枠なし背景色なし
 *   - spellcheck や autocomplete を無効化
 * - 3桁カンマ区切り表示オプション
 * - フォーカスアウト時に半角数値への正規化を自動実行
 */
export const NumericTextBox = React.forwardRef<HTMLInputElement, NumericTextBoxProps>((props, ref) => {
  const { commaSeparated, className, readOnly, disabled, onBlur, onFocus, onChange, ...rest } = props

  const handleFocus = (e: React.FocusEvent<HTMLInputElement>) => {
    if (commaSeparated && !readOnly && !disabled) {
      const val = e.target.value.replace(/,/g, '')
      if (val !== e.target.value) {
        e.target.value = val
        onChange?.(e as any)
      }
    }
    onFocus?.(e)
  }

  const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
    let val = e.target.value
    // 全角→半角
    val = val.replace(/[０-９]/g, (s) => String.fromCharCode(s.charCodeAt(0) - 0xFEE0))

    if (commaSeparated) {
      const numStr = val.replace(/,/g, '')
      const num = Number(numStr)
      if (!isNaN(num) && numStr !== '') {
        val = num.toLocaleString()
      }
    }

    if (val !== e.target.value) {
      e.target.value = val
      onChange?.(e as any)
    }
    onBlur?.(e)
  }

  const baseStyle = "block py-px px-1 border outline-blue-500 focus:ring-blue-500 text-right"
  const editableStyle = "border-gray-700 bg-white"
  const readOnlyStyle = "border-transparent bg-transparent shadow-none cursor-default focus:ring-0"

  const isEditable = !readOnly && !disabled

  return (
    <input
      type="text"
      ref={ref}
      readOnly={readOnly}
      disabled={disabled}
      spellCheck={false}
      autoComplete="off"
      className={`${baseStyle} ${isEditable ? editableStyle : readOnlyStyle} ${className ?? ''}`}
      onFocus={handleFocus}
      onBlur={handleBlur}
      onChange={onChange}
      {...rest}
    />
  )
})
