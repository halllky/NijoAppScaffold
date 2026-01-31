import React from "react"

/**
 * 文字入力をトリガーとしてセル編集を開始するためのフック
 */
export function useOnKeyDownToStartEditing(isImeOpened: boolean): ((e: React.KeyboardEvent, onStartEditing: () => void) => void) {

  return React.useCallback((e, onStartEditing) => {

    // クイック編集（編集モードでない状態でいきなり文字入力して編集を開始する）
    const isQuickEditKey = isImeOpened
      || e.key === 'Process' // IMEが開いている場合のkeyはこれになる
      || !e.ctrlKey && !e.metaKey && e.key.length === 1 /*文字や数字や記号の場合*/

    if (e.key === 'F2' || isQuickEditKey) {
      onStartEditing()
      e.preventDefault()
    }

  }, [isImeOpened])

}
