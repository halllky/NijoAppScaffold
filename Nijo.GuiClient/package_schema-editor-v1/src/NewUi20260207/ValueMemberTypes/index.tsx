import { Allotment, LayoutPriority } from "allotment"
import React from "react"
import * as ReactHookForm from "react-hook-form"
import { ATTR_TYPE, SchemaDefinitionGlobalState, TYPE_STATIC_ENUM_MODEL } from "../../types"
import StaticEnumGrid from "./StaticEnumGrid"

/**
 * 属性種類定義画面。
 *
 * Nijo の ValueMemberType に相当する属性種類を定義する。
 */
export default function (props: {
  formMethods?: ReactHookForm.UseFormReturn<SchemaDefinitionGlobalState>
}) {
  const { control } = props.formMethods ?? {}
  const xmlElementTrees = control ? ReactHookForm.useWatch({
    control,
    name: "xmlElementTrees"
  }) : []

  // 静的区分の一覧を取得
  const enumItems = React.useMemo(() => {
    return (xmlElementTrees ?? [])
      .map(tree => tree.xmlElements[0])
      .filter(el => el?.attributes?.[ATTR_TYPE] === TYPE_STATIC_ENUM_MODEL)
      .map(el => ({
        uniqueId: el.uniqueId,
        localName: el.localName,
      }))
  }, [xmlElementTrees])

  return (
    <Allotment proportionalLayout={false} separator={false}>

      {/* 目次 */}
      <Allotment.Pane preferredSize={200} className="border-r border-gray-400">
        <div className="h-full w-full overflow-y-auto bg-gray-50 p-2">

          <SideMenuLink hash="string-values">
            文字列系項目
          </SideMenuLink>
          <SideMenuLink hash="numeric-values">
            数値系項目
          </SideMenuLink>
          <SideMenuLink hash="date-values">
            日付系項目
          </SideMenuLink>
          <SideMenuLink hash="selection-values">
            選択・区分系項目
          </SideMenuLink>

          {/* 静的区分一覧へのリンク */}
          <div className="ml-2 flex flex-col items-stretch border-l border-gray-300 gap-1 pb-4">
            {enumItems.map(item => (
              <button
                type="button"
                key={item.uniqueId}
                onClick={() => {
                  const el = document.getElementById(`enum-def-${item.uniqueId}`)
                  if (el) el.scrollIntoView({ block: 'center', behavior: 'smooth' })
                }}
                className="pl-2 py-0.5 w-full text-left hover:bg-gray-200 dark:hover:bg-gray-700 cursor-pointer select-none"
              >
                <div className="text-xs truncate max-w-full" title={item.localName}>
                  {item.localName || '(名前なし)'}
                </div>
              </button>
            ))}
          </div>

          <SideMenuLink hash="other-values">
            その他
          </SideMenuLink>

        </div>
      </Allotment.Pane>

      {/* コンテンツ */}
      <Allotment.Pane priority={LayoutPriority.High}>
        <div className="h-full w-full overflow-y-auto px-4 py-2">

          <section>
            <h2 id="string-values" className="text-2xl font-bold">文字列系項目</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">word (単語型):</strong>
                ものの名前など改行を含まない単語を表す。
              </li>
              <li>
                <strong className="mr-2">description (文章型):</strong>
                コメントや備考などフリーフォーマットの文章。
              </li>
            </ul>
          </section>

          <section className="mt-8">
            <h2 id="numeric-values" className="text-2xl font-bold">数値系項目</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">int (整数型):</strong>
                数量、回数、順序番号などの整数。
              </li>
              <li>
                <strong className="mr-2">decimal (実数型):</strong>
                金額、重量、割合などの小数を含む数値。
              </li>
              <li>
                <strong className="mr-2">sequence (採番型):</strong>
                DB登録時に自動採番される整数。
              </li>
            </ul>
          </section>

          <section className="mt-8">
            <h2 id="date-values" className="text-2xl font-bold">日付系項目</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">date (日付型):</strong>
                年月日。時刻は含まない。
              </li>
              <li>
                <strong className="mr-2">datetime (日時型):</strong>
                年月日と時刻。
              </li>
              <li>
                <strong className="mr-2">year (年型):</strong>
                西暦年。
              </li>
              <li>
                <strong className="mr-2">year-month (年月型):</strong>
                年月。
              </li>
            </ul>
          </section>

          <section className="mt-8">
            <h2 id="selection-values" className="text-2xl font-bold">選択・区分系項目</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">bool (真偽値型):</strong>
                はい/いいえ (True/False)。
              </li>
              <li>
                <strong className="mr-2">静的区分:</strong>
              </li>
            </ul>

            {props.formMethods && (
              <StaticEnumGrid formMethods={props.formMethods} />
            )}

          </section>

          <section className="mt-8 pb-96">
            <h2 id="other-values" className="text-2xl font-bold">その他</h2>

            <ul className="list-disc list-inside my-2 space-y-1">
              <li>
                <strong className="mr-2">bytearray (バイナリ型):</strong>
                ハッシュ化されたパスワードなどのバイナリデータ。
              </li>
            </ul>
          </section>

        </div>
      </Allotment.Pane>
    </Allotment>
  )
}

/**
 * 目次のリンク
 */
function SideMenuLink({ hash, children }: {
  hash: string
  children?: React.ReactNode
}) {

  const handleClick = () => {
    const el = document.getElementById(hash)
    if (el) el.scrollIntoView({ block: 'start', behavior: 'smooth' })
  }

  return (
    <button type="button" onClick={handleClick} className="px-2 py-1 w-full text-sm text-left truncate hover:bg-gray-200 dark:hover:bg-gray-700 cursor-pointer select-none">
      {children}
    </button>
  )
}
