import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"
import { MentionableTextarea, MentionableTextareaReadOnly } from "./Mention"
import { useFieldValidationError } from "../ProjectPage/useValidation"
import { EditingProject } from "../backend"
import { useMentionSuggestions } from "./useMentionSuggestions"
import { JumpToElementContext } from "../ProjectPage/useJumpToElement"
import { setFieldByPath } from "./setFieldByPath"

export type CreateTextCellFunction = <TRow>(
  header: string,
  key: ReactHookForm.Path<TRow>,
  options?: Omit<Partial<EG2.EditableGridLeafColumn<TRow>>, 'wrap'> & {
    format?: (value: unknown) => string
    parse?: (value: string) => unknown
    /** メンション使用可能かどうか */
    mentionAvailable?: boolean
    /** 折り返し表示するかどうか */
    wrap?: boolean
    /** バリデーションエラーがあるときにセル背景色を変えるための設定 */
    validationErrorSettings?: [
      getXmlElementUniqueId: (row: TRow) => string | null | undefined,
      attributeName?: string
    ]
  }
) => EG2.EditableGridLeafColumn<TRow>

/**
 * EditableGrid のテキスト列
 */
export function createTextCellHelper(
  getValues: ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>,
  setValue: ReactHookForm.UseFormSetValue<ReactHookForm.FieldValues>,
  control: ReactHookForm.Control<ReactHookForm.FieldValues>,
  arrayName: string,
  skipFirstRow: boolean | undefined,
): CreateTextCellFunction {

  return (header, key, options) => {
    const { format, parse, mentionAvailable, wrap, validationErrorSettings, ...restOptions } = options ?? {}

    return {
      columnId: key as string,
      editor: mentionAvailable
        ? MentionableCellEditor
        : TextCellEditor,
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700">
          {header}
        </div>
      ),
      renderBody: ({ rowIndex, getRow }) => {
        const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
        const value: string | null | undefined = ReactHookForm.useWatch({ name: `${arrayName}.${fieldRowIndex}.${key}`, control })
        const { hasError, errorMessages } = useFieldValidationError(validationErrorSettings?.[0]?.(getRow()), validationErrorSettings?.[1])
        const jumpToElement = React.useContext(JumpToElementContext)

        return mentionAvailable ? (
          <MentionableTextareaReadOnly
            title={errorMessages.join('\n')}
            onClickMention={part => jumpToElement?.(part.targetId)}
            className={`w-full px-1 text-sm ${wrap ? 'whitespace-pre-wrap' : 'truncate'} ${hasError ? 'bg-amber-300/50' : ''}`}
          >
            {value ?? undefined}
          </MentionableTextareaReadOnly>
        ) : (
          <div
            title={errorMessages.join('\n')}
            className={`w-full px-1 text-sm ${wrap ? 'whitespace-pre-wrap' : 'truncate'} ${hasError ? 'bg-amber-300/50' : ''}`}
          >
            {format?.(value) ?? value}
          </div>
        )
      },
      cellToText: row => {
        const val = ReactHookForm.get(row, key)
        return format?.(val) ?? val?.toString() ?? ''
      },
      textToCell: (row, text) => {
        const val = parse?.(text) ?? text
        return setFieldByPath(row, key as string, val)
      },
      ...restOptions,
    }
  }
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
    if (!isEditing) return
    if (e.nativeEvent.isComposing) return // 日本語入力中は無視

    if (e.key === 'Enter') {
      requestCommit(value)
      e.preventDefault()
    }
    else if (e.key === 'Escape') {
      requestCancel()
      e.preventDefault()
    }
  }

  React.useImperativeHandle(ref, () => ({
    getCurrentValue: () => refInput.current?.value ?? '',
    setValueAndSelectAll: (v, timing) => {
      setValue(v)
      if (timing === 'move-focus' || timing === 'edit-end') {
        setTimeout(() => refInput.current?.select(), 0)
      }
    },
    getDomElement: () => refInput.current,
  }), [])

  return (
    <label
      style={style}
      className="px-[3px] text-sm resize-none border border-black bg-white"
    >
      <input
        ref={refInput}
        value={value ?? ''}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        spellCheck={false}
        autoComplete="off"
        className="block mt-[-1px] text-sm w-full field-sizing-content outline-none"
      />
    </label>
  )
})

/**
 * メンション使用可能な列のセルエディタ
 */
export const MentionableCellEditor: EG2.EditableGridCellEditor = React.forwardRef(({
  requestCancel,
  requestCommit,
  style,
  isEditing,
}, ref) => {

  const { getValues } = ReactHookForm.useFormContext<EditingProject>()
  const getMentionSuggestions = useMentionSuggestions(getValues)

  const textareaRef = React.useRef<HTMLTextAreaElement>(null);
  const [value, setValue] = React.useState('');

  React.useImperativeHandle(ref, () => ({
    getCurrentValue: () => {
      return value
    },
    setValueAndSelectAll: (value, timing) => {
      setValue(value)
      if (timing === 'move-focus') {
        window.setTimeout(() => textareaRef.current?.select(), 0)
      }
    },
    getDomElement: () => textareaRef.current,
  }))

  const handleKeyDown: React.KeyboardEventHandler<HTMLTextAreaElement> = e => {
    if (e.key === 'Enter' && !e.shiftKey && !e.nativeEvent.isComposing) {
      e.preventDefault()
      requestCommit(value)
    } else if (e.key === 'Escape') {
      e.preventDefault()
      requestCancel()
    }
  }

  return (
    <MentionableTextarea
      getSuggestions={getMentionSuggestions}
      ref={textareaRef}
      value={value ?? ''}
      onChange={setValue}
      onKeyDown={handleKeyDown}
      // グリッドが渡す height はセルそのものの高さ。
      // そのまま適用すると複数行入力しても1行分の高さのままなので、minHeight に読み替えて下方向に伸びられるようにする。
      // 非編集時に読み替えないのは、エディタが編集中でなくてもDOM上に存在しフォーカス移動先セルの値を保持しており、
      // 伸ばすと不可視のエディタがグリッドのスクロール範囲を広げてしまうため。
      style={isEditing ? { ...style, height: undefined, minHeight: style.height } : style}
      className="text-sm bg-white border border-gray-700 [&_textarea]:px-[3px] mt-[-1px]"
    />
  )
})
