import React from "react"
import * as Icon from "@heroicons/react/24/solid"
import * as UI from "../UI"
import { ServeTarget } from "../backend"
import { createProject } from "../backend/api"

/**
 * 編集対象のフォルダが nijo プロジェクトでないときの画面。
 * プロジェクト名とテンプレートを指定して、そのフォルダの直下にプロジェクトを新規作成する。
 * 誤った場所への作成を防ぐため、作成先のパスを明示する。
 */
export function NewProjectPage({ target }: {
  target: ServeTarget
}) {
  const [projectName, setProjectName] = React.useState('')
  const [templateName, setTemplateName] = React.useState<string>()
  const [nowCreating, setNowCreating] = React.useState(false)
  const [error, setError] = React.useState<string>()

  const trimmedProjectName = projectName.trim()
  const destination = `${target.projectRoot}${target.directorySeparator}${trimmedProjectName}`

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!trimmedProjectName || !templateName) return
    if (!window.confirm(`以下のフォルダにテンプレート「${templateName}」でプロジェクトを作成します。\n\n${destination}\n\nよろしいですか？`)) return

    setError(undefined)
    setNowCreating(true)

    const result = await createProject(trimmedProjectName, templateName)
    if (!result.ok) {
      setError(result.error)
      setNowCreating(false)
      return
    }

    // 再読み込みすると作成したプロジェクトが編集対象になっているので編集画面が開く
    window.location.reload()
  }

  return (
    <div className="flex items-center justify-center min-h-full bg-gray-50 p-8">
      <form onSubmit={handleSubmit} className="w-full max-w-2xl flex flex-col gap-4 p-8 bg-white rounded-lg shadow-md">

        <h1 className="text-2xl font-bold">プロジェクトの新規作成</h1>

        <p className="text-sm text-gray-600">
          以下のフォルダは nijo プロジェクトではありません。このフォルダの直下にプロジェクトのフォルダを作成できます。
        </p>

        <label className="flex flex-col gap-1">
          <span className="text-sm font-semibold text-gray-700">プロジェクト名</span>
          <input
            type="text"
            value={projectName}
            onChange={e => setProjectName(e.target.value)}
            placeholder="例: my-project"
            autoFocus
            className="px-2 py-1 border border-gray-500"
          />
          <span className="text-xs text-gray-600">作成するフォルダの名前になります。</span>
        </label>

        <div className="flex flex-col gap-1">
          <span className="text-sm font-semibold text-gray-700">作成先</span>
          <div className="flex items-start gap-2 p-2 bg-gray-100 border border-gray-300 rounded">
            <Icon.FolderIcon className="w-5 h-5 text-gray-500 shrink-0" />
            <code className="font-bold break-all">{destination}</code>
          </div>
        </div>

        <fieldset className="flex flex-col gap-2">
          <legend className="pb-2 text-sm font-semibold text-gray-700">テンプレート</legend>
          {target.templates.map(template => (
            <label key={template.name} className="flex items-start gap-2 cursor-pointer">
              <input
                type="radio"
                name="template"
                checked={templateName === template.name}
                onChange={() => setTemplateName(template.name)}
                className="mt-1"
              />
              <span className="flex flex-col">
                <span className="text-sm font-semibold">{template.name}</span>
                <span className="text-xs text-gray-600">{template.description}</span>
              </span>
            </label>
          ))}
        </fieldset>

        {error && (
          <div className="text-rose-500 text-sm whitespace-pre-wrap break-all">
            {error}
          </div>
        )}

        <div className="flex justify-end">
          <UI.Button
            submit
            fill
            icon={Icon.PlusIcon}
            disabled={!trimmedProjectName || !templateName}
            loading={nowCreating}
          >
            プロジェクトを作成
          </UI.Button>
        </div>
      </form>
    </div>
  )
}
