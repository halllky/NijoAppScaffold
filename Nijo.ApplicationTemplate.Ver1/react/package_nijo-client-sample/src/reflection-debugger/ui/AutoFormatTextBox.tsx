import React from "react";
import useEvent from "react-use-event-hook";

export type AutoFormatTextBoxProps = React.InputHTMLAttributes<HTMLInputElement> & {
  /** オートフォーマット関数 */
  autoFormat?: (value: string) => string
}

/**
 * フォーカス離脱時に任意のオートフォーマットを適用するテキストボックス。
 * 単語の前後トリム、数値のオートフォーマットなどを行なう。
 */
export const AutoFormatTextBox = React.forwardRef<HTMLInputElement, AutoFormatTextBoxProps>(({
  onFocus,
  onBlur,
  autoComplete,
  spellCheck,
  autoFormat,
  className,
  ...rest
}, ref) => {

  // フォーカスインしたときの値
  const [valueOnFocus, setValueOnFocus] = React.useState(rest.value)

  // フォーカスイン処理
  const handleFocus: React.FocusEventHandler<HTMLInputElement> = useEvent(e => {
    setValueOnFocus(e.target.value)
    onFocus?.(e)
  })

  // フォーカスアウト処理
  const handleBlur: React.FocusEventHandler<HTMLInputElement> = useEvent(e => {
    // フォーカスインしたときの値と異なる場合、ここでオートフォーマットを適用する
    if (valueOnFocus !== e.target.value && autoFormat) {
      e.target.value = autoFormat(e.target.value)
    }
    onBlur?.(e)
  })

  let cls = 'px-1'

  // 背景色
  if (rest.disabled || rest.readOnly) {
    // 背景色なし
  } else {
    cls += ' bg-white'
  }

  // 枠
  if (rest.disabled || rest.readOnly) {
    cls += ' border border-transparent'
  } else {
    cls += ' border'
  }

  // カスタム
  if (className) {
    cls += ` ${className}`
  }

  return (
    <input
      ref={ref}
      {...rest}
      autoComplete={autoComplete ?? 'off'}
      spellCheck={spellCheck ?? false}
      onFocus={handleFocus}
      onBlur={handleBlur}
      className={cls}
    />
  )
})
