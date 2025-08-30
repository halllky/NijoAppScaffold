import React from "react"
import { FormLayoutContext } from "./internal-context"

/** ラベルは複数個所で登場するのでレンダリング処理を共通化するためのコンポーネント */
export const LabelRenderer = ({ label, labelEnd, style, align }: {
  label?: string
  labelEnd?: React.ReactNode
  style?: React.CSSProperties
  align?: 'right' | 'full' | 'left'
}) => {

  const { LabelComponent } = React.useContext(FormLayoutContext)

  if (align === 'full') {
    return (
      <div style={{
        ...style,
        display: 'flex',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: '4px',
      }}>
        {label && (
          <LabelComponent>{label}</LabelComponent>
        )}
        {labelEnd}
      </div>
    )
  }

  return (
    <div style={{
      ...style,
      display: 'flex',
      alignItems: 'start',
      justifyContent: align === 'right' ? 'flex-end' : undefined,
      textAlign: align === 'right' ? 'right' : undefined,
    }}>
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: align === 'right' ? 'flex-end' : undefined,
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
