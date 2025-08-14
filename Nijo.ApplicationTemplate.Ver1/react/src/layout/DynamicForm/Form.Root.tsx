import React from "react"
import * as ReactHookForm from "react-hook-form"
import { VForm2 } from "../VForm2"
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
    const { root, membersTypes } = props
    return {
      props: { root, membersTypes },
      useFormReturn,
    }
  }, [props.root, props.membersTypes, useFormReturn])

  // フォームの深さ
  const formDepth = React.useMemo(() => {
    return countFormDepth(props.root)
  }, [props.root])

  return (
    <DynamicFormContext.Provider value={contextValue}>
      <VForm2.Root estimatedLabelWidth="10rem" maxDepth={formDepth} className={props.className}>
        <MembersGroupByBreakPoint owner={props.root} ancestorsPath="" />
      </VForm2.Root>
    </DynamicFormContext.Provider>
  )
})
