import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"

export type CreateTextCellFunction = <TRow>(
  path: ReactHookForm.Path<TRow>,
  header: string,
  options?: Omit<Partial<EG2.EditableGridLeafColumn<TRow>>, 'columnId'> & {
    format?: (value: unknown) => string
    parse?: (value: string) => unknown
    /** 折り返し表示するかどうか */
    wrap?: boolean
  }
) => EG2.EditableGridLeafColumn<TRow>

/**
 * EditableGrid のテキスト列
 */
export function createTextCellHelper(): CreateTextCellFunction {

  return (path, header, options) => {
    const { format, parse, wrap, ...restOptions } = options ?? {}

    return {
      columnId: path,
      editor: wrap ? MultiLineTextCellEditor : SingleLineTextCellEditor,
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700">
          {header}
        </div>
      ),
      getValuesForRender: row => [ReactHookForm.get(row, path)],
      renderBody: ({ deps: [value] }) => {
        return (
          <div className={`w-full px-1 text-sm ${wrap ? 'whitespace-pre-wrap' : 'truncate'}`}>
            {format?.(value as string) ?? value as string}
          </div>
        )
      },
      cellToText: row => {
        const val = ReactHookForm.get(row, path)
        return format?.(val) ?? val?.toString() ?? ''
      },
      textToCell: (row, text) => {
        const clone = window.structuredClone(row)
        const parsed = parse?.(text) ?? text
        ReactHookForm.set(clone as ReactHookForm.FieldValues, path, parsed)
        return clone
      },
      ...restOptions,
    }
  }
}

/**
 * 通常のテキスト列のセルエディタ
 *
 * - wrap = false: 単一行の input。Enter で確定。
 * - wrap = true:  複数行の textarea。Enter で確定、Alt + Enter で改行。
 */
function createTextCellEditor(wrap: boolean): EG2.EditableGridCellEditor {
  return React.forwardRef(function DefaultEditor({ style, isEditing, requestCommit, requestCancel }, ref) {
    const [value, setValue] = React.useState<string>('')
    const refInput = React.useRef<HTMLInputElement & HTMLTextAreaElement>(null)

    const handleChange: React.ChangeEventHandler<HTMLInputElement | HTMLTextAreaElement> = e => {
      setValue(e.target.value)
    }

    const handleKeyDown: React.KeyboardEventHandler<HTMLInputElement | HTMLTextAreaElement> = e => {
      if (!isEditing) return
      if (e.nativeEvent.isComposing) return // 日本語入力中は無視

      if (e.key === 'Enter') {
        // 折り返しありの場合、Alt + Enter は改行として扱う
        if (wrap && (e.altKey || e.metaKey)) return
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

    const commonProps = {
      value: value ?? '',
      onChange: handleChange,
      onKeyDown: handleKeyDown,
      spellCheck: false,
      autoComplete: 'off',
    } as const

    return wrap ? (
      <label
        // グリッドが渡す height はセルそのものの高さ。
        // そのまま適用すると複数行入力しても1行分の高さのままなので、minHeight に読み替えて下方向に伸びられるようにする。
        // 非編集時に読み替えないのは、エディタが編集中でなくてもDOM上に存在しフォーカス移動先セルの値を保持しており、
        // 伸ばすと不可視のエディタがグリッドのスクロール範囲を広げてしまうため。
        style={isEditing ? { ...style, height: undefined, minHeight: style.height } : style}
        className="px-[3px] text-sm border border-black bg-white"
      >
        <textarea
          ref={refInput}
          {...commonProps}
          rows={1}
          className="block mt-[-1px] w-full resize-none whitespace-pre-wrap field-sizing-content outline-none"
        />
      </label>
    ) : (
      <label
        style={style}
        className="px-[3px] text-sm resize-none border border-black bg-white"
      >
        <input
          ref={refInput}
          {...commonProps}
          className="block mt-[-1px] w-full field-sizing-content outline-none"
        />
      </label>
    )
  })
}

/** 通常のテキスト列のセルエディタ: 改行なし */
const SingleLineTextCellEditor = createTextCellEditor(false)
/** 通常のテキスト列のセルエディタ: 改行あり */
const MultiLineTextCellEditor = createTextCellEditor(true)
