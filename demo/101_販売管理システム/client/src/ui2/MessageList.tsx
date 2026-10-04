/**
 * 1つの表示領域に表示するメッセージ。
 * クライアント側エラー（react-hook-form の検証エラー）とサーバー側メッセージを合わせたもの。
 * クライアント側エラーには警告・情報の区別が無いので、errors にだけ入る。
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
 *
 * 表示するだけのコンポーネントで、メッセージの出どころは区別しない。
 * クライアント側エラーとサーバー側メッセージは、呼び出し元が合わせたうえで渡すこと。
 */
export function MessageList({ messages, className }: MessageListProps): React.ReactNode {
  if (!hasMessages(messages)) return null

  return (
    <ul className={`flex flex-col text-xs ${className ?? ''}`}>
      {/* エラー */}
      {messages.errors.map((message, index) => (
        <li key={`error-${index}`} className="text-rose-700">{message}</li>
      ))}
      {/* 警告 */}
      {messages.warnings.map((message, index) => (
        <li key={`warning-${index}`} className="text-amber-700">{message}</li>
      ))}
      {/* 情報 */}
      {messages.informations.map((message, index) => (
        <li key={`information-${index}`} className="text-sky-700">{message}</li>
      ))}
    </ul>
  )
}

/** メッセージが1件以上あるかどうか */
export function hasMessages(messages: Messages): boolean {
  return messages.errors.length > 0
    || messages.warnings.length > 0
    || messages.informations.length > 0
}
