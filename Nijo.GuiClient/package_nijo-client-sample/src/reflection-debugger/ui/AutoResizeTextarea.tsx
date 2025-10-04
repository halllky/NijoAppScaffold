import React from "react"

/**
 * コンテンツの長さに応じて自動的にサイズを調整するテキストエリア。
 */
export const AutoResizeTextarea = React.forwardRef<HTMLTextAreaElement, React.TextareaHTMLAttributes<HTMLTextAreaElement>>(({
  autoComplete,
  spellCheck,
  className,
  ...rest
}, ref) => {

  let cls = 'px-1 field-sizing-content resize-none'

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
    <textarea
      ref={ref}
      {...rest}
      autoComplete={autoComplete ?? 'off'}
      spellCheck={spellCheck ?? false}
      className={cls}
    />
  )
})