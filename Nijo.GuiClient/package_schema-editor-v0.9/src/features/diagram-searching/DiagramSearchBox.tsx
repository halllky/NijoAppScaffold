import React from "react"
import * as ReactHookForm from "react-hook-form"
import { MagnifyingGlassIcon } from "@heroicons/react/24/outline"
import type { EditingProject } from "../backend"
import { isInvalidRegex, searchRootAggregates, type SearchCondition } from "./searchRootAggregates"

/**
 * ダイアグラム上のルート集約を検索するテキストボックス。
 * 部分一致検索と、「.*」ボタンをオンにした場合の正規表現検索ができる。
 * 検索対象はルート集約とその子孫の論理名、コメント、文字列型と数値型のオプショナル属性の値。
 * 検索対象のデータは親のフォームのコンテキストから取得する。
 *
 * 検索はテキストボックスで Enter キーを押したときと、正規表現モードを切り替えたときに行う。
 * 文字を入力しただけでは検索しない。ただし検索文字列を空にした場合は、その時点で検索していない状態に戻す。
 * 検索後に集約の内容が編集されても自動では検索し直さない。
 * 検索結果をどう表示に反映するかは呼び出し側で実装する。
 */
export function DiagramSearchBox({ onHitRootIdsChanged, className }: {
  /**
   * 検索を行ったときに、検索にヒットしたルート集約の uniqueId を伴って呼ばれる。
   * 検索文字列が空の場合や正規表現として不正な場合は、検索していない状態を表す undefined を伴って呼ばれる。
   */
  onHitRootIdsChanged: (hitRootIds: ReadonlySet<string> | undefined) => void
  /** 細かいレイアウトの微調整に使用 */
  className?: string
}) {

  const inputRef = React.useRef<HTMLInputElement>(null)

  const { getValues } = ReactHookForm.useFormContext<EditingProject>()

  // 入力中の検索条件
  const [condition, setCondition] = React.useState<SearchCondition>({ text: "", isRegex: false })
  const invalid = isInvalidRegex(condition)

  /** 指定の条件で検索する */
  const search = (target: SearchCondition) => {
    onHitRootIdsChanged(searchRootAggregates(getValues("rootAggregates"), getValues("optionalAttributes"), target))
  }

  /**
   * 検索文字列を変更する。
   * 空にした場合は、Enter キーを待たずに検索していない状態に戻す。
   * 入力欄のクリアボタンで消したときに、薄く表示されたままにならないようにするため
   */
  const handleTextChange = (text: string) => {
    const next = { ...condition, text }
    setCondition(next)
    if (text === "") search(next)
  }

  /** 正規表現モードを切り替え、切り替え後の条件で検索する */
  const handleToggleRegex = () => {
    const next = { ...condition, isRegex: !condition.isRegex }
    setCondition(next)
    search(next)
  }

  /** Enter キーで検索する。IME の変換確定の Enter では検索しない */
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" && !e.nativeEvent.isComposing) search(condition)
  }

  return (
    <div
      className={[
        "flex items-center gap-1 pl-1 pr-3 bg-white border resize-x overflow-hidden",
        invalid ? "border-rose-600" : "border-gray-800",
        className ?? "",
      ].join(" ")}
      title={invalid ? "正規表現が不正です" : "検索。ヒットしたもの以外を半透明にします"}
    >
      {/* 虫眼鏡アイコン */}
      <MagnifyingGlassIcon
        onClick={() => inputRef.current?.select()}
        className="h-4 w-4 shrink-0 text-gray-500 cursor-pointer"
      />

      {/* 検索文字列 */}
      <input
        ref={inputRef}
        type="search"
        value={condition.text}
        onChange={e => handleTextChange(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="検索"
        className="flex-1 min-w-0 py-px text-sm outline-none"
      />

      {/* 正規表現モードの切り替え */}
      <button
        type="button"
        onClick={handleToggleRegex}
        title="正規表現を使用する"
        aria-pressed={condition.isRegex}
        className={[
          "px-1 font-mono text-xs border rounded cursor-pointer select-none",
          condition.isRegex ? "text-white bg-sky-600 border-sky-700" : "text-gray-600 border-transparent hover:bg-gray-100",
        ].join(" ")}
      >
        .*
      </button>
    </div>
  )
}
