import React from "react"

export type FieldGroupProps = {
  /** 見出し。未指定の場合は見出しを表示しない */
  title?: React.ReactNode
  /** FieldColumn と、横幅いっぱいに表示する FieldLabel */
  children?: React.ReactNode
  /**
   * 横並びの FieldLabel のラベル部分の幅。CSS の長さで指定する（例: `'8rem'`）。
   * この中のすべての列と、横幅いっぱいに表示する FieldLabel に適用される。FieldColumn で指定した場合はその列だけそちらが優先される。
   * 未指定の場合は既定の幅になる。
   */
  labelWidth?: string
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
 * - 列の幅はすべて等しい。
 * - 横並びの FieldLabel のラベル部分の幅は FieldLabel ごとには決めず、FieldGroup か FieldColumn の labelWidth で決める。
 *   縦に並ぶ FieldLabel 同士でラベルの幅がそろっていないと読みにくいため。最適な幅は画面ごとに異なるので画面側で指定できるようにしている。
 * - 画面の幅が足りず列を横に並べられない場合は、列を縦に積む。左の列が上になるので、鏡面N字と同じ読み順のまま1列になる。
 *
 * フォームとは結びつかないので、どのフォームの FieldLabel を並べてもよい。
 */
export function FieldGroup(props: FieldGroupProps): React.ReactNode {
  const sections = splitIntoSections(props.children)

  return (
    <section className={`flex flex-col gap-2 ${props.className ?? ''}`} style={labelWidthStyle(props.labelWidth)}>
      {/* 見出し */}
      {props.title && (
        <h3 className="font-bold text-gray-700 border-b border-gray-300 select-none">
          {props.title}
        </h3>
      )}

      {/* 列の並びと、横幅いっぱいの項目 */}
      {sections.map(section => section.kind === 'columns' ? (
        // 列の並び。全列を横に並べられる幅があれば横に、無ければすべて縦に積む。
        // 途中で折り返すと鏡面N字の読み順が崩れるため、各列の flex-basis を
        // 「全列を並べるのに必要な幅 − 実際の幅」の大きな倍数にして、全列横並びか全列縦積みのどちらかにしかならないようにしている
        <div
          key={section.key}
          className="flex flex-wrap gap-x-8 gap-y-1"
          style={{ '--field-group-threshold': `${section.columns.length * COLUMN_MIN_WIDTH_REM}rem` } as React.CSSProperties}
        >
          {section.columns}
        </div>
      ) : (
        <React.Fragment key={section.key}>
          {section.item}
        </React.Fragment>
      ))}
    </section>
  )
}

export type FieldColumnProps = {
  /** この列に上から順に並べる FieldLabel */
  children?: React.ReactNode
  /**
   * この列の、横並びの FieldLabel のラベル部分の幅。CSS の長さで指定する（例: `'8rem'`）。
   * 未指定の場合は FieldGroup の labelWidth、それも無ければ既定の幅になる。
   */
  labelWidth?: string
}

/**
 * FieldGroup の中の1列。中の FieldLabel を上から下へ並べる。
 * FieldGroup の直下に置くこと。それ以外の場所に置いた場合の配置は保証しない。
 */
export function FieldColumn(props: FieldColumnProps): React.ReactNode {
  return (
    // 列自体をラベルと中身の2列のグリッドにし、各 FieldLabel はその行になる
    <div className="grow min-w-0 grid grid-cols-[max-content_minmax(0,1fr)] gap-x-2 gap-y-1 content-start" style={{ ...COLUMN_STYLE, ...labelWidthStyle(props.labelWidth) }}>
      <FieldColumnContext.Provider value={true}>
        {props.children}
      </FieldColumnContext.Provider>
    </div>
  )
}

/**
 * ui フォルダ内部でのみ使用する。
 * FieldColumn の中かどうかを返す。中なら、FieldLabel は FieldColumn のグリッドの1行として配置される。
 */
export function useIsInFieldColumn(): boolean {
  return React.useContext(FieldColumnContext)
}

const FieldColumnContext = React.createContext(false)

/**
 * ui フォルダ内部でのみ使用する。
 * 横並びの FieldLabel のラベル部分に付けるスタイル。囲んでいる FieldGroup・FieldColumn の labelWidth の幅になる。
 */
export const LABEL_PART_WIDTH_STYLE: React.CSSProperties = {
  width: 'var(--field-label-width, 7rem)',
}

/**
 * labelWidth を、中の FieldLabel に CSS 変数で伝えるスタイル。未指定なら何もしない。
 * CSS 変数は内側の要素に引き継がれ、より内側で指定したものが優先されるので、FieldColumn の指定が FieldGroup の指定より優先される
 */
function labelWidthStyle(labelWidth: string | undefined): React.CSSProperties | undefined {
  return labelWidth === undefined
    ? undefined
    : { '--field-label-width': labelWidth } as React.CSSProperties
}

/** 1列の最小の幅。列の数とこの幅の積より FieldGroup が狭いと、列を縦に積む */
const COLUMN_MIN_WIDTH_REM = 20

/**
 * 列の flex-basis。FieldGroup が全列を並べるのに必要な幅より広ければ負（= 0 扱い）になって全列が等幅で横に並び、
 * 狭ければ非常に大きくなって各列が1行を占める
 */
const COLUMN_STYLE: React.CSSProperties = {
  flexBasis: 'calc((var(--field-group-threshold) - 100%) * 999)',
}

/** FieldGroup の直下の要素の区切り */
type Section
  = { kind: 'columns', key: React.Key, columns: React.ReactElement[] }
  | { kind: 'full-width', key: React.Key, item: React.ReactNode }

/** FieldGroup の直下の要素を、FieldColumn が連続する区間と、横幅いっぱいに表示する要素とに分ける */
function splitIntoSections(children: React.ReactNode): Section[] {
  const sections: Section[] = []
  React.Children.toArray(children).forEach((child, index) => {
    const key = React.isValidElement(child) && child.key !== null ? child.key : index
    const last = sections[sections.length - 1]
    if (React.isValidElement(child) && child.type === FieldColumn) {
      if (last?.kind === 'columns') {
        last.columns.push(child)
      } else {
        sections.push({ kind: 'columns', key, columns: [child] })
      }
    } else {
      sections.push({ kind: 'full-width', key, item: child })
    }
  })
  return sections
}
