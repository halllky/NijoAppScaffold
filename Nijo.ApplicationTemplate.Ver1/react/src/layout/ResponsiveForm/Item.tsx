import React from "react"
import { Label } from "./Label"
import { ResponsiveFormContext } from "./ResponsiveFormContext"

export type ItemProps = {
  /** 2段組みレイアウトのとき横幅いっぱいにする */
  fullWidth?: boolean
  /** ラベル */
  label?: string
  /** ラベルの後ろに挿入される */
  labelEnd?: React.ReactNode
  children?: React.ReactNode
}

/**
 * ラベルと値の組。
 */
export const Item = (props: ItemProps): React.ReactNode => {
  const { labelAlign } = React.useContext(ResponsiveFormContext)
  return (
    <>
      {/* ラベル */}
      {(props.label || props.labelEnd) && (
        <div style={{
          textAlign: !props.fullWidth && labelAlign === 'right' ? 'right' : undefined,
          gridColumn: props.fullWidth ? 'span 2' : undefined,
        }}>
          {props.label && (
            <Label>
              {props.label}
            </Label>
          )}

          {props.labelEnd}
        </div>
      )}

      {/* 値 */}
      <div style={{
        gridColumn: props.fullWidth ? 'span 2' : undefined,
      }}>
        {props.children}
      </div>
    </>
  )
}
