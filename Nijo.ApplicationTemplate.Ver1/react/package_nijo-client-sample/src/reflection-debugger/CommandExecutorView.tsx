import React from "react"
import { Allotment, LayoutPriority } from "allotment"
import * as Icon from "@heroicons/react/24/solid"
import { DynamicForm, MemberOwner, DynamicFormRef } from "@nijo/ui-components/layout"
import * as Input from "@nijo/ui-components/input"
import { UUID } from "uuidjs"
import { useHttpRequest } from "../util"
import dayjs from "dayjs"

/**
 * Command Model のコマンド実行と Query Model の検索処理の両方で用いられる
 */
export default function ({
  title,
  localStorageUniqueKey,
  executionEndPoint,
  parameterFormSchema,
  returnValueFormSchema,
  className,
}: {
  title: string
  parameterFormSchema: MemberOwner
  returnValueFormSchema: MemberOwner
  localStorageUniqueKey: string
  executionEndPoint: string
  className?: string
}) {

  const { commandHistory, executeCommand, clearCommandHistory } = useCommandExecutor(localStorageUniqueKey, executionEndPoint)
  const parameterFormRef = React.useRef<DynamicFormRef>(null)

  // コマンド実行
  const [nowExecuting, setNowExecuting] = React.useState(false)
  const handleExecuteCommand = React.useCallback(async () => {
    if (nowExecuting) return
    setNowExecuting(true)
    try {
      const parameter = parameterFormRef.current?.useFormReturn.getValues()
      if (!parameter) return
      const newHistoryId = await executeCommand(parameter)
      if (!newHistoryId) return
      setSelectedHistoryId(newHistoryId)
    } finally {
      setNowExecuting(false)
    }
  }, [executeCommand, parameterFormRef])

  // 表示中の履歴のID
  const [selectedHistoryId, setSelectedHistoryId] = React.useState<string | null>(null)

  return (
    <Allotment
      vertical
      separator={false}
      proportionalLayout={false}
      className={className}
    >

      {/* パラメータ */}
      <Allotment.Pane
        priority={LayoutPriority.Low}
        className="h-full w-full flex flex-col gap-1"
      >
        <h2 className="font-bold select-none">
          {title}

          {/* デバッグ用 */}
          <button type="button"
            onClick={() => parameterFormRef.current?.consoleLog()}
            className="ml-2 border px-1 py-0.5"
          >
            console.log
          </button>
        </h2>

        <div className="flex-1 overflow-y-auto bg-gray-100 p-1 border border-gray-300">
          <DynamicForm
            ref={parameterFormRef}
            root={parameterFormSchema}
          />
        </div>
      </Allotment.Pane>

      {/* 実行結果 */}
      <Allotment.Pane className="flex flex-col gap-1 pt-4">
        <Allotment
          className="w-full h-full"
          separator={false}
          proportionalLayout={false}
        >
          {/* 実行履歴 */}
          <Allotment.Pane
            priority={LayoutPriority.Low}
            preferredSize={124}
            className="flex flex-col gap-1"
          >
            <div className="flex flex-wrap gap-1">
              <Input.IconButton fill icon={Icon.ArrowRightIcon} onClick={handleExecuteCommand} loading={nowExecuting}>
                実行
              </Input.IconButton>
              <Input.IconButton outline onClick={clearCommandHistory}>
                クリア
              </Input.IconButton>
            </div>
            <ul className="flex-1 overflow-y-auto">
              {commandHistory.map((history) => (
                <li
                  key={history.id}
                  className={`p-1 truncate select-none text-xs cursor-pointer hover:bg-gray-200 ${selectedHistoryId === history.id ? 'bg-gray-200' : ''}`}
                  onClick={() => setSelectedHistoryId(history.id)}
                >
                  {history.createdAt}
                </li>
              ))}
            </ul>
          </Allotment.Pane>

          {/* 実行結果（戻り値） */}
          <Allotment.Pane>
            <div className="w-full h-full bg-gray-100 border border-gray-300 pl-1 pb-1 pt-4 overflow-y-auto">
              {selectedHistoryId && (
                <DynamicForm
                  key={selectedHistoryId}
                  root={returnValueFormSchema}
                  defaultValues={commandHistory.find(history => history.id === selectedHistoryId)?.returnValue}
                  isReadOnly
                  className="w-full"
                />
              )}
            </div>
          </Allotment.Pane>

        </Allotment>
      </Allotment.Pane>
    </Allotment>
  )
}

// ---------------------------------

/**
 * コマンド実行と履歴の管理。
 * コマンド実行履歴はlocalStorage に保存される。
 */
const useCommandExecutor = (
  localStorageUniqueKey: string,
  executionEndPoint: string,
) => {
  const localStorageKey = `EXECUTE_HISTORY::${localStorageUniqueKey}`
  const [commandHistory, setCommandHistory] = React.useState<ExecutionHistory[]>([])

  // 履歴をlocalStorage から読み込む
  React.useEffect(() => {
    const history = localStorage.getItem(localStorageKey)
    if (history) {
      setCommandHistory(JSON.parse(history))
    }
  }, [])

  // コマンド実行
  const { complexPost } = useHttpRequest()
  const executeCommand = React.useCallback(async (parameter: object): Promise<string | undefined> => {
    // コマンド実行
    const returnValue = await complexPost<object>(executionEndPoint, parameter)
    if (!returnValue) {
      // エラーメッセージの表示は complexPost の中で行われるためここでは何もしない
      return undefined
    }

    // 結果をstateに追加
    const newHistoryId = UUID.generate()
    const newHistory: ExecutionHistory = {
      id: newHistoryId,
      parameter,
      returnValue,
      createdAt: dayjs().format('YYYY-MM-DD HH:mm:ss'),
    }
    setCommandHistory(prev => [newHistory, ...prev])

    // 履歴をlocalStorage に保存
    let currentHistory: ExecutionHistory[]
    try {
      currentHistory = JSON.parse(localStorage.getItem(localStorageKey) ?? '[]')
    } catch {
      currentHistory = []
    }
    localStorage.setItem(localStorageKey, JSON.stringify([newHistory, ...currentHistory]))

    return newHistoryId
  }, [complexPost, executionEndPoint, localStorageKey])

  // 履歴をクリア
  const clearCommandHistory = React.useCallback(() => {
    localStorage.removeItem(localStorageKey)
    setCommandHistory([])
  }, [localStorageKey])

  return {
    /** コマンド実行履歴 */
    commandHistory,
    /** コマンド実行 */
    executeCommand,
    /** コマンド実行履歴をクリア */
    clearCommandHistory,
  }
}

/** コマンド実行履歴 */
type ExecutionHistory = {
  id: string
  parameter: object
  returnValue: object
  createdAt: string
}
