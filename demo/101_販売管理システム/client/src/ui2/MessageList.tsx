/**
 * 1つの項目に対するメッセージ。
 * react-hook-form の検証エラーはここでは errors に含まれる。
 */
export type Messages = {
  errors: string[]
  warnings: string[]
  informations: string[]
}

export type MessageListProps = {
  messages: Messages
  className?: string
}

/**
 * メッセージの一覧。エラー・警告・情報の順に、すべてのメッセージを縦に並べて表示する。
 * メッセージが1件も無い場合は何も描画しない。
 * どの項目のメッセージを表示するかの判断はこのコンポーネントでは行わない。
 */
export function MessageList({ messages, className }: MessageListProps): React.ReactNode {
  throw new Error('not implemented')
}
