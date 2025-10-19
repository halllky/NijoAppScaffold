import React from "react"
import useEvent from "react-use-event-hook"
import * as UI from "@nijo/ui-components"
import { ModalDialog } from "@nijo/ui-components/layout"
import { ModelTypeSelectorForSchema } from "./Grid/Input.ModelTypeSelector"
import { TYPE_COMMAND_MODEL, TYPE_DATA_MODEL, TYPE_QUERY_MODEL, TYPE_STRUCTURE_MODEL } from "../types"

export type RootAggregateModelType =
  | typeof TYPE_DATA_MODEL
  | typeof TYPE_QUERY_MODEL
  | typeof TYPE_COMMAND_MODEL
  | typeof TYPE_STRUCTURE_MODEL

type CreateRootAggregateDialogProps = {
  onClose: () => void
  onCreate: (params: { localName: string; modelType: RootAggregateModelType }) => void
}

export const CreateRootAggregateDialog: React.FC<CreateRootAggregateDialogProps> = ({ onClose, onCreate }) => {
  const [modelType, setModelType] = React.useState<RootAggregateModelType>(TYPE_DATA_MODEL)
  const [localName, setLocalName] = React.useState("")
  const [errorMessage, setErrorMessage] = React.useState<string | undefined>(undefined)

  const handleClose = useEvent(() => {
    onClose()
  })

  const handleSubmit = useEvent((event?: React.FormEvent) => {
    event?.preventDefault()
    const trimmedName = localName.trim()
    if (!trimmedName) {
      setErrorMessage("名前を入力してください。")
      return
    }
    setErrorMessage(undefined)
    onCreate({ localName: trimmedName, modelType })
  })

  const handleLocalNameChange = useEvent((event: React.ChangeEvent<HTMLInputElement>) => {
    setLocalName(event.target.value)
    if (errorMessage) {
      setErrorMessage(undefined)
    }
  })

  return (
    <ModalDialog open onOutsideClick={handleClose}>
      <form onSubmit={handleSubmit} className="w-[420px] max-w-[95vw] bg-white rounded shadow-md overflow-hidden flex flex-col">
        <div className="flex items-center gap-2 px-4 py-2 border-b">
          <h1 className="text-base font-semibold flex-1">ルート集約を追加</h1>
          <UI.IconButton outline mini onClick={handleClose}>
            閉じる
          </UI.IconButton>
        </div>

        <div className="flex flex-col gap-3 px-4 py-4">
          <label className="flex flex-col gap-1 text-sm">
            <span className="font-medium">モデル種類</span>
            <ModelTypeSelectorForSchema
              value={modelType}
              onChange={value => setModelType(value as RootAggregateModelType)}
              className="w-full"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="font-medium">ルート集約名</span>
            <input
              type="text"
              value={localName}
              onChange={handleLocalNameChange}
              className="px-2 py-1 border border-gray-300 rounded"
              placeholder="例: Customer"
              autoFocus
            />
          </label>

          {errorMessage && (
            <div className="text-xs text-rose-600">
              {errorMessage}
            </div>
          )}
        </div>

        <div className="flex justify-end gap-2 px-4 py-3 border-t bg-gray-50">
          <button
            type="button"
            onClick={handleClose}
            className="px-3 py-1 text-sm border border-gray-300 rounded"
          >
            キャンセル
          </button>
          <button
            type="submit"
            className="px-3 py-1 text-sm rounded bg-sky-600 text-white disabled:opacity-60 disabled:cursor-not-allowed"
            disabled={!localName.trim()}
          >
            作成
          </button>
        </div>
      </form>
    </ModalDialog>
  )
}
