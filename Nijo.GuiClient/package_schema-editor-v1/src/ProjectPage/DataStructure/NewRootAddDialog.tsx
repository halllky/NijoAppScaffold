import React, { useEffect, useState } from "react";
import { ModalDialog } from "@nijo/ui-components";
import { Button, DataStructureFlags, DataStructureFlagsCheckBoxes, ROOT_KIND_DATA_STRUCTURE, ROOT_KIND_OPTIONS, RootKind, RootKindRadioButtonGroup, WordTextBox } from "../../UI";

/**
 * 新しいルート集約追加ダイアログ
 */
export function NewRootAddDialog({ open, onClose, onRegister }: {
  open: boolean
  onClose: () => void
  /** flags は rootKind がデータ構造の場合のみ意味を持つ */
  onRegister: (name: string, rootKind: RootKind, flags: DataStructureFlags) => void
}) {

  const [step, setStep] = useState<1 | 2>(1)
  const [name, setName] = useState("")
  const [rootKind, setRootKind] = useState<RootKind>(ROOT_KIND_DATA_STRUCTURE)
  const [flags, setFlags] = useState<DataStructureFlags>(DEFAULT_FLAGS)

  // ダイアログが開かれたときに状態をリセット
  useEffect(() => {
    if (open) {
      setStep(1)
      setName("")
      setRootKind(ROOT_KIND_DATA_STRUCTURE)
      setFlags(DEFAULT_FLAGS)
    }
  }, [open])

  const handleRegister = () => {
    if (!name) return
    onRegister(name, rootKind, flags)
  }

  const handleSelectRootKind = (selected: RootKind) => {
    setRootKind(selected)
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
              <RootKindRadioButtonGroup onChange={handleSelectRootKind} />
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
            {/* 選択された種類 */}
            <div className="flex flex-col items-start gap-1 transition-colors text-left">
              <div className={`font-bold ${ROOT_KIND_OPTIONS[rootKind].colors.nameText}`}>
                {ROOT_KIND_OPTIONS[rootKind].displayName}
              </div>
              <div className={`text-xs mt-1 leading-snug ${ROOT_KIND_OPTIONS[rootKind].colors.descriptionText}`}>
                {ROOT_KIND_OPTIONS[rootKind].description}
              </div>
            </div>

            {/* データ構造の IsStored, IsQueryable 属性 */}
            {rootKind === ROOT_KIND_DATA_STRUCTURE && (
              <DataStructureFlagsCheckBoxes value={flags} onChange={setFlags} showDescription />
            )}

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

/** 新規作成時の IsStored, IsQueryable 属性の初期値 */
const DEFAULT_FLAGS: DataStructureFlags = { isStored: true, isQueryable: false }
