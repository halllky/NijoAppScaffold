import React from "react"
import * as ReactHookForm from "react-hook-form"
import { Button } from "../../ui"
import {
  NODE_TYPE_COMMAND_MODEL,
  NODE_TYPE_READ_MODEL,
  NODE_TYPE_WRITE_MODEL,
  NODE_TYPE_WRITE_READ_MODEL,
  type EditingProject,
} from "../../features/backend"
import { getModelKind } from "../../features/diagram"
import { MODEL_COLORS } from "./modelColors"

/**
 * 新しいルート集約を追加するダイアログ。
 * モデルの種類をその説明文を見ながら選び、名前を入力させる。
 *
 * 表示している間だけマウントされる前提で、マウントと同時にモーダル表示される。
 * 閉じる処理と、ルート集約の追加そのものは呼び出し側で実装する。
 */
export function NewRootAggregateDialog({ onCreate, onClose }: {
  /** 作成ボタンが押されたときに呼ばれる */
  onCreate: (displayName: string, type: string) => void
  /** キャンセルボタンまたは Esc キーで閉じられようとしたときに呼ばれる */
  onClose: () => void
}) {

  const [type, setType] = React.useState(MODEL_TYPES[0])
  const [displayName, setDisplayName] = React.useState("")

  // モデルの種類の説明文。サーバー側で定義されている説明をそのまま表示する
  const { control } = ReactHookForm.useFormContext<EditingProject>()
  const nodeTypes = ReactHookForm.useWatch({ control, name: "aggregateOrMemberTypes" })
  const options = React.useMemo(() => MODEL_TYPES.map(key => {
    const def = nodeTypes.find(t => t.key === key)
    return { key, displayName: def?.displayName ?? key, helpText: def?.helpText ?? "" }
  }), [nodeTypes])

  // マウントされたらモーダルとして表示する
  const dialogRef = React.useCallback((dialog: HTMLDialogElement | null) => {
    if (dialog && !dialog.open) dialog.showModal()
  }, [])

  const handleSubmit: React.SubmitEventHandler = e => {
    e.preventDefault()
    if (displayName.trim() === "") return
    onCreate(displayName.trim(), type)
  }

  const handleCancel: React.ReactEventHandler<HTMLDialogElement> = e => {
    // Esc キーで閉じる場合もダイアログを消すかどうかは呼び出し側に任せる
    e.preventDefault()
    onClose()
  }

  return (
    <dialog
      ref={dialogRef}
      onCancel={handleCancel}
      className="m-auto w-[560px] max-w-[calc(100vw-32px)] rounded shadow-lg backdrop:bg-black/30"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 p-4">
        <h2 className="text-lg font-bold text-gray-700">
          新しいルート集約を作成
        </h2>

        {/* モデルの種類 */}
        <div className="flex flex-col gap-2">
          {options.map(opt => (
            <label
              key={opt.key}
              className={`flex items-start gap-3 p-3 rounded border cursor-pointer ${type === opt.key
                ? "bg-sky-50 border-sky-400 ring-1 ring-sky-400"
                : "bg-white border-gray-200 hover:bg-gray-50"}`}
            >
              <input
                type="radio"
                name="new-root-aggregate-type"
                value={opt.key}
                checked={type === opt.key}
                onChange={() => setType(opt.key)}
                className="mt-1"
              />
              <div className="flex flex-col gap-1">
                <span className="font-bold" style={{ color: getModelColor(opt.key) }}>
                  {opt.displayName}
                </span>
                <span className="text-xs text-gray-600 whitespace-pre-wrap">
                  {opt.helpText}
                </span>
              </div>
            </label>
          ))}
        </div>

        {/* 名前 */}
        <label className="flex flex-col gap-1">
          <span className="text-sm font-bold text-gray-600">名前</span>
          <input
            value={displayName}
            onChange={e => setDisplayName(e.target.value)}
            spellCheck={false}
            autoComplete="off"
            className="px-2 py-1 border border-gray-300"
          />
        </label>

        {/* 操作 */}
        <div className="flex justify-end gap-2">
          <Button border onClick={onClose}>
            キャンセル
          </Button>
          <Button fill submit disabled={displayName.trim() === ""}>
            作成
          </Button>
        </div>
      </form>
    </dialog>
  )
}

/** ダイアグラムから追加できるルート集約の種類。ダイアグラムに表示されない種類は含まない */
const MODEL_TYPES = [
  NODE_TYPE_WRITE_MODEL,
  NODE_TYPE_WRITE_READ_MODEL,
  NODE_TYPE_READ_MODEL,
  NODE_TYPE_COMMAND_MODEL,
]

/** ルート集約の種類の、ダイアグラム上での色 */
function getModelColor(type: string): string | undefined {
  const model = getModelKind(type)
  return model && MODEL_COLORS[model].stroke
}
