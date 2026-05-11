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

/**
 * 直近の親がどれかを子コンポーネント側が知るためのコンテキスト。
 * 親によってCSSが異なるため。
 */
export const RecentParentContext = React.createContext<
  | undefined
  | '2-cols-grid'
  | 'responsive-container'
  | 'item'
>(undefined)

/**
 * 直近の親が枠線を表示するかどうかを子コンポーネント側が知るためのコンテキスト。
 * 祖先の枠のパディングの累計がこのコンテキストに設定される。
 * 枠があるとパディングの分だけ subgrid の幅が小さくなるので子側で補正する必要がある。
 */
export const BorderPaddingContext = React.createContext<number>(0)
