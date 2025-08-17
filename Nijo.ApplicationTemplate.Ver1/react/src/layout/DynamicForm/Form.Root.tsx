import React from "react"
import * as ReactHookForm from "react-hook-form"
import { DynamicFormProps, DynamicFormRef } from "./types"
import { DynamicFormContext, DynamicFormContextValue } from "./DynamicFormContext"
import { countFormDepth } from "./helpers"
import { MembersGroupByBreakPoint } from "./Form.Members"

/**
 * 扱うデータ構造が動的に決まるフォーム。
 */
export const DynamicForm = React.forwardRef<DynamicFormRef, DynamicFormProps>((props, ref) => {

  // react hook form
  const useFormReturn = ReactHookForm.useForm<ReactHookForm.FieldValues>({
    defaultValues: props.defaultValues,
  })

  // ref
  React.useImperativeHandle(ref, () => ({
    useFormReturn,
  }), [useFormReturn])

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
  const { isWideLayout, gridStyle } = React.useMemo(() => {
    const labelWidth = (countFormDepth(props.root) * 4) + (props.labelWidthPx ?? 120)
    const valueWidth = props.valueWidthPx ?? 200
    // 4列レイアウトには2組分（ラベル+値）×2の幅が必要
    const breakpoint = (labelWidth + valueWidth) * 2

    // コンテナの幅がブレークポイントより大きい場合は4列、そうでなければ2列
    const isWideLayout = containerWidth >= breakpoint

    const gridStyle: React.CSSProperties = isWideLayout
      ? {
        // 4列レイアウト: ラベル、値、ラベル、値
        gridTemplateColumns: `${labelWidth}px ${valueWidth}px ${labelWidth}px 1fr`,
      }
      : {
        // 2列レイアウト: ラベル、値
        gridTemplateColumns: `${labelWidth}px 1fr`,
      }

    return { isWideLayout, gridStyle }
  }, [props.root, props.labelWidthPx, props.valueWidthPx, containerWidth])

  // コンテキスト（レイアウト情報を含む）
  const contextValue: DynamicFormContextValue = React.useMemo(() => {
    const { root } = props
    return {
      props: { root },
      useFormReturn,
      isWideLayout,
    }
  }, [props.root, useFormReturn, isWideLayout])

  return (
    <DynamicFormContext.Provider value={contextValue}>
      <div
        ref={containerRef}
        className={`grid ${props.className ?? ''}`}
        style={gridStyle}
      >
        <MembersGroupByBreakPoint owner={props.root} ancestorsPath="" />
      </div>
    </DynamicFormContext.Provider>
  )
})
