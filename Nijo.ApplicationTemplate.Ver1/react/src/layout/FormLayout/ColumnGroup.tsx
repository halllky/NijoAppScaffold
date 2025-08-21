import React from "react"
import { Label } from "./Label"
import { FormLayoutContext } from "./ResponsiveFormContext"

export type ColumnGroupProps = {
  /** ラベル */
  label?: string
  /** ラベルの後ろに挿入される */
  labelEnd?: React.ReactNode
  /** コンテンツ */
  children?: React.ReactNode
}

/**
 * レスポンシブなセクション。
 * `Root` の直下に配置する。
 * この直下に `Column` を配置する。
 */
export const ColumnGroup = (props: ColumnGroupProps) => {

  const { isWideLayout } = React.useContext(FormLayoutContext)

  return (
    <div className="flex flex-col gap-1">
      {/* ラベル */}
      {(props.label || props.labelEnd) && (
        <div className="col-span-2 flex items-center flex-wrap gap-1">
          {props.label && (
            <Label>{props.label}</Label>
          )}
          {props.labelEnd}
        </div>
      )}

      {/* コンテンツ */}
      {isWideLayout ? (
        // ワイドレイアウトの場合は、それぞれの Column を横方向に並べる。
        <div className="flex gap-2 items-start [&>*:last-child]:flex-1">
          {props.children}
        </div>
      ) : (
        // ワイドではないので縦方向に並べる
        <div className="flex flex-col gap-1 items-stretch">
          {props.children}
        </div>
      )}
    </div>
  )
}
