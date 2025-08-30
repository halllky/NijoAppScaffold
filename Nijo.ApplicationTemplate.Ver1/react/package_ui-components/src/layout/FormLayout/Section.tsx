import React from "react"
import { BorderPaddingContext, FormLayoutContext, RecentParentContext } from "./internal-context"

import "./Section.css"
import { LabelRenderer } from "./internal-label-renderer"

export type SectionProps = {
  /** ラベル */
  label?: string
  /** ラベルの後ろに挿入される */
  labelEnd?: React.ReactNode
  /**
   * レスポンシブレイアウトの場合に指定する。
   * この子要素は、コンテナ横方向に余裕がある場合は横方向に並ぶ。
   * 数値を指定した場合、格納可能な列の数がその数値を超えた時のみ横方向に並ぶ。（規定値は2）
   */
  responsive?: boolean | number
  /** 枠線を表示するかどうか */
  border?: boolean
  /** コンテンツ */
  children?: React.ReactNode
  className?: string
}

/**
 * 複数のItemまたはColumnを囲むグループ。
 */
export const Section = React.forwardRef<HTMLDivElement, SectionProps>((props, ref) => {

  const parent = React.useContext(RecentParentContext)
  const ancestorBorderPadding = React.useContext(BorderPaddingContext)
  const { columnCount, labelWidth, valueWidth } = React.useContext(FormLayoutContext)

  // ワイドレイアウトの場合は、それぞれの Column を横方向に並べる。
  // ※ style属性だけでは「最後の子要素」を実現できないのでCSSファイルで定義している。
  const isResponsive = typeof props.responsive === 'number'
    ? columnCount >= props.responsive
    : props.responsive && columnCount >= 2
  if (isResponsive) {

    // 横方向並べ ... display: flex による横一直線のレイアウト
    return (
      <RecentParentContext.Provider value="responsive-container">
        <BorderPaddingContext.Provider value={ancestorBorderPadding + (props.border ? 5 : 0)}>
          <div ref={ref} className={props.className} style={{
            gridColumn: parent === '2-cols-grid' ? 'span 2' : undefined,
            display: 'flex',
            flexDirection: 'column',
            rowGap: '4px',
            // コンテナの横幅がきわめて小さい場合に要素が親の右側にはみ出るのを防ぐ
            overflowX: parent === undefined ? 'auto' : undefined,
          }}>
            {/* ラベル */}
            {(props.label || props.labelEnd) && (
              <LabelRenderer
                label={props.label}
                labelEnd={props.labelEnd}
                align="full"
              />
            )}

            {/* コンテンツ */}
            <div className="form-layout-section-responsive-column-group" style={{
              display: 'flex',
              gap: '4px',
              alignItems: 'start',
              border: props.border ? '1px solid #ccc' : undefined,
              padding: props.border ? '4px' : undefined,
            }}>
              {props.children}
            </div>
          </div>
        </BorderPaddingContext.Provider>
      </RecentParentContext.Provider>
    )
  }

  // 縦方向並べ ... display: grid による2列構成のグリッド
  return (
    <RecentParentContext.Provider value="2-cols-grid">
      <BorderPaddingContext.Provider value={ancestorBorderPadding + (props.border ? 5 : 0)}>
        <div ref={ref} className={props.className} style={{
          gridColumn: parent === '2-cols-grid' ? 'span 2' : undefined,
          display: 'grid',
          gap: '4px',
          alignItems: 'stretch',
          gridTemplateColumns: parent === '2-cols-grid'
            ? 'subgrid'
            : `${labelWidth - ancestorBorderPadding}px minmax(${valueWidth}px, 1fr)`,
          // コンテナの横幅がきわめて小さい場合に要素が親の右側にはみ出るのを防ぐ
          overflowX: parent === undefined ? 'auto' : undefined,
        }}>
          {/* ラベル */}
          {(props.label || props.labelEnd) && (
            <LabelRenderer
              label={props.label}
              labelEnd={props.labelEnd}
              style={{ gridColumn: 'span 2' }}
              align="full"
            />
          )}

          {/* コンテンツ */}
          {props.border ? (
            <div style={{
              display: 'grid',
              rowGap: '4px',
              gridTemplateColumns: 'subgrid',
              gridColumn: 'span 2',
              border: '1px solid #ccc',
              padding: '4px',
            }}>
              {props.children}
            </div>
          ) : (
            props.children
          )}
        </div>
      </BorderPaddingContext.Provider>
    </RecentParentContext.Provider>
  )
})
