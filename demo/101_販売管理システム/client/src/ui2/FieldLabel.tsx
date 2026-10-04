import React from "react"
import * as RHF from "react-hook-form"
import { QuestionMarkCircleIcon } from "@heroicons/react/24/outline"
import { findMemberMetadata, type FormBinding } from "./FormBinding"
import { LABEL_PART_WIDTH_STYLE, useIsInFieldColumn } from "./FieldGroup"
import { MessageList } from "./MessageList"

export type FieldLabelProps<TValues extends RHF.FieldValues> = {
  /** ラベルを付ける項目。表示名・ヘルプテキスト・メッセージはこの項目のものが使われる */
  name: RHF.Path<TValues>
  /**
   * ラベルと children を縦に並べるなら true。未指定の場合は横並び。
   * 縦並びの場合はエラーメッセージがラベルの下に、横並びの場合は children の下に表示される。
   */
  vertical?: boolean
  /**
   * 必須マークを付けるかどうか。未指定の場合は付けない。
   * 基準はデータ構造上必須か否かではなく、ユーザーが入力する必要があるかどうか。
   * 例えばプログラム上で自動的に採番される項目は、データ構造上必須でもユーザーは入力する必要がない。
   */
  requiredMark?: boolean
  /** ラベル文字列の右側に追加で表示する内容。行追加ボタンなど */
  afterLabel?: React.ReactNode
  /**
   * ラベルを付ける対象。入力コンポーネントに限らず、グリッドなど何を置いてもよい。
   */
  children?: React.ReactNode
}

/**
 * 特定の項目のラベル。以下を表示する。
 *
 * - 表示名（メタデータの表示用名称）
 * - 必須マーク
 * - ヘルプテキスト（メタデータのコメント。アイコンにマウスを乗せると表示される）
 * - この項目とその子孫に対するメッセージ。ただし、より内側に表示領域（別の FieldLabel やグリッドのセル）がある項目のものは除く。
 *   クライアント側エラー（react-hook-form の検証エラー）とサーバー側メッセージの両方。複数ある場合はすべて表示する。
 *
 * ここに表示したメッセージは RootErrors には表示されない。
 *
 * 項目の配置（どの列に並べるか、横幅いっぱいに表示するか、ラベルの幅をそろえるか）はこのコンポーネントでは決めない。
 * FieldGroup と FieldColumn のどこに置くかで決まる。
 */
export function FieldLabel<TValues extends RHF.FieldValues>(props: FieldLabelProps<TValues> & {
  binding: FormBinding<TValues>
}): React.ReactNode {
  const { binding, name, vertical, requiredMark, afterLabel, children } = props
  const formPath = binding.toFormPath(name)
  const member = findMemberMetadata(binding.metadata, name)
  const isInFieldColumn = useIsInFieldColumn()

  // この項目とその子孫を、メッセージの表示領域として登録する
  React.useEffect(() => {
    return binding.registerMessageArea(formPath, { includesDescendants: true })
  }, [binding, formPath])
  const messages = React.useSyncExternalStore(binding.subscribeMessages, () => binding.getMessages(formPath))

  // FieldColumn の中では、FieldColumn のグリッドの1行になる。ラベル部分の幅を同じ列の中でそろえるため
  const layoutClassName = vertical
    ? (isInFieldColumn ? 'col-span-2 flex flex-col gap-px' : 'flex flex-col gap-px')
    : (isInFieldColumn ? 'col-span-2 grid grid-cols-subgrid items-start' : 'grid grid-cols-[max-content_minmax(0,1fr)] gap-x-2 items-start')

  // 横並びでは表示名が長くても入力項目を押しつぶさないよう、ラベル部分の幅を固定し、収まらない表示名は折り返す
  const labelPart = (
    <div className={`flex flex-wrap items-center gap-1 min-h-6 ${vertical ? '' : 'justify-end min-w-0'}`}>
      {/* ヘルプテキスト */}
      {member.comment && (
        <span title={member.comment} className="text-gray-400 cursor-help">
          <QuestionMarkCircleIcon className="w-4 h-4" />
        </span>
      )}
      {/* 表示名 */}
      <span className={`text-sm text-gray-700 select-none ${vertical ? '' : 'min-w-0 text-right break-words'}`}>
        {member.displayNameIsEmpty ? '' : member.displayName}
      </span>
      {/* 必須マーク */}
      {requiredMark && (
        <span className="text-rose-600 text-xs select-none" title="必須">*</span>
      )}
      {/* ラベルの右側に追加で表示する内容 */}
      {afterLabel}
    </div>
  )

  return vertical ? (
    // ラベル、メッセージ、入力項目が上から順に並ぶレイアウト
    <div className={layoutClassName}>
      {labelPart}
      <MessageList messages={messages} />
      {children}
    </div>
  ) : (
    // 左にラベル、右に入力項目とメッセージが並ぶレイアウト
    <div className={layoutClassName}>
      <div className="flex justify-end" style={LABEL_PART_WIDTH_STYLE}>
        {labelPart}
      </div>
      <div className="flex flex-col gap-px min-w-0">
        {children}
        {/* 横並びではメッセージは children の下 */}
        <MessageList messages={messages} />
      </div>
    </div>
  )
}
