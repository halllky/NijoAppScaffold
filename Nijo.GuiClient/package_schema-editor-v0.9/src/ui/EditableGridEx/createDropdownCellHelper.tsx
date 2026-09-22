import React from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@halllky/editable-grid"

/** ドロップダウン列の選択肢 */
export type DropdownCandidate = {
  value: string
  /** 画面上の表示名。未指定の場合は value がそのまま表示される */
  displayName?: string
}

export type CreateDropdownCellFunction = <TRow>(
  path: ReactHookForm.Path<TRow>,
  header: string,
  candidates: DropdownCandidate[],
  options?: Omit<Partial<EG2.EditableGridLeafColumn<TRow>>, 'columnId'>
) => EG2.EditableGridLeafColumn<TRow>

/**
 * EditableGrid のドロップダウン列。
 * 選択肢にない値がデータに入っている場合は、その値がそのまま表示される。
 */
export function createDropdownCellHelper(): CreateDropdownCellFunction {

  // グリッドはセルエディタのコンポーネントの参照が変わると編集中の状態を捨てるため、
  // 同じ選択肢に対しては必ず同じコンポーネントを返す必要がある
  const editors = new Map<string, EG2.EditableGridCellEditor>()
  const getEditor = (candidates: DropdownCandidate[]): EG2.EditableGridCellEditor => {
    const cacheKey = candidates.map(c => `${c.value}\t${c.displayName ?? ""}`).join("\n")
    let editor = editors.get(cacheKey)
    if (!editor) {
      editor = createDropdownCellEditor(candidates)
      editors.set(cacheKey, editor)
    }
    return editor
  }

  return (path, header, candidates, options) => {
    const getDisplayName = (value: unknown) => {
      const found = candidates.find(c => c.value === value)
      return found ? found.displayName ?? found.value : value as string
    }

    return {
      columnId: path,
      editor: getEditor(candidates),
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700">
          {header}
        </div>
      ),
      getValuesForRender: row => [ReactHookForm.get(row, path)],
      renderBody: ({ deps: [value] }) => (
        <div className="w-full px-1 text-sm truncate">
          {getDisplayName(value)}
        </div>
      ),
      cellToText: row => ReactHookForm.get(row, path)?.toString() ?? '',
      textToCell: (row, text) => {
        const clone = window.structuredClone(row)
        ReactHookForm.set(clone as ReactHookForm.FieldValues, path, text)
        return clone
      },
      onCellKeyDown: ({ event, requestEditStart }) => {
        // ドロップダウンはキーボードだけでも開けるようにする。
        // preventDefault しないと、グリッド標準の行操作（Alt + ↑↓ で行移動）も同時に走ってしまう
        const alt = event.altKey || event.metaKey
        const upDown = event.key === 'ArrowUp' || event.key === 'ArrowDown'
        if (event.key === 'Enter' || (alt && upDown)) {
          requestEditStart()
          event.preventDefault()
        }
      },
      ...options,
    }
  }
}

/**
 * ドロップダウン列のセルエディタ。
 * 選択肢によって内容が変わるため、選択肢ごとに作られる。
 */
function createDropdownCellEditor(candidates: DropdownCandidate[]): EG2.EditableGridCellEditor {
  return React.forwardRef(function DropdownEditor({ style, isEditing, requestCommit, requestCancel }, ref) {
    const [value, setValue] = React.useState<string>('')
    const refSelect = React.useRef<HTMLSelectElement>(null)

    // 選択肢を選んだ時点で確定する。ドロップダウンでは確定操作を別途要求する意味が薄いため
    const handleChange: React.ChangeEventHandler<HTMLSelectElement> = e => {
      setValue(e.target.value)
      requestCommit(e.target.value)
    }

    const handleKeyDown: React.KeyboardEventHandler<HTMLSelectElement> = e => {
      if (!isEditing) return
      if (e.key === 'Escape') {
        requestCancel()
        e.preventDefault()
      }
    }

    React.useImperativeHandle(ref, () => ({
      getCurrentValue: () => refSelect.current?.value ?? '',
      setValueAndSelectAll: (v, timing) => {
        setValue(v)
        // 編集開始と同時に選択肢を開く。値の反映より後に呼ぶ必要があるため描画を1回待つ
        if (timing === 'edit-start') setTimeout(() => refSelect.current?.showPicker?.(), 0)
      },
      getDomElement: () => refSelect.current,
    }), [])

    return (
      <select
        ref={refSelect}
        value={value}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        style={style}
        className="px-[2px] text-sm bg-white border border-black outline-none"
      >
        {/* 未指定に戻すための空の選択肢 */}
        <option value="">&nbsp;</option>

        {candidates.map(c => (
          <option key={c.value} value={c.value}>
            {c.displayName ?? c.value}
          </option>
        ))}

        {/* 選択肢にない値がデータに入っている場合、その値を選んだ状態として表示できるようにする */}
        {value !== '' && !candidates.some(c => c.value === value) && (
          <option value={value}>{value}</option>
        )}
      </select>
    )
  })
}
