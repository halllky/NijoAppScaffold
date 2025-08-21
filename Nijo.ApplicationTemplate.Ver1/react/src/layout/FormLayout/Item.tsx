import React from "react"
import { Label } from "./Label"
import { IsInColumnContext, FormLayoutContext } from "./ResponsiveFormContext"

export type ItemProps = {
  /** 値をラベルの右ではなく下に表示する */
  vertical?: boolean
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

  // Column の外にあり、かつverticalでない場合、
  // ラベル + 残りの幅全部コンテンツ
  if (!isInColumn && !props.vertical) {
    return (props.label || props.labelEnd) ? (
      // 枠とパディングの分だけラベルとコンテンツの境界部分がずれるので
      // それらの影響を受けないdivを外側に配置、内部のgridはsubgridを使う
      <div className="grid" style={{ gridTemplateColumns: `${labelWidth}px 1fr` }}>
        <div className="col-span-full grid grid-cols-[subgrid] gap-1">
          {/* ラベル */}
          <div className={`flex items-center flex-wrap gap-1 col-span-1 ${labelAlign === 'right' ? 'justify-end' : ''}`}>
            {props.label && (
              <Label>{props.label}</Label>
            )}
            {props.labelEnd}
          </div>

          {/* コンテンツ */}
          <div className="col-span-[2/-1]">
            {props.children}
          </div>
        </div>
      </div>
    ) : (
      // コンテンツのみ
      <div>
        {props.children}
      </div>
    )
  }

  // Column の中にある場合は CSS Grid が適用されているので React Fragment の中にdivを2つ並べる
  return (
    <>
      {/* ラベル */}
      {(props.label || props.labelEnd) && (
        <div className={`flex items-center flex-wrap gap-1 ${props.vertical ? 'col-span-full' : ''} ${!props.vertical && labelAlign === 'right' ? 'justify-end' : ''}`}>
          {props.label && (
            <Label>{props.label}</Label>
          )}
          {props.labelEnd}
        </div>
      )}
      {/* コンテンツ */}
      <div className={props.vertical ? 'col-span-full' : ''}>
        {props.children}
      </div>
    </>
  )
}
