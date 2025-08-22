import React from "react"
import { DefaultLabel, LabelProps } from "./DefaultLabel"

/** フォーム内部で使用するコンテキスト */
export type FormLayoutContextValue = {
  /** `ResponsiveColumn` の受け入れ可能列数。2以上の整数 */
  columnCount: number
  /** ラベル列の幅 */
  labelWidth: number
  /** 値列の幅 */
  valueWidth: number
  /** ラベルの位置 */
  labelAlign: 'left' | 'right'
  /** ラベルのコンポーネント */
  LabelComponent: React.ElementType<LabelProps>
}

/** フォーム内部で使用するコンテキスト */
export const FormLayoutContext = React.createContext<FormLayoutContextValue>({
  columnCount: 2,
  labelWidth: 120,
  valueWidth: 200,
  labelAlign: 'right',
  LabelComponent: DefaultLabel,
})

/** Column の中にあるかどうかを子孫コンポーネント側が知るためのコンテキスト */
export const IsInColumnContext = React.createContext<boolean>(false)
