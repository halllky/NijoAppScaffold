import React from "react"
import * as ReactHookForm from "react-hook-form"
import { DynamicFormProps, DynamicFormRef, SectionMember } from "./types"
import { DynamicFormContext, DynamicFormContextValue } from "./DynamicFormContext"
import { MembersGroupByBreakPoint } from "./Form.Members"
import FormLayout from "../FormLayout"

/**
 * 扱うデータ構造が動的に決まるフォーム。
 */
export const DynamicForm = React.forwardRef<DynamicFormRef, DynamicFormProps>(({
  root,
  defaultValues,
  isReadOnly,
  ...responsiveFormProps
}, ref) => {

  // react hook form
  const useFormReturn = ReactHookForm.useForm<ReactHookForm.FieldValues>({
    defaultValues,
  })

  // ref
  React.useImperativeHandle(ref, () => ({
    useFormReturn,
    consoleLog: () => console.log(root),
  }), [useFormReturn, root])

  // コンテキスト
  const contextValue: DynamicFormContextValue = React.useMemo(() => ({
    props: { root, isReadOnly },
    useFormReturn,
  }), [root, isReadOnly, useFormReturn])

  return (
    <FormLayout.Root {...responsiveFormProps}>
      <DynamicFormContext.Provider value={contextValue}>
        <MembersGroupByBreakPoint
          owner={root}
          ancestorsPath={(root as SectionMember).physicalName ?? ''}
        />
      </DynamicFormContext.Provider>
    </FormLayout.Root>
  )
})
