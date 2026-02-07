import React from "react"
import { SchemaDefinitionMentionTextarea } from "../../../UI/Mention"
import * as EG2 from "@nijo/ui-components/layout/EditableGrid2"

/**
 * メンションを含むセル編集エディタ（スキーマ定義編集用）。
 * メンション使用可能。
 */
export const CommentCellEditor: EG2.EditableGridCellEditor = React.forwardRef(({
  isEditing,
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
