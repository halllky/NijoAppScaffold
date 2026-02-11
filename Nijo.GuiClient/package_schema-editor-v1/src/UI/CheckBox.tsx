import React from "react"
import * as ReactHookForm from "react-hook-form"

type CheckBoxProps = Omit<React.InputHTMLAttributes<HTMLInputElement>, 'value' | 'checked' | 'onChange'> & {
  control: ReactHookForm.Control<any>
  name: string
}

/**
 * サーバー側ではbool値ではなく "True" or "" で管理しているのでそれにあわせたチェックボックス
 */
export const CheckBox = React.forwardRef<HTMLInputElement, CheckBoxProps>(({ control, name, className, ...props }, ref) => {
  return (
    <ReactHookForm.Controller
      control={control}
      name={name}
      render={({ field: { onChange, value, ref: fieldRef, ...fieldRest } }) => (
        <input
          type="checkbox"
          className={`h-4 w-4 ${className ?? ''}`}
          checked={!!value}
          onChange={e => onChange(toServerValue(e.target.checked))}
          ref={(e) => {
            fieldRef(e)
            if (typeof ref === 'function') ref(e)
            else if (ref) ref.current = e
          }}
          {...fieldRest}
          {...props}
        />
      )}
    />
  )
})

/**
 * サーバー側ではbool値ではなく "True" or "" で管理しているのでそれにあわせる
 */
export function toServerValue(value: unknown): 'True' | '' {
  return value === true || value === 'True' ? 'True' : ''
}
