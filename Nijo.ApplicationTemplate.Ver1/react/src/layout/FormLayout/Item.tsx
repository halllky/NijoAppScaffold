import React from "react"
import { IsInColumnContext, FormLayoutContext } from "./internal-context"
import { LabelRenderer } from "./internal-label-renderer"

export type ItemProps = {
  /** 値を横幅いっぱい表示する */
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
  const { labelAlign, labelWidth } = React.useContext(FormLayoutContext)
  const isInColumn = React.useContext(IsInColumnContext)

  // Column の外にある場合は CSS Grid の考慮をここで行なう
  if (!isInColumn) {

    // ラベルなし
    if (!props.label && !props.labelEnd) {
      return (
        <div>
          {props.children}
        </div>
      )
    }

    // 値を横幅いっぱい表示するケース
    if (props.fullWidth) {
      return (
        <>
          {/* ラベル */}
          <LabelRenderer
            label={props.label}
            labelEnd={props.labelEnd}
          />
          {/* コンテンツ */}
          <div>
            {props.children}
          </div>
        </>
      )
    }

    // 値をラベルの右に表示するケース
    return (
      <div style={{
        display: 'grid',
        gridTemplateColumns: `${labelWidth}px 1fr`,
        gap: '4px',
      }}>
        {/* ラベル */}
        <LabelRenderer
          label={props.label}
          labelEnd={props.labelEnd}
          style={{ justifyContent: labelAlign === 'right' ? 'flex-end' : undefined }}
        />

        {/* コンテンツ */}
        <div>
          {props.children}
        </div>
      </div>
    )
  }

  // Column の中にある場合は CSS Grid が適用されているので React Fragment の中にdivを2つ並べる
  return (
    <>
      {/* ラベル */}
      {(props.label || props.labelEnd) && (
        <LabelRenderer
          label={props.label}
          labelEnd={props.labelEnd}
          style={{
            gridColumn: props.fullWidth ? 'span 2' : undefined,
            justifyContent: !props.fullWidth && labelAlign === 'right' ? 'flex-end' : undefined,
          }}
        />
      )}
      {/* コンテンツ */}
      <div style={{ gridColumn: props.fullWidth ? 'span 2' : undefined }}>
        {props.children}
      </div>
    </>
  )
}
