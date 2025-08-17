import React from "react"
import * as ReactHookForm from "react-hook-form"
import { DynamicFormProps } from "./types"

/** フォーム内部で使用するコンテキスト */
export type DynamicFormContextValue = {
  /** フォームのプロパティ */
  props: Pick<DynamicFormProps, "root">
  /** react-hook-form の useForm の戻り値 */
  useFormReturn: ReactHookForm.UseFormReturn<ReactHookForm.FieldValues>
}

/** フォーム内部で使用するコンテキスト */
export const DynamicFormContext = React.createContext<DynamicFormContextValue>({
  props: {} as DynamicFormProps,
  useFormReturn: {} as ReactHookForm.UseFormReturn<ReactHookForm.FieldValues>,
})