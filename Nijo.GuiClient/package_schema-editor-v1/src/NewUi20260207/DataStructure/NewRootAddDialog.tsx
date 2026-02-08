import React, { useEffect, useState } from "react";
import { ModalDialog } from "@nijo/ui-components";
import { Button, ModelTypeSelector, WordTextBox } from "../../UI";
import { TYPE_DATA_MODEL } from "../../types";

/**
 * 新しいルート集約追加ダイアログ
 */
export function NewRootAddDialog({ open, onClose, onRegister }: {
  open: boolean
  onClose: () => void
  onRegister: (name: string, modelType: string) => void
}) {

  const [name, setName] = useState("")
  const [modelType, setModelType] = useState(TYPE_DATA_MODEL)

  // ダイアログが開かれたときに状態をリセット
  useEffect(() => {
    if (open) {
      setName("")
      setModelType(TYPE_DATA_MODEL)
    }
  }, [open])

  const handleRegister = () => {
    if (!name) return
    onRegister(name, modelType)
  }

  return (
    <ModalDialog open={open} onOutsideClick={onClose} className="p-4 w-[500px] shadow-lg rounded">
      <div className="flex flex-col gap-4">
        <h2 className="text-lg font-bold text-gray-700">新しいルート集約を作成</h2>
        <div className="flex flex-col gap-1">
          <label className="text-sm text-gray-600">名前</label>
          <WordTextBox value={name} onChange={(e) => setName(e.currentTarget.value)} autoFocus />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-sm text-gray-600">種別</label>
          <ModelTypeSelector value={modelType} onChange={setModelType} />
        </div>
        <div className="flex justify-end gap-2 mt-4">
          <Button onClick={onClose} outline>キャンセル</Button>
          <Button onClick={handleRegister} fill disabled={!name}>作成</Button>
        </div>
      </div>
    </ModalDialog>
  )
}
