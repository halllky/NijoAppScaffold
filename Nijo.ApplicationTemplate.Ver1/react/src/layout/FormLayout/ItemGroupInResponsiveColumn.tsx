import React from "react"
import { LabelRenderer } from "./internal-label-renderer"

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
 * `ResponsiveColumn` の中での使用を想定している。
 */
export const ItemGroupInResponsiveColumn = (props: ItemGroupProps) => {

  return (
    <>
      {/* ラベル */}
      {(props.label || props.labelEnd) && (
        <LabelRenderer
          label={props.label}
          labelEnd={props.labelEnd}
          style={{ gridColumn: 'span 2' }}
        />
      )}

      {/* コンテンツ */}
      <div style={{
        gridColumn: 'span 2',
        display: 'grid',
        gridTemplateColumns: 'subgrid',
        gap: '4px',
        padding: '8px',
        border: '1px solid #ccc',
      }}>
        {props.children}
      </div>
    </>
  )
}
