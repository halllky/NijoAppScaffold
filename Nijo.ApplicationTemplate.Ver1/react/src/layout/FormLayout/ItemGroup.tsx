import React from "react"
import { Label } from "./Label"
import { IsInColumnContext } from "./ResponsiveFormContext"
import { Column } from "./Column"

export type ItemGroupProps = {
  /** ラベル */
  label?: string
  /** ラベルの後ろに挿入される */
  labelEnd?: React.ReactNode
  /** グループの内容 */
  children?: React.ReactNode
}

/**
 * 複数の `Item` を囲む枠。
 */
export const ItemGroup = (props: ItemGroupProps) => {

  // Column の中にあるかどうか
  const isInColumn = React.useContext(IsInColumnContext)

  return (
    <>
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
      {isInColumn ? (
        // Column の中にある場合は、そのまま子要素を配置する
        <div className="grid col-span-2 gap-1 p-2 grid-cols-[subgrid] border border-gray-300">
          {props.children}
        </div>
      ) : (
        // Column の外にある場合
        <Column className="gap-1 p-2 border border-gray-300">
          {props.children}
        </Column>
      )}
    </>
  )
}
