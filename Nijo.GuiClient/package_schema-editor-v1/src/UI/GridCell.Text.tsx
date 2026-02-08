import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import { ReadOnlyMentionText } from "./MentionInputWrapper"
import { SchemaDefinitionMentionTextarea } from "./Mention"

export type CreateTextCellFunction = <TRow>(
  header: string,
  key: ReactHookForm.Path<TRow>,
  options?: Partial<EG2.EditableGrid2LeafColumn<TRow>> & {
    format?: (value: unknown) => string
    parse?: (value: string) => unknown
    /** メンション使用可能かどうか */
    mentionAvailable?: boolean
  }
) => EG2.EditableGrid2LeafColumn<TRow>

/**
 * EditableGrid2 のテキスト列
 */
export function createTextCellHelper(
  getValues: ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>,
  setValue: ReactHookForm.UseFormSetValue<ReactHookForm.FieldValues>,
  control: ReactHookForm.Control<ReactHookForm.FieldValues>,
  arrayName: string,
  skipFirstRow: boolean | undefined,
): CreateTextCellFunction {

  return (header, key, options) => ({
    editor: options?.mentionAvailable
      ? CommentCellEditor
      : TextCellEditor,
    renderHeader: () => (
      <div className="px-1 py-px truncate text-sm text-gray-700">
        {header}
      </div>
    ),
    renderBody: ({ context }) => {
      const fieldRowIndex = skipFirstRow ? context.row.index + 1 : context.row.index
      const value: string | null | undefined = ReactHookForm.useWatch({
        control,
        name: `${arrayName}.${fieldRowIndex}.${key}`,
      })

      return options?.mentionAvailable ? (
        <ReadOnlyMentionText className="px-1 truncate">
          {value ?? undefined}
        </ReadOnlyMentionText>
      ) : (
        <div className={`px-1 py-px ${options?.wrap ? 'whitespace-pre-wrap' : 'truncate'}`}>
          {options?.format?.(value) ?? value}
        </div>
      )
    },
    getValueForEditor: ({ rowIndex }) => {
      const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
      const val = getValues(`${arrayName}.${fieldRowIndex}.${key}`)
      return options?.format?.(val) ?? val?.toString() ?? ''
    },
    setValueFromEditor: ({ rowIndex, value }) => {
      const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
      const val = options?.parse?.(value) ?? value
      setValue(
        `${arrayName}.${fieldRowIndex}.${key}`,
        val as ReactHookForm.PathValue<ReactHookForm.FieldValues, typeof key>,
        { shouldDirty: true }
      )
    },
    ...options,
  })
}

/**
 * 通常のテキスト列のセルエディタ
 */
export const TextCellEditor: EG2.EditableGridCellEditor = React.forwardRef(function DefaultEditor({ style, isEditing, requestCommit, requestCancel }, ref) {
  const [value, setValue] = React.useState<string>('')
  const refInput = React.useRef<HTMLInputElement>(null)

  const handleChange: React.ChangeEventHandler<HTMLInputElement> = e => {
    setValue(e.target.value)
  }

  const handleKeyDown: React.KeyboardEventHandler<HTMLInputElement> = e => {
    if (isEditing) {
      if (e.key === 'Enter') {
        requestCommit(value)
        e.preventDefault()
      }
      else if (e.key === 'Escape') {
        requestCancel()
        e.preventDefault()
      }
    }
  }

  React.useImperativeHandle(ref, () => ({
    blur: () => refInput.current?.blur(),
    getCurrentValue: () => refInput.current?.value ?? '',
    setValueAndSelectAll: (v, timing) => {
      setValue(v)
      if (timing === 'move-focus') {
        setTimeout(() => refInput.current?.select(), 0)
      }
    },
    getDomElement: () => refInput.current,
  }), [])

  return (
    <input
      ref={refInput}
      value={value ?? ''}
      onChange={handleChange}
      onKeyDown={handleKeyDown}
      className="px-[3px] resize-none field-sizing-content outline-none border border-black bg-white"
      style={style}
    />
  )
})

/**
 * メンション使用可能な列のセルエディタ
 */
export const CommentCellEditor: EG2.EditableGridCellEditor = React.forwardRef(({
  requestCancel,
  requestCommit,
  style,
}, ref) => {

  const textareaRef = React.useRef<HTMLTextAreaElement>(null);
  const [value, setValue] = React.useState('');

  React.useImperativeHandle(ref, () => ({
    getCurrentValue: () => {
      return textareaRef.current?.value ?? value
    },
    setValueAndSelectAll: (value, timing) => {
      setValue(value)
      if (timing === 'move-focus') {
        window.setTimeout(() => textareaRef.current?.select(), 0)
      }
    },
    getDomElement: () => textareaRef.current,
  }))

  const handleKeyDown: React.KeyboardEventHandler<HTMLTextAreaElement> = React.useCallback(e => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      requestCommit((e.target as HTMLTextAreaElement).value)
    } else if (e.key === 'Escape') {
      e.preventDefault()
      requestCancel()
    }
  }, [requestCancel, requestCommit])

  return (
    <SchemaDefinitionMentionTextarea
      ref={textareaRef}
      value={value ?? ''}
      onChange={newValue => setValue(newValue)}
      onKeyDown={handleKeyDown}
      style={style}
    />
  )
})
