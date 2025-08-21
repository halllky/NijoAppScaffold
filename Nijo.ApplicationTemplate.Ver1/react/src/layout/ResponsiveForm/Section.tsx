import React from "react"
import { Label } from "./Label"
import { toLayoutedChildren } from "./toLayoutedChildren"
import { ResponsiveFormContext } from "./ResponsiveFormContext"

export type SectionProps = {
  /** ラベル */
  label?: string
  /** ラベルの後ろに挿入される */
  labelEnd?: React.ReactNode
  /** セクションの内容 */
  children?: React.ReactNode
  /** フル幅で表示するかどうか */
  fullWidth?: boolean
}

/**
 * `FormItem` のグルーピングを行う。
 *
 * * このコンポーネントは `Root` の直下に配置される必要がある。
 * * このコンポーネントの中に `FormItem` を配置することができる。
 */
export const Section = (props: SectionProps) => {

  const { isWideLayout, labelWidth, valueWidth } = React.useContext(ResponsiveFormContext)

  return (
    <>
      {/* ラベル */}
      {(props.label || props.labelEnd) && (
        <div className="col-span-2">
          {props.label && (
            <Label>{props.label}</Label>
          )}
          {props.labelEnd}
        </div>
      )}

      {/* コンテンツ */}
      {props.fullWidth ? (
        <div className="flex flex-col items-stretch">
          {toLayoutedChildren(props.children, isWideLayout, labelWidth, valueWidth)}
        </div>
      ) : (
        <div className="grid col-span-2 gap-1 p-2 grid-cols-[subgrid] border border-gray-300">
          {props.children}
        </div>
      )}
    </>

  )
}