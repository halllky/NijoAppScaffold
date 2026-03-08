import React from "react"
import { FolderIcon } from "@heroicons/react/24/outline"
import { DocumentIcon } from "@heroicons/react/24/solid"
import AVAILABLE_SOURCE_CODES from "./available-files"
import { FileTreeItem, FileTreeLeaf, useFileTree } from "./useFileTree"

import "./SourceCodeViewer.css"

export type SourceCodePathAndComment<TProject extends keyof typeof AVAILABLE_SOURCE_CODES> = {
  path: (typeof AVAILABLE_SOURCE_CODES)[TProject][number]
  comment?: string
  focus?: number
}

/**
 * 自動生成後のソースコードをドキュメント内に表示するコンポーネント。
 * ソースコードは明示的に指定されたもののみが static フォルダに配置されており、
 * このコンポーネントは表示時にそこからフェッチしてくる。
 */
export function SourceCodeViewer<TProject extends keyof typeof AVAILABLE_SOURCE_CODES>(props: {
  project: TProject
  files: SourceCodePathAndComment<TProject>[]
  height?: string
}) {

  const handleFileClick = React.useCallback((item: FileTreeItem) => {
    if (item.kind === "file") {
      setSelectedFile(item)
    }
  }, [])

  // 引数チェック
  if (!AVAILABLE_SOURCE_CODES[props.project]) {
    throw new Error(`プロジェクト名が available-files.js に登録されていません: ${props.project}`)
  }
  for (const file of props.files) {
    if (!AVAILABLE_SOURCE_CODES[props.project].includes(file.path)) {
      throw new Error(`指定されたファイルが available-files.js に登録されていません: ${file.path}`)
    }
  }

  // propsで渡されたファイル名の一覧をパス区切りで分割してツリー構造にしたもの
  const filesTree = useFileTree(props.files)

  // 選択中のファイル
  const [selectedFile, setSelectedFile] = React.useState<FileTreeLeaf | null>(null)
  const [selectedFileContents, setSelectedFileContents] = React.useState(() => new Map<string, string>())
  const preRef = React.useRef<HTMLPreElement>(null)

  // ソースコードは画面表示時にまとめてフェッチしてキャッシュしておく
  React.useEffect(() => {
    Promise.all(props.files.map((file) => fetch(`/NijoAppScaffold/source-codes/${props.project}/${file.path}`).then(res => {
      if (res.ok) {
        return res.text()
      } else {
        throw new Error(`Failed to fetch source code: ${res.statusText}`)
      }
    }).then(body => {
      return [file.path, body] as const

    }).catch(err => {
      console.error(err)
      return [file.path, "ソースコードの取得に失敗しました。"] as const

    }))).then(results => {
      setSelectedFileContents(new Map(results))
    })
  }, [props.project, props.files])

  // フォーカス行へのスクロール処理
  React.useEffect(() => {
    if (!preRef.current) return;
    const focusLineNum = selectedFile && props.files.find(f => f.path === selectedFile.relativePath)?.focus
    if (focusLineNum && focusLineNum > 0) {
      // 行高を計算（テストのための一時的な測定）
      const lineHeight = preRef.current.style.lineHeight
        ? parseFloat(preRef.current.style.lineHeight)
        : parseFloat(window.getComputedStyle(preRef.current).lineHeight)

      // フォーカス行の上部がビューのトップに来るようにスクロール。5はパディング
      const scrollPosition = (focusLineNum - 1) * lineHeight + 5
      preRef.current.scrollTop = scrollPosition
    } else {
      preRef.current.scrollTop = 0
    }
  }, [selectedFile, props.files])

  return (
    <div className="source-code-viewer" style={{ height: props.height ?? "400px" }}>
      <div className={`file-explorer ${selectedFile ? "name-only" : "show-comment"}`}>
        {filesTree.map(file => (
          <div
            key={file.relativePath}
            className={`explorer-item ${file.kind === "file" ? "is-file" : "is-folder"} ${selectedFile?.relativePath === file.relativePath ? "is-selected" : ""}`}
            onClick={() => handleFileClick(file)}
          >
            <div className="file-name-and-indent">
              {Array.from({ length: file.indent }).map((_, ix) => (
                <span key={ix} className="indent" />
              ))}
              {file.kind === "folder" ? (
                <FolderIcon className="file-or-folder-icon" />
              ) : (
                <DocumentIcon className="file-or-folder-icon" />
              )}
              <span className="file-name">
                {file.displayName}
              </span>
            </div>

            {!selectedFile && (
              <span className="comment-to-file">
                {file.kind === "file" ? file.comment : ""}
              </span>
            )}
          </div>
        ))}

        {!selectedFile && (
          <div className="item-area-placeholder" />
        )}
      </div>

      {selectedFile && (
        <div className="source-code-preview">
          <span className="source-code-preview-header">
            <span className="source-code-preview-file-name" title={selectedFile.relativePath}>
              {selectedFile.relativePath}
            </span>
            <button
              type="button"
              className="source-code-preview-close"
              onClick={() => setSelectedFile(null)}
            >
              閉じる
            </button>
          </span>
          <pre className="source-code-preview-pre" ref={preRef}>
            {selectedFileContents.get(selectedFile.relativePath) ?? "読み込み中..."}
          </pre>
        </div>
      )}
    </div>
  )
}
