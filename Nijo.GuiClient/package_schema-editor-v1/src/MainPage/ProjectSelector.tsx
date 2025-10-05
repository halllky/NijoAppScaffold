import React from 'react'
import * as ReactRouter from 'react-router-dom'
import * as Input from '@nijo/ui-components/input'
import * as Icon from '@heroicons/react/24/solid'
import { NIJOUI_CLIENT_ROUTE_PARAMS } from '../routing'

/**
 * トップページ：プロジェクトフォルダを選択する画面
 */
export const ProjectSelector = () => {
  const navigate = ReactRouter.useNavigate()
  const [folderPath, setFolderPath] = React.useState('')
  const [recentProjects, setRecentProjects] = React.useState<string[]>([])
  const [error, setError] = React.useState<string>()

  // 最近開いたプロジェクトを読み込み
  React.useEffect(() => {
    setRecentProjects(getRecentProjects())
  }, [])

  const navigateToProject = React.useCallback((path: string) => {
    if (!path.trim()) {
      setError('フォルダパスを入力してください')
      return
    }

    setError(undefined)

    // localStorageに保存
    addRecentProject(path)

    // クエリパラメータとしてプロジェクトディレクトリを渡す
    const params = new URLSearchParams()
    params.set(NIJOUI_CLIENT_ROUTE_PARAMS.QUERY_PROJECT_DIR, path)
    navigate(`/project?${params.toString()}`)
  }, [navigate])

  const handleSubmit = React.useCallback((e: React.FormEvent) => {
    e.preventDefault()
    navigateToProject(folderPath)
  }, [folderPath, navigateToProject])

  const handleRecentProjectClick = React.useCallback((path: string) => {
    navigateToProject(path)
  }, [navigateToProject])

  const handleRemoveRecentProject = React.useCallback((e: React.MouseEvent, path: string) => {
    e.stopPropagation()
    try {
      const recent = getRecentProjects()
      const updated = recent.filter(p => p !== path)
      localStorage.setItem(RECENT_PROJECTS_KEY, JSON.stringify(updated))
      setRecentProjects(updated)
    } catch (error) {
      console.error('Failed to remove recent project:', error)
    }
  }, [])

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 p-8">
      <div className="max-w-2xl flex flex-col items-center gap-4 p-8 bg-white rounded-lg shadow-md">
        <h1 className="text-2xl font-bold">Nijo Application Builder</h1>

        <p className="text-gray-600">
          nijo.xmlファイルが含まれるプロジェクトフォルダのパスを入力してください
        </p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4 w-full">
          <div className="flex gap-2">
            <input
              type="text"
              value={folderPath}
              onChange={(e) => setFolderPath(e.target.value)}
              placeholder="例: C:\projects\myproject"
              className="flex-1 px-4 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <Input.IconButton
              fill
              submit
              className="px-6"
            >
              開く
            </Input.IconButton>
          </div>

          {error && (
            <div className="text-rose-500 text-sm">
              {error}
            </div>
          )}
        </form>

        {recentProjects.length > 0 && (
          <div className="w-full flex flex-col gap-2">
            <h2 className="text-sm font-semibold text-gray-700">最近開いたプロジェクト</h2>
            <div className="space-y-1">
              {recentProjects.map((path, index) => (
                <div
                  key={index}
                  className="flex items-start gap-2 rounded hover:bg-gray-100 cursor-pointer group"
                  onClick={() => handleRecentProjectClick(path)}
                >
                  <Icon.FolderIcon className="w-5 h-5 text-gray-500 flex-shrink-0" />
                  <span className="flex-1 text-sm text-gray-700 whitespace-pre-wrap" title={path}>
                    {path}
                  </span>
                  <button
                    onClick={(e) => handleRemoveRecentProject(e, path)}
                    className="p-1 cursor-pointer text-gray-500 bg-gray-200 rounded flex-shrink-0"
                    title="履歴から削除"
                  >
                    <Icon.XMarkIcon className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

// -----------------------------
// ローカルストレージ関連

const RECENT_PROJECTS_KEY = 'nijo_recent_projects'
const MAX_RECENT_PROJECTS = 50

/**
 * 最近開いたプロジェクトをlocalStorageから取得
 */
const getRecentProjects = (): string[] => {
  try {
    const stored = localStorage.getItem(RECENT_PROJECTS_KEY)
    return stored ? JSON.parse(stored) : []
  } catch {
    return []
  }
}

/**
 * 最近開いたプロジェクトをlocalStorageに保存
 */
const addRecentProject = (path: string) => {
  try {
    const recent = getRecentProjects()
    // 既存のものを削除して先頭に追加
    const filtered = recent.filter(p => p !== path)
    const updated = [path, ...filtered].slice(0, MAX_RECENT_PROJECTS)
    localStorage.setItem(RECENT_PROJECTS_KEY, JSON.stringify(updated))
  } catch (error) {
    console.error('Failed to save recent project:', error)
  }
}
