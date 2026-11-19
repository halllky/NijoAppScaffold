import React from "react"
import { Button } from "../input/Button"
import { uploadAttachmentAsync, type AttachmentFile } from "../../http/uploadAttachmentAsync"
import type { OperationMessages } from "../../http/callOperationAsync"

export type AttachmentUploadButtonProps = {
  /** ファイルを1つ一時保存するたびに呼ばれる。受け取った情報を入力中のデータに入れる */
  onUploaded: (file: AttachmentFile) => void
  /** 一時保存できなかったファイルがあったときに呼ばれる。メッセージの先頭にはファイル名が付く */
  onError: (messages: OperationMessages) => void
  /**
   * 一時保存を始めたときと終えたときに呼ばれる。
   * 送っている間に保存されると、そのファイルが添付されないまま保存されるので、保存のボタンを押せないようにするのに使う。
   */
  onUploadingChange?: (uploading: boolean) => void
  /** 複数のファイルを一度に選べるようにするなら true */
  multiple?: boolean
  /** ボタンの文言 */
  children?: React.ReactNode
}

/**
 * ファイルを選ぶボタン。選んだ時点で、ファイルを1つずつ一時保存の API に送る。
 */
export function AttachmentUploadButton({ onUploaded, onError, onUploadingChange, multiple, children }: AttachmentUploadButtonProps) {
  const inputRef = React.useRef<HTMLInputElement>(null)
  const [uploading, setUploading] = React.useState(false)

  const handleChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? [])
    // 同じファイルを続けて選んでも change が起きるよう、選ばれたファイルを空にしておく
    e.target.value = ''
    if (files.length === 0) return

    setUploading(true)
    onUploadingChange?.(true)
    try {
      const errors: string[] = []
      for (const file of files) {
        const result = await uploadAttachmentAsync(file)
        if (result.type === 'ok') {
          onUploaded(result.file)
        } else if (result.type === 'error') {
          errors.push(...collectErrors(result.messages).map(message => `${file.name}: ${message}`))
        }
      }
      if (errors.length > 0) onError({ error: errors })
    } finally {
      setUploading(false)
      onUploadingChange?.(false)
    }
  }

  return (
    <>
      <Button mini outline loading={uploading} onClick={() => inputRef.current?.click()}>
        {children ?? 'ファイルを添付'}
      </Button>
      <input
        ref={inputRef}
        type="file"
        multiple={multiple}
        onChange={handleChange}
        className="hidden"
      />
    </>
  )
}

/** 一時保存の API が返したメッセージは項目に結びつかないので、子のメッセージも含めて平らにする */
function collectErrors(messages: OperationMessages): string[] {
  return [
    ...(messages.error ?? []),
    ...Object.values(messages.children ?? {}).flatMap(collectErrors),
  ]
}
