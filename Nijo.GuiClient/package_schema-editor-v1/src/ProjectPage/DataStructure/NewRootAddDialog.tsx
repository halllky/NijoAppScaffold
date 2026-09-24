import React, { useEffect, useState } from "react";
import { ModalDialog } from "@nijo/ui-components";
import { Button, MODEL_COLORS, MODEL_TYPE_OPTIONS, ModelTypeRadioButtonGroup, WordTextBox } from "../../UI";
import { MODEL_DATA } from "../../backend";

/**
 * 新しいルート集約追加ダイアログ
 */
export function NewRootAddDialog({ open, onClose, onRegister }: {
  open: boolean
  onClose: () => void
  onRegister: (name: string, modelType: string) => void
}) {

  const [step, setStep] = useState<1 | 2>(1)
  const [name, setName] = useState("")
  const [modelType, setModelType] = useState(MODEL_DATA)

  // ダイアログが開かれたときに状態をリセット
  useEffect(() => {
    if (open) {
      setStep(1)
      setName("")
      setModelType(MODEL_DATA)
    }
  }, [open])

  const handleRegister = () => {
    if (!name) return
    onRegister(name, modelType)
  }

  const handleSelectModelType = (selected: string) => {
    setModelType(selected)
    setStep(2)
  }

  return (
    <ModalDialog open={open} onOutsideClick={onClose} className="w-[500px] shadow-lg rounded flex flex-col bg-white">
      {/* ヘッダー */}
      <h2 className="text-lg font-bold text-gray-700 px-5 py-1 select-none">新規作成</h2>

      <div className="px-5 py-1">
        {step === 1 && (
          <div className="flex flex-col gap-4 animate-fadeIn">
            <div>
              <ModelTypeRadioButtonGroup onChange={handleSelectModelType} />
            </div>

            <div className="flex justify-end gap-2 mt-2">
              <Button onClick={onClose} outline>キャンセル</Button>
            </div>
          </div>
        )}

        {step === 2 && (
          <form
            className="flex flex-col gap-4 animate-fadeIn"
            onSubmit={(e) => {
              e.preventDefault()
              handleRegister()
            }}
          >
            {/* 選択されたモデル */}
            <div className="flex flex-col items-start gap-1 transition-colors text-left">
              <div className={`font-bold ${MODEL_COLORS[modelType].nameText}`}>
                {MODEL_TYPE_OPTIONS[modelType].displayName}
              </div>
              <div className={`text-xs mt-1 leading-snug ${MODEL_COLORS[modelType].descriptionText}`}>
                {MODEL_TYPE_OPTIONS[modelType].description}
              </div>
            </div>

            {/* 集約名 */}
            <div>
              <label className="text-sm font-bold text-gray-600 mb-1 block">名前</label>
              <WordTextBox
                value={name}
                onChange={(e) => setName(e.currentTarget.value)}
                autoFocus={true} /* Stepが切り替わったときにフォーカスが当たるように */
                className="w-full text-lg p-2"
              />
            </div>

            <div className="flex justify-end gap-2 mt-8">
              <Button onClick={() => setStep(1)} outline>戻る</Button>
              <Button submit fill disabled={!name}>作成</Button>
            </div>
          </form>
        )}
      </div>
    </ModalDialog>
  )
}
