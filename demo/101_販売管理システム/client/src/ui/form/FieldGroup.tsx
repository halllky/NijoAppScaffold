import React from "react"

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
 * - 列の幅はすべて等しい。
 * - 横並びの FieldLabel のラベル部分の幅は、この中のすべての横並びの FieldLabel でそろえる。
 *   幅は表示名の長さから自動で決まる（最も長いラベルに合わせる。ただし上限を超える表示名は折り返す）。
 *   列を縦に積んだときに上下の列でラベルの幅がそろっていないと読みにくいため、列ごとではなく全体でそろえる。
 * - 全列を横に並べられる幅が無い場合は、列を縦に積む。左の列が上になるので、鏡面N字と同じ読み順のまま1列になる。
 *   横に並べられるかどうかは、ラベル部分の幅と値部分の最小幅から決まる。値部分の最小幅は中身によらず一律で、中身はその幅まで縮むものとして扱う。
 *
 * フォームとは結びつかないので、どのフォームの FieldLabel を並べてもよい。
 */
export function FieldGroup(props: FieldGroupProps): React.ReactNode {
  const sections = splitIntoSections(props.children)
  const [labelWidth, registerLabelPart] = useMaxLabelPartWidth()
  const contextValue = React.useMemo<FieldGroupContextValue>(() => ({ registerLabelPart, isInFieldColumn: false }), [registerLabelPart])

  // ラベル部分の幅は、中の FieldColumn と FieldLabel に CSS 変数で伝える
  const style = labelWidth === undefined
    ? undefined
    : { '--field-label-width': `${labelWidth}px` } as React.CSSProperties

  return (
    <section className={`flex flex-col gap-2 ${props.className ?? ''}`} style={style}>
      {/* 見出し */}
      {props.title && (
        <h3 className="font-bold text-gray-700 border-b border-gray-300 select-none">
          {props.title}
        </h3>
      )}

      {/* 列の並びと、横幅いっぱいの項目 */}
      <FieldGroupContext.Provider value={contextValue}>
        {sections.map(section => section.kind === 'columns' ? (
          // 列の並び。全列を横に並べられる幅があれば横に、無ければすべて縦に積む。
          // 途中で折り返すと鏡面N字の読み順が崩れるため、各列の flex-basis を
          // 「全列を並べるのに必要な幅 − 実際の幅」の大きな倍数にして、全列横並びか全列縦積みのどちらかにしかならないようにしている
          <div
            key={section.key}
            className="flex flex-wrap gap-x-8 gap-y-1"
            style={{ '--field-group-threshold': columnsThreshold(section.columns.length) } as React.CSSProperties}
          >
            {section.columns}
          </div>
        ) : (
          <React.Fragment key={section.key}>
            {section.item}
          </React.Fragment>
        ))}
      </FieldGroupContext.Provider>
    </section>
  )
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
  const group = React.useContext(FieldGroupContext)
  const contextValue = React.useMemo(() => group && { ...group, isInFieldColumn: true }, [group])

  return (
    // 列自体をラベルと中身の2列のグリッドにし、各 FieldLabel はその行になる
    <div className="grow min-w-0 grid grid-cols-[var(--field-label-width,max-content)_minmax(0,1fr)] gap-x-2 gap-y-1 content-start" style={COLUMN_STYLE}>
      <FieldGroupContext.Provider value={contextValue}>
        {props.children}
      </FieldGroupContext.Provider>
    </div>
  )
}

/**
 * ui フォルダ内部でのみ使用する。
 * FieldLabel を、囲んでいる FieldGroup・FieldColumn に合わせて配置するためのもの。
 *
 * - isInFieldColumn: FieldColumn の中かどうか。中なら、FieldLabel は FieldColumn のグリッドの1行として配置される。
 * - labelPartProps: 横並びの FieldLabel のラベル部分に渡す。
 *   ラベル部分は表示名の長さなりの幅（上限あり）になり、FieldGroup はその最大値を、そろえた幅として CSS 変数で中に伝える。
 *   FieldColumn の中ではラベル部分を置く列の幅は FieldColumn が決める。FieldColumn の外では、利用側がその CSS 変数の幅にすること。
 */
