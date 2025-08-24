import React from "react"
import { RecentParentContext, FormLayoutContext } from "./internal-context"
import { LabelRenderer } from "./internal-label-renderer"

export type ItemProps = {
  /** 通常はラベルが左、値が右に表示されるが、これをtrueにするとラベルと値を縦に並べ、横幅いっぱいに表示する。 */
  fullWidth?: boolean
  /** ラベル */
  label?: string
  /** ラベルの後ろに挿入される */
  labelEnd?: React.ReactNode
  children?: React.ReactNode
}

/** コンテンツ */
export const Item = (props: ItemProps): React.ReactNode => {
  const { labelAlign, labelWidth } = React.useContext(FormLayoutContext)
  const parent = React.useContext(RecentParentContext)

  // Group(レスポンシブコンテナ) の中にある場合
  if (parent === 'responsive-container') {
    return (
      <RecentParentContext.Provider value="item">
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '4px',
        }}>
          {/* ラベル */}
          {(props.label || props.labelEnd) && (
            <LabelRenderer
              label={props.label}
              labelEnd={props.labelEnd}
            />
          )}
          {/* コンテンツ */}
          <div>
            {props.children}
          </div>
        </div>
      </RecentParentContext.Provider>
    )
  }

  // Group (display: grid) の中にある場合は CSS Grid が適用されているので React Fragment の中にdivを2つ並べる
  if (parent === '2-cols-grid') {

    const fullWidth = props.fullWidth || !props.label && !props.labelEnd

    return (
      <RecentParentContext.Provider value="item">
        {/* ラベル */}
        {(props.label || props.labelEnd) && (
          <LabelRenderer
            label={props.label}
            labelEnd={props.labelEnd}
            alignRight={!fullWidth && labelAlign === 'right'}
            style={{
              gridColumn: fullWidth ? 'span 2' : undefined,
            }}
          />
        )}
        {/* コンテンツ */}
        <div style={{ gridColumn: fullWidth ? 'span 2' : undefined }}>
          {props.children}
        </div>
      </RecentParentContext.Provider>
    )
  }

  // ************ 以下、 Item の中に Item がある特殊ケース **************
  // ※ 推奨されない使い方なのである程度それっぽく表示できていればよい

  // ラベルなし
  if (!props.label && !props.labelEnd) {
    return (
      <RecentParentContext.Provider value="item">
        <div>
          {props.children}
        </div>
      </RecentParentContext.Provider>
    )
  }

  // 値を横幅いっぱい表示するケース
  if (props.fullWidth) {
    return (
      <RecentParentContext.Provider value="item">
        {/* ラベル */}
        <LabelRenderer
          label={props.label}
          labelEnd={props.labelEnd}
        />
        {/* コンテンツ */}
        <div>
          {props.children}
        </div>
      </RecentParentContext.Provider>
    )
  }

  // 値をラベルの右に表示するケース
  return (
    <RecentParentContext.Provider value="item">
      <div style={{
        display: 'grid',
        gridTemplateColumns: `${labelWidth}px 1fr`,
      }}>
        {/* ラベル */}
        <LabelRenderer
          label={props.label}
          labelEnd={props.labelEnd}
          alignRight={labelAlign === 'right'}
          style={{ padding: '0 4px' }}
        />

        {/* コンテンツ */}
        <div>
          {props.children}
        </div>
      </div>
    </RecentParentContext.Provider>
  )
}
