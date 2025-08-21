import React from "react"
import { FormLayoutContext, FormLayoutContextValue } from "./ResponsiveFormContext"

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

/** レスポンシブフォームのもっとも外側に配置されるコンテナ */
export const Root = (props: ResponsiveFormProps) => {

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

  // レスポンシブレイアウト用の計算
  const { labelWidth, valueWidth } = React.useMemo(() => {
    const labelWidth = props.labelWidthPx ?? 120
    const valueWidth = props.valueWidthPx ?? 200
    return { labelWidth, valueWidth }
  }, [props.labelWidthPx, props.valueWidthPx])

  // 1段組みか2段組みかを判定する
  const isWideLayout = React.useMemo(() => {
    // 2段組みレイアウトには2組分（ラベル+値）×2の幅が必要。
    // 16はだいたいのマージンの幅。
    const breakpoint = (labelWidth + valueWidth) * 2 + 16
    return containerWidth >= breakpoint
  }, [containerWidth, labelWidth, valueWidth])

  // コンテキスト
  const contextValue: FormLayoutContextValue = React.useMemo(() => ({
    isWideLayout,
    labelWidth,
    valueWidth,
    labelAlign: props.labelAlign ?? 'right',
  }), [isWideLayout, labelWidth, valueWidth, props.labelAlign])


  return (
    <FormLayoutContext.Provider value={contextValue}>
      <div
        ref={containerRef}
        className={`flex flex-col gap-1 items-stretch ${props.className ?? ''}`}
      >
        {props.children}
      </div>
    </FormLayoutContext.Provider>
  )
}
