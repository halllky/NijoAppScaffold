import React from "react"
import * as Icon from "@heroicons/react/24/solid"
import * as Layout from "@nijo/ui-components/layout"
import { COLUMN_ID_COMMENT } from "./index"
import { SchemaDefinitionMentionTextarea } from "../../UI/Mention"

/**
 * メンションを含むセル編集エディタ（スキーマ定義編集用）。
 * コメント列でのみメンション使用可能。
 * それ以外の列では通常のテキストエリアとして動作。
 */
export const CellEditorWithMention = React.forwardRef(({
  value,
  onChange,
  showOptions,
  columnDef,
}: Layout.CellEditorTextareaProps, ref: React.ForwardedRef<Layout.CellEditorTextareaRef>) => {

  const textareaRef = React.useRef<HTMLTextAreaElement>(null);

  React.useImperativeHandle(ref, () => ({
    focus: () => textareaRef.current?.focus(),
    select: () => textareaRef.current?.select(),
    value: value ?? '',
  }), [value, onChange])

  // コメント列かどうかを判断
  const isCommentColumn = columnDef?.columnId === COLUMN_ID_COMMENT

  return (
    <>
      {isCommentColumn ? (
        // コメント列の場合：メンション機能付きテキストエリア
        <SchemaDefinitionMentionTextarea
          ref={textareaRef}
          value={value ?? ''}
          onChange={onChange}
          className="flex-1 mx-[3px] my-[-1px]"
        />
      ) : (
        // それ以外の場合：通常のテキストエリア
        <textarea
          ref={textareaRef}
          value={value ?? ''}
          onChange={e => onChange(e.target.value)}
          className="flex-1 mx-[3px] my-[-1px] outline-none break-all resize-none field-sizing-content"
          spellCheck={false}
        />
      )}
      {showOptions && (
        <Icon.ChevronDownIcon className="w-4 cursor-pointer" />
      )}
    </>
  )
})
