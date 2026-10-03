export type FieldGroupProps = {
  /** 見出し。未指定の場合は見出しを表示しない */
  title?: React.ReactNode
  /** 1行に並べる項目の数の上限。未指定の場合は画面の幅に収まるだけ並べる */
  maxColumns?: number
  /** 並べる FieldLabel */
  children?: React.ReactNode
  /** 外側の余白の調整用。項目の配置はこのコンポーネントが決めるので、ここで grid などを指定しないこと */
  className?: string
}

/**
 * FieldLabel を並べる枠。
 *
 * - 画面の幅に応じて、1行に並べる項目の数を変える。
 * - 横並びの FieldLabel のラベル部分の幅を、同じ列の中でそろえる。
 * - 横幅いっぱいに表示する項目は、FieldLabel 側の wide で指定する。
 */
export function FieldGroup(props: FieldGroupProps): React.ReactNode {
  throw new Error('not implemented')
}
