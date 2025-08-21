import React from "react"

/** フォーム内部で使用するコンテキスト */
export type ResponsiveFormContextValue = {
  /** 4列レイアウトかどうか */
  isWideLayout: boolean
  /** ラベル列の幅 */
  labelWidth: number
  /** 値列の幅 */
  valueWidth: number
  labelAlign: 'left' | 'right'
}

/** フォーム内部で使用するコンテキスト */
export const ResponsiveFormContext = React.createContext<ResponsiveFormContextValue>({
  isWideLayout: false,
  labelWidth: 120,
  valueWidth: 200,
  labelAlign: 'right',
})
