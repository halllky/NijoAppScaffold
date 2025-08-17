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

  // コンテキスト
  const contextValue: DynamicFormContextValue = React.useMemo(() => {
    const { root } = props
    return {
      props: { root },
      useFormReturn,
    }
  }, [props.root, useFormReturn])

  // ラベル列の横幅
  const gridStyle: React.CSSProperties = React.useMemo(() => ({
    gridTemplateColumns: `${(countFormDepth(props.root) * 4) + (props.labelWidthPx ?? 120)}px 1fr`,
  }), [props.root, props.labelWidthPx])

  return (
    <DynamicFormContext.Provider value={contextValue}>
      <div className={`grid ${props.className ?? ''}`} style={gridStyle}>
        <MembersGroupByBreakPoint owner={props.root} ancestorsPath="" />
      </div>
    </DynamicFormContext.Provider>
  )
})
