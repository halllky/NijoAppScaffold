import React from "react"
import { FormLayoutContext } from "./internal-context"

import "./ResponsiveColumnGroup.css"

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
 * この直下に `ResponsiveColumn` を配置する。
 */
export const ResponsiveColumnGroup = (props: ColumnGroupProps) => {

  const { columnCount, LabelComponent } = React.useContext(FormLayoutContext)

  return (
    <div style={{
      display: 'flex',
      flexDirection: 'column',
      gap: '4px',
    }}>
      {/* ラベル */}
      {(props.label || props.labelEnd) && (
        <div style={{
          display: 'flex',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '4px',
        }}>
          {props.label && (
            <LabelComponent>{props.label}</LabelComponent>
          )}
          {props.labelEnd}
        </div>
      )}

      {/* コンテンツ */}
      {columnCount >= 2 ? (
        // ワイドレイアウトの場合は、それぞれの Column を横方向に並べる。
        // style属性だけでは「最後の子要素」を実現できないのでCSSファイルで定義している。
        <div className="responsive-column-group" style={{
          display: 'flex',
          gap: '16px',
          alignItems: 'start',
        }}>
          {props.children}
        </div>
      ) : (
        // ワイドではないので縦方向に並べる
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '4px',
          alignItems: 'stretch',
        }}>
          {props.children}
        </div>
      )}
    </div>
  )
}
