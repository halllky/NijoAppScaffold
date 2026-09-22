import React from "react"
import * as ReactHookForm from "react-hook-form"
import type { EditingProject } from "../backend"
import { buildAggregateTree, type DiagramStructure } from "./aggregateTree"

/**
 * ダイアグラムの構成を保持し、構成に影響する編集があったことを編集した側から明示的に通知してもらうためのコンテキスト。
 *
 * フォームの値を監視して構成を組み立てると、名前以外の属性やコメントなど、
 * ダイアグラムに関係の無い項目の編集のたびにダイアグラム全体が再描画されてしまう。
 * そのため構成はここで保持し、通知があったときにだけフォームの最新の値から組み立て直す。
 */
export function DiagramStructureProvider({ children }: {
  children?: React.ReactNode
}) {

  const { getValues } = ReactHookForm.useFormContext<EditingProject>()

  // ダイアグラムの構成
  const [structure, setStructure] = React.useState(() => buildAggregateTree(getValues("rootAggregates")))

  /** フォームの最新の値からダイアグラムの構成を組み立て直す */
  const notifyStructureChanged = React.useCallback(() => {
    setStructure(buildAggregateTree(getValues("rootAggregates")))
  }, [getValues])

  // 通知する側のコンポーネントが構成の変化のたびに再描画されないよう、構成と通知関数を別のコンテキストで配る
  return (
    <NotifyContext.Provider value={notifyStructureChanged}>
      <StructureContext.Provider value={structure}>
        {children}
      </StructureContext.Provider>
    </NotifyContext.Provider>
  )
}

/**
 * ダイアグラムの構成を取得する。
 * 構成に影響する編集が通知されたときにだけ新しい値に変わる。
 */
export function useDiagramStructure(): DiagramStructure {
  const structure = React.useContext(StructureContext)
  if (structure === null) throw new Error("DiagramStructureProvider の外では使用できません。")
  return structure
}

/**
 * ダイアグラムの構成に影響する編集を行ったことを通知する関数を取得する。
 * フォームの値を書き換えた後に呼ぶこと。
 * 1回の操作で複数の値を書き換える場合は、すべて書き換えた後に1回呼べばよい。
 */
export function useNotifyDiagramStructureChanged(): () => void {
  const notify = React.useContext(NotifyContext)
  if (notify === null) throw new Error("DiagramStructureProvider の外では使用できません。")
  return notify
}

const StructureContext = React.createContext<DiagramStructure | null>(null)
const NotifyContext = React.createContext<(() => void) | null>(null)
