import React, { useState, useRef, useEffect, useLayoutEffect, useImperativeHandle } from "react"
import * as ReactHookForm from "react-hook-form"
import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"
import { useSchemaCandidates } from "../MainPage/SchemaCandidatesContext"

export type CreateComboBoxCellFunction = <TRow>(
  header: string,
  key: ReactHookForm.Path<TRow>,
  options?: Partial<EG2.EditableGrid2LeafColumn<TRow>>
) => EG2.EditableGrid2LeafColumn<TRow>

/**
 * ノード種別のコンボボックス列（テキスト入力可 + ドロップダウン選択）。
 * 一応、選択肢に存在しない値も入力可能。
 * ドロップダウンの候補は React Context 経由で取得する。
 */
export function createComboBoxCellHelper(
  getValues: ReactHookForm.UseFormGetValues<ReactHookForm.FieldValues>,
  setValue: ReactHookForm.UseFormSetValue<ReactHookForm.FieldValues>,
  control: ReactHookForm.Control<ReactHookForm.FieldValues>,
  arrayName: string,
  skipFirstRow: boolean | undefined,
): CreateComboBoxCellFunction {

  return (header, key, options) => {
    return {
      renderHeader: () => (
        <div className="px-1 py-px truncate text-sm text-gray-700">
          {header}
        </div>
      ),
      renderBody: ({ context }) => {
        const fieldRowIndex = skipFirstRow ? context.row.index + 1 : context.row.index
        const value = ReactHookForm.useWatch({ control, name: `${arrayName}.${fieldRowIndex}.${key}` })

        // 論理名
        const { items } = useSchemaCandidates()
        const displayText = items.find(item => item.value === value)?.text

        // ComboBoxなので、値そのものを表示する
        return (
          <div className="self-start flex items-center gap-2 px-1 truncate">
            <span className="truncate">
              {value as string}
            </span>
            {displayText && displayText !== value && (
              <span className="flex-1 text-gray-400 text-sm truncate">
                {displayText}
              </span>
            )}
          </div>
        )
      },
      editor: TypeComboEditor,
      getValueForEditor: ({ rowIndex }) => {
        const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
        const val: string | null | undefined = getValues(`${arrayName}.${fieldRowIndex}.${key}`)
        return val ?? ''
      },
      setValueFromEditor: ({ rowIndex, value }) => {
        const fieldRowIndex = skipFirstRow ? rowIndex + 1 : rowIndex
        setValue(
          `${arrayName}.${fieldRowIndex}.${key}`,
          value as ReactHookForm.PathValue<ReactHookForm.FieldValues, typeof key>,
          { shouldDirty: true }
        )
      },
      onCellKeyDown: ({ event, requestEditStart }) => {
        const alt = event.altKey || event.metaKey
        const upDown = event.key === 'ArrowUp' || event.key === 'ArrowDown'
        // 文字入力でも編集開始したいので、シングルキャラクターなどのチェックも入れたほうが親切だが、
        // 最低限 Dropdown と合わせるなら Enter or Alt+Arrow で開始。
        // ただし ComboBox なので任意のキー入力で編集開始してほしい場合が多い。
        // ここでは Dropdown に合わせつつ、任意の文字キー入力は EditableGrid2 のデフォルト挙動に任せる。
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
 * ComboBox列のセルエディタ
 */
const TypeComboEditor: EG2.EditableGridCellEditor = React.forwardRef((props, ref) => {
  const inputRef = useRef<HTMLInputElement>(null)
  const listRef = useRef<HTMLUListElement>(null)
  const wrapperRef = useRef<HTMLDivElement>(null)

  // テキストボックスの値
  const [value, setVal] = useState('')

  // 選択肢
  const { items, isLoading } = useSchemaCandidates()
  const filteredItems = React.useMemo(() => {
    if (!value) return items
    const lowerValue = value.toLowerCase()
    return items.filter(item => item.value.toLowerCase().includes(lowerValue))
  }, [items, value])

  // ドロップダウンの状態
  const [selectedIndex, setSelectedIndex] = useState(-1)
  const [dropdownPosition, setDropdownPosition] = useState<'bottom' | 'top'>('bottom')

  // 外部からの制御
  useImperativeHandle(ref, () => ({
    getCurrentValue: () => inputRef.current?.value ?? '',
    setValueAndSelectAll: (v, timing) => {
      setVal(v)
      if (timing === 'move-focus' || timing === 'edit-end') {
        setTimeout(() => inputRef.current?.select(), 0)
      }
    },
    getDomElement: () => inputRef.current,
  }))

  // 選択肢に自動フォーカス
  React.useEffect(() => {
    if (!props.isEditing) return
    if (filteredItems.length === 0) return
    if (selectedIndex === -1 || selectedIndex >= filteredItems.length) {
      setSelectedIndex(0)
    }
  }, [props.isEditing, filteredItems])

  // ドロップダウン位置計算
  useLayoutEffect(() => {
    if (props.isEditing && wrapperRef.current) {
      // Gridのエディタは position: fixed ではなく absolute で配置されていることが多いが、
      // 画面端の判定には getBoundingClientRect を使う
      const rect = wrapperRef.current.getBoundingClientRect()
      const windowHeight = window.innerHeight
      const spaceBelow = windowHeight - rect.bottom
      const spaceAbove = rect.top

      // 下が狭くて上が広いなら上に出す (AsyncComboBoxのロジック参考)
      if (spaceBelow < 250 && spaceAbove > spaceBelow) {
        setDropdownPosition('top')
      } else {
        setDropdownPosition('bottom')
      }
    }
  }, [props.isEditing, filteredItems.length])

  // キーボード操作でリストスクロール
  useEffect(() => {
    if (props.isEditing && listRef.current && selectedIndex >= 0) {
      const selectedElement = listRef.current.children[selectedIndex] as HTMLElement
      if (selectedElement) {
        selectedElement.scrollIntoView({ block: 'nearest' })
      }
    }
  }, [selectedIndex, props.isEditing])

  const handleKeyDown: React.KeyboardEventHandler<HTMLInputElement> = (e) => {
    if (!props.isEditing) return
    if (e.nativeEvent.isComposing) return // 日本語入力中は無視

    // 候補選択操作
    if (filteredItems.length > 0) {
      if (e.key === 'ArrowDown') {
        e.preventDefault()
        setSelectedIndex(prev => (prev < filteredItems.length - 1 ? prev + 1 : 0))
        return
      }
      if (e.key === 'ArrowUp') {
        e.preventDefault()
        setSelectedIndex(prev => (prev > 0 ? prev - 1 : filteredItems.length - 1))
        return
      }
    }

    // 編集確定
    if (e.key === 'Enter') {
      e.preventDefault()
      props.requestCommit(selectedIndex >= 0 && selectedIndex < filteredItems.length
        ? filteredItems[selectedIndex].value
        : (e.target as HTMLInputElement).value)
      return
    }

    // 編集キャンセル
    if (e.key === 'Escape') {
      // 編集自体をキャンセル
      props.requestCancel()
      e.preventDefault()
    }
  }

  return (
    <div style={props.style} ref={wrapperRef} className="relative">
      <input
        ref={inputRef}
        value={value}
        onChange={ev => setVal(ev.target.value)}
        onKeyDown={handleKeyDown}
        className="w-full h-full border border-gray-700 outline-none bg-white px-1"
      />

      {props.isEditing && (
        <ul
          ref={listRef}
          className={`absolute z-50 w-full min-w-[320px] bg-white border border-gray-700 max-h-60 overflow-auto shadow-lg list-none p-0 m-0 text-left ${dropdownPosition === 'top' ? 'bottom-full mb-1' : 'mt-1'
            }`}
        >
          {isLoading && filteredItems.length === 0 && (
            <li className="p-2 text-gray-500 text-sm">検索中...</li>
          )}

          {filteredItems.length === 0 && !isLoading && (
            <li className="p-2 text-gray-500 text-sm">該当なし</li>
          )}

          {filteredItems.map((item, index) => (
            <li
              key={item.value}
              className={`px-1 py-px cursor-pointer border-b border-gray-100 last:border-none ${index === selectedIndex ? 'bg-blue-100' : 'hover:bg-blue-50'}`}
              onClick={() => props.requestCommit(item.value)}
              onMouseDown={e => e.stopPropagation()} // EditableGrid2 が外側クリックで編集終了扱いにしないようにする
              onMouseMove={() => setSelectedIndex(index)}
            >
              {item.value}
              {item.text !== item.value && (
                <span className="text-gray-400 text-sm ml-2">
                  {item.text}
                </span>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
})