export function useFieldLabelLayout(): {
  isInFieldColumn: boolean
  labelPartProps: { ref: React.RefCallback<HTMLElement>, style: React.CSSProperties }
} {
  const group = React.useContext(FieldGroupContext)
  const ref = React.useCallback<React.RefCallback<HTMLElement>>(element => {
    if (group && element) return group.registerLabelPart(element)
  }, [group])

  return {
    isInFieldColumn: group?.isInFieldColumn ?? false,
    labelPartProps: { ref, style: LABEL_PART_STYLE },
  }
}

type FieldGroupContextValue = {
  /** 幅をそろえる対象のラベル部分を登録する。戻り値は登録の解除 */
  registerLabelPart: (element: HTMLElement) => () => void
  /** FieldColumn の中かどうか */
  isInFieldColumn: boolean
}

/** FieldGroup の外では null */
const FieldGroupContext = React.createContext<FieldGroupContextValue | null>(null)

/** ラベル部分の幅の上限。これより長い表示名は折り返す */
const LABEL_MAX_WIDTH_REM = 12
/** 値部分の最小幅。列の幅がこれを下回る場合は列を縦に積む */
const VALUE_MIN_WIDTH_REM = 12
/** ラベル部分と値部分の間隔。FieldColumn と FieldLabel の gap-x-2 と一致させること */
const LABEL_VALUE_GAP_REM = 0.5
/** 列同士の間隔。列の並びの gap-x-8 と一致させること */
const COLUMN_GAP_REM = 2

/**
 * ラベル部分のスタイル。幅は親の幅によらず表示名の長さなり（上限あり）になる。
 * 親の方が狭くても縮まないのは、縮むと測った幅が親の幅に引きずられ、より長い表示名が加わったときに広がらなくなるため
 */
const LABEL_PART_STYLE: React.CSSProperties = {
  width: 'max-content',
  maxWidth: `${LABEL_MAX_WIDTH_REM}rem`,
  flexShrink: 0,
}

/** 指定の数の列を横に並べるのに必要な幅 */
function columnsThreshold(columnCount: number): string {
  const columnMinWidth = `(var(--field-label-width, 0px) + ${LABEL_VALUE_GAP_REM + VALUE_MIN_WIDTH_REM}rem)`
  return `calc(${columnCount} * ${columnMinWidth} + ${(columnCount - 1) * COLUMN_GAP_REM}rem)`
}

/**
 * 列の flex-basis。FieldGroup が全列を並べるのに必要な幅より広ければ負（= 0 扱い）になって全列が等幅で横に並び、
 * 狭ければ非常に大きくなって各列が1行を占める
 */
const COLUMN_STYLE: React.CSSProperties = {
  flexBasis: 'calc((var(--field-group-threshold) - 100%) * 999)',
}

/**
 * 登録されたラベル部分の幅の最大値（px）と、ラベル部分を登録する関数を返す。登録が無ければ undefined。
 * ラベル部分の幅は表示名とフォントによってのみ変わり、FieldGroup の幅には左右されないので、幅を変えても測り直しは起きない。
 */
function useMaxLabelPartWidth(): [number | undefined, (element: HTMLElement) => () => void] {
  const [width, setWidth] = React.useState<number>()
  const elementsRef = React.useRef(new Set<HTMLElement>())
  const observerRef = React.useRef<ResizeObserver | null>(null)

  const measure = React.useCallback(() => {
    const widths = Array.from(elementsRef.current, element => element.getBoundingClientRect().width)
    // 小数点以下を切り捨てると、最も長い表示名が収まらず折り返してしまうので切り上げる
    setWidth(widths.length === 0 ? undefined : Math.ceil(Math.max(...widths)))
  }, [])

  // 登録の時点で測るのは、最初の描画の前に幅を確定させ、ラベルの幅が変わるちらつきを防ぐため
  const register = React.useCallback((element: HTMLElement) => {
    elementsRef.current.add(element)
    observerRef.current?.observe(element)
    measure()
    return () => {
      elementsRef.current.delete(element)
      observerRef.current?.unobserve(element)
      measure()
    }
  }, [measure])

  // 表示名やフォントが変わったときに測り直す
  React.useEffect(() => {
    const observer = new ResizeObserver(measure)
    elementsRef.current.forEach(element => observer.observe(element))
    observerRef.current = observer
    return () => {
      observer.disconnect()
      observerRef.current = null
    }
  }, [measure])

  return [width, register]
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
