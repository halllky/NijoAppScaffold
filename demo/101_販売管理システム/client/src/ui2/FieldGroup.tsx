export type FieldGroupProps = {
  /** 見出し。未指定の場合は見出しを表示しない */
  title?: React.ReactNode
  /** FieldColumn と、横幅いっぱいに表示する FieldLabel */
  children?: React.ReactNode
  /** 外側の余白の調整用。項目の配置はこのコンポーネントが決めるので、ここで grid などを指定しないこと */
  className?: string
}

/**
 * FieldLabel を並べる枠。
 *
 * - 項目は鏡面N字の順（左上→左下→右上→右下）に並ぶ。
 *   列の中で上から下へ並べ、列の末尾に達したら右隣の列の先頭へ進む。
 *   左から右へ並べて折り返す Z字の順は、目線が行ごとに左右へ往復して読みにくいため採らない。
 * - どこで次の列に移るかは自動では決めない。項目の意味の切れ目で区切れるよう、利用側が FieldColumn で列を分ける。
 * - FieldGroup の直下に置いた FieldLabel は、横幅いっぱいに表示する。FieldColumn の並びはそこで区切られ、その下から新しく並び始める。
 * - 列の幅はすべて等しい。横並びの FieldLabel のラベル部分の幅は、同じ列の中でそろえる。
 * - 画面の幅が足りず列を横に並べられない場合は、列を縦に積む。左の列が上になるので、鏡面N字と同じ読み順のまま1列になる。
 *
 * フォームとは結びつかないので、どのフォームの FieldLabel を並べてもよい。
 */
export function FieldGroup(props: FieldGroupProps): React.ReactNode {
  throw new Error('not implemented')
}

export type FieldColumnProps = {
  /** この列に上から順に並べる FieldLabel */
  children?: React.ReactNode
}

/**
 * FieldGroup の中の1列。中の FieldLabel を上から下へ並べる。
 * FieldGroup の直下に置くこと。それ以外の場所に置いた場合の配置は保証しない。
 */
export function FieldColumn(props: FieldColumnProps): React.ReactNode {
  throw new Error('not implemented')
}
