import React from "react"
import { FormLayoutContext, FormLayoutContextValue, RecentParentContext } from "./internal-context"
import { DefaultLabel, LabelProps } from "./DefaultLabel"
import { Section } from "./Section"

/** フォームレイアウトのプロパティ */
export type FormLayoutProps = {
  /** ラベル列の幅。未指定の場合は既定の幅が使用される。 */
  labelWidthPx?: number
  /** 値列の幅。レスポンシブレイアウトの判定に使用される。 */
  valueWidthPx?: number
  /** ルート要素に適用される。スタイルの微調整に用いる。 */
  className?: string
  /** ラベルのテキストの位置。デフォルトは右寄せ。 */
  labelAlign?: 'left' | 'right'
  /** ラベルのコンポーネント。未指定の場合は既定のコンポーネントが使用される。 */
  labelComponent?: React.ElementType<LabelProps>
  /** 子要素 */
  children?: React.ReactNode
}

/** FormLayoutのルートコンテナ */
export const Root = (props: FormLayoutProps) => {

  // レスポンシブレイアウト用の状態管理
  const [containerWidth, setContainerWidth] = React.useState<number>(0)
  const containerRef = React.useRef<HTMLDivElement>(null)

  // コンテナの幅を監視
  React.useEffect(() => {
    const container = containerRef.current
    if (!container) return

    // 初期幅を即時反映（ResizeObserver の初回発火を待たない）
    setContainerWidth(container.getBoundingClientRect().width)

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

  // 何段組みまで可能かを判定する
  const columnCount = React.useMemo(() => {
    // ラベル+値の1ユニットと、列間ギャップ（概算）を考慮して収まる列数を算出
    // 列間ギャップを g、ユニット幅を w、コンテナ幅を W とすると、
    //   W >= col*w + (col-1)*g  <=>  W + g >= col*(w + g)
    // よって  col = floor((W + g) / (w + g))
    const gapPx = 8
    const paddingZakkuri = 24 // 一般的な使い方だとだいたいこのぐらいのパディングが入る
    const unitWidth = labelWidth + valueWidth
    const cols = Math.floor((containerWidth + gapPx) / (unitWidth + gapPx + paddingZakkuri))
    return Math.max(1, cols)
  }, [containerWidth, labelWidth, valueWidth])

  // コンテキスト
  const contextValue: FormLayoutContextValue = React.useMemo(() => ({
    columnCount,
    labelWidth,
    valueWidth,
    labelAlign: props.labelAlign ?? 'right',
    LabelComponent: props.labelComponent ?? DefaultLabel,
  }), [columnCount, labelWidth, valueWidth, props.labelAlign, props.labelComponent])

  return (
    <FormLayoutContext.Provider value={contextValue}>
      <Section ref={containerRef} className={props.className}>
        {props.children}
      </Section>
    </FormLayoutContext.Provider>
  )
}
