import React from "react"
import { ResponsiveFormContext, ResponsiveFormContextValue } from "./ResponsiveFormContext"
import { toLayoutedChildren } from "./toLayoutedChildren"

/** レスポンシブフォームのプロパティ */
export type ResponsiveFormProps = {
  /** ラベル列の幅。未指定の場合は既定の幅が使用される。 */
  labelWidthPx?: number
  /** 値列の幅。レスポンシブレイアウトの判定に使用される。 */
  valueWidthPx?: number
  /** ルート要素に適用される。スタイルの微調整に用いる。 */
  className?: string
  /** ラベルのテキストの位置。デフォルトは右寄せ。 */
  labelAlign?: 'left' | 'right'
  /** 子要素 */
  children?: React.ReactNode
}

/** 1段組み、2段組みを自動的に切り替えるレスポンシブフォーム */
export const Container = (props: ResponsiveFormProps) => {

  // レスポンシブレイアウト用の状態管理
  const [containerWidth, setContainerWidth] = React.useState<number>(0)
  const containerRef = React.useRef<HTMLDivElement>(null)

  // コンテナの幅を監視
  React.useEffect(() => {
    const container = containerRef.current
    if (!container) return

    const resizeObserver = new ResizeObserver((entries) => {
      for (const entry of entries) {
        setContainerWidth(entry.contentRect.width)
      }
    })

    resizeObserver.observe(container)
    return () => resizeObserver.disconnect()
  }, [])

  // 1段組みか2段組みかを判定する
  const isWideLayout = React.useMemo(() => {
    const labelWidth = props.labelWidthPx ?? 120
    const valueWidth = props.valueWidthPx ?? 200
    // 2段組みレイアウトには2組分（ラベル+値）×2の幅が必要。
    // 16はだいたいのマージンの幅。
    const breakpoint = (labelWidth + valueWidth) * 2 + 16
    return containerWidth >= breakpoint
  }, [containerWidth, props.labelWidthPx, props.valueWidthPx])

  // レスポンシブレイアウト用の計算
  const { labelWidth, valueWidth, gridStyle } = React.useMemo(() => {
    const labelWidth = props.labelWidthPx ?? 120
    const valueWidth = props.valueWidthPx ?? 200

    const gridStyle: React.CSSProperties = isWideLayout
      ? {
        // 4列レイアウト: ラベル、値、ラベル、値
        gridTemplateColumns: `${labelWidth}px ${valueWidth}px ${labelWidth}px 1fr`,
      }
      : {
        // 2列レイアウト: ラベル、値
        gridTemplateColumns: `${labelWidth}px 1fr`,
      }

    return { labelWidth, valueWidth, gridStyle }
  }, [props.labelWidthPx, props.valueWidthPx, isWideLayout])

  // コンテキスト
  const contextValue: ResponsiveFormContextValue = React.useMemo(() => ({
    isWideLayout,
    labelWidth,
    valueWidth,
    labelAlign: props.labelAlign ?? 'right',
  }), [isWideLayout, labelWidth, valueWidth, props.labelAlign])


  return (
    <ResponsiveFormContext.Provider value={contextValue}>
      <div
        ref={containerRef}
        className={`flex flex-col items-stretch ${props.className ?? ''}`}
        style={gridStyle}
      >
        {toLayoutedChildren(props.children, isWideLayout, labelWidth, valueWidth)}
      </div>
    </ResponsiveFormContext.Provider>
  )
}
