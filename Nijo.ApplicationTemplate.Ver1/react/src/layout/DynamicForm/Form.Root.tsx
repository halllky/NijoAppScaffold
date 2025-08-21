import React from "react"
import * as ReactHookForm from "react-hook-form"
import { DynamicFormProps, DynamicFormRef } from "./types"
import { DynamicFormContext, DynamicFormContextValue } from "./DynamicFormContext"
import { MembersGroupByBreakPoint } from "./Form.Members"
import ResponsiveForm from "../ResponsiveForm"

/**
 * 扱うデータ構造が動的に決まるフォーム。
 */
export const DynamicForm = React.forwardRef<DynamicFormRef, DynamicFormProps>(({
  root,
  defaultValues,
  ...responsiveFormProps
}, ref) => {

  // react hook form
  const useFormReturn = ReactHookForm.useForm<ReactHookForm.FieldValues>({
    defaultValues,
  })

  // ref
  React.useImperativeHandle(ref, () => ({
    useFormReturn,
  }), [useFormReturn])

  // コンテキスト
  const contextValue: DynamicFormContextValue = React.useMemo(() => ({
    props: { root },
    useFormReturn,
  }), [root, useFormReturn])

  return (
    <ResponsiveForm.Container {...responsiveFormProps}>
      <DynamicFormContext.Provider value={contextValue}>
        <MembersGroupByBreakPoint owner={root} ancestorsPath="" />
      </DynamicFormContext.Provider>
    </ResponsiveForm.Container>
  )
})
