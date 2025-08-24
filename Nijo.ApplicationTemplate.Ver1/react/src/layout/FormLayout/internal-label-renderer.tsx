import React from "react"
import { FormLayoutContext } from "./internal-context"

/** ラベルは複数個所で登場するのでレンダリング処理を共通化するためのコンポーネント */
export const LabelRenderer = ({ label, labelEnd, style, alignRight }: {
  label?: string
  labelEnd?: React.ReactNode
  style?: React.CSSProperties
  alignRight?: boolean
}) => {

  const { LabelComponent } = React.useContext(FormLayoutContext)

  return (
    <div style={{
      ...style,
      display: 'flex',
      alignItems: 'start',
      justifyContent: alignRight ? 'flex-end' : undefined,
      textAlign: alignRight ? 'right' : undefined,
    }}>
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: alignRight ? 'flex-end' : undefined,
        flexWrap: 'wrap',
        gap: '4px',
      }}>
        {label && (
          <LabelComponent>{label}</LabelComponent>
        )}
        {labelEnd}
      </div>
    </div>
  )
}
