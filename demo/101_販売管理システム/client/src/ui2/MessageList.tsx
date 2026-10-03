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
  // 実装時の注意: ui フォルダと、ui フォルダに依存するモジュールには依存せず、ui2 単独で実装すること。
  // ui フォルダは ui2 の動作が安定したら削除するので、依存していると削除時に巻き込まれるため。
  throw new Error('not implemented')
}
